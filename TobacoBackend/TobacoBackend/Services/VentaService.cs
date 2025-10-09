using AutoMapper;
using System.Security.Claims;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace TobacoBackend.Services
{
    public class VentaService : IVentaService
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IVentaPagoService _ventaPagoService;
        private readonly IPrecioEspecialService _precioEspecialService;
        private readonly IClienteService _clienteService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PricingService _pricingService;

        public VentaService(IVentaRepository ventaRepository, IMapper mapper, IProductoRepository productoRepository, IVentaPagoService ventaPagoService, IPrecioEspecialService precioEspecialService, IClienteService clienteService, IHttpContextAccessor httpContextAccessor, PricingService pricingService)
        {
            _ventaRepository = ventaRepository;
            _mapper = mapper;
            _productoRepository = productoRepository;
            _ventaPagoService = ventaPagoService;
            _precioEspecialService = precioEspecialService;
            _clienteService = clienteService;
            _httpContextAccessor = httpContextAccessor;
            _pricingService = pricingService;
        }

        public async Task AddVenta(VentaDTO ventaDto)
        {
            // Obtener el ID del usuario actual del contexto de autenticación
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            int? usuarioId = null;
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                usuarioId = userId;
            }

            var venta = new Venta
            {
                ClienteId = ventaDto.ClienteId,
                Fecha = DateTime.Now,
                VentaProductos = new List<VentaProducto>(),
                MetodoPago = ventaDto.MetodoPago,
                UsuarioId = usuarioId
            };

            decimal total = 0;

            foreach (var productoDto in ventaDto.VentaProductos)
            {
                var producto = await _productoRepository.GetProductoById(productoDto.ProductoId);
                if (producto == null)
                {
                    throw new Exception($"Producto con ID {productoDto.ProductoId} no encontrado.");
                }

                // Obtener el precio especial si existe
                decimal? specialPrice = null;
                try
                {
                    specialPrice = await _precioEspecialService.GetPrecioFinalProductoAsync(ventaDto.ClienteId, producto.Id);
                }
                catch
                {
                    // No special price available, use regular pricing
                }

                // Calculate optimal pricing using quantity-based pricing
                var pricingResult = _pricingService.CalculateOptimalPricing(
                    producto, 
                    (int)productoDto.Cantidad, 
                    specialPrice, 
                    null // Global discount will be applied later
                );

                var ventaProducto = new VentaProducto
                {
                    ProductoId = producto.Id,
                    Cantidad = productoDto.Cantidad,
                    PrecioFinalCalculado = pricingResult.TotalPrice // Precio después de descuentos por cantidad
                };

                total += pricingResult.TotalPrice;

                venta.VentaProductos.Add(ventaProducto);
            }

            // Aplicar descuento global del cliente si existe
            var cliente = await _clienteService.GetClienteById(ventaDto.ClienteId);
            decimal descuentoGlobal = 0;
            if (cliente != null && cliente.DescuentoGlobal > 0)
            {
                descuentoGlobal = total * (cliente.DescuentoGlobal / 100);
                total = total - descuentoGlobal;
            }

            venta.Total = total;

            // Calcular y asignar precios finales a cada producto después del descuento global
            if (descuentoGlobal > 0)
            {
                foreach (var ventaProducto in venta.VentaProductos)
                {
                    // Calcular la proporción de este producto en el total antes del descuento global
                    var precioAntesDescuentoGlobal = ventaProducto.PrecioFinalCalculado;
                    var proporcionProducto = precioAntesDescuentoGlobal / (total + descuentoGlobal);
                    
                    // Aplicar el descuento global proporcionalmente a este producto
                    var descuentoProducto = descuentoGlobal * proporcionProducto;
                    ventaProducto.PrecioFinalCalculado = precioAntesDescuentoGlobal - descuentoProducto;
                }
            }

            // Add the venta first to get the ID
            await _ventaRepository.AddVenta(venta);

            // Add VentaPagos if provided
            if (ventaDto.VentaPagos != null && ventaDto.VentaPagos.Any())
            {
                foreach (var ventaPagoDto in ventaDto.VentaPagos)
                {
                    ventaPagoDto.VentaId = venta.Id;
                    await _ventaPagoService.AddVentaPago(ventaPagoDto);
                }
            }

            // Si el método de pago es CuentaCorriente, sumar SOLO la parte pagada con cuenta corriente
            if (venta.MetodoPago == MetodoPagoEnum.CuentaCorriente)
            {
                // Calcular el monto pagado con cuenta corriente
                decimal montoCuentaCorriente = 0;
                if (ventaDto.VentaPagos != null && ventaDto.VentaPagos.Any())
                {
                    montoCuentaCorriente = ventaDto.VentaPagos
                        .Where(vp => vp.Metodo == MetodoPagoEnum.CuentaCorriente)
                        .Sum(vp => vp.Monto);
                }
                
                // Solo agregar a la deuda si hay monto pagado con cuenta corriente
                if (montoCuentaCorriente > 0)
                {
                    await _clienteService.AgregarDeuda(venta.ClienteId, montoCuentaCorriente);
                }
            }
        }




        public async Task<bool> DeleteVenta(int id)
        {
            // Get the venta before deleting to check if it affects debt
            var ventaExistente = await _ventaRepository.GetVentaById(id);
            if (ventaExistente == null)
            {
                return false; // Venta no encontrada
            }

            // Calculate the debt amount that needs to be reduced
            decimal montoDeudaAReducir = 0;
            if (ventaExistente.MetodoPago == MetodoPagoEnum.CuentaCorriente)
            {
                // Get the actual amount paid with cuenta corriente from existing VentaPagos
                var ventaPagosAnteriores = await _ventaPagoService.GetVentaPagosByVentaId(id);
                montoDeudaAReducir = ventaPagosAnteriores
                    .Where(vp => vp.Metodo == MetodoPagoEnum.CuentaCorriente)
                    .Sum(vp => vp.Monto);
            }
            
            // Delete associated VentaPagos first
            await _ventaPagoService.DeleteVentaPagosByVentaId(id);
            
            // Then delete the venta
            bool eliminado = await _ventaRepository.DeleteVenta(id);
            
            // Reduce debt only if there was actual debt from cuenta corriente payments
            if (eliminado && montoDeudaAReducir > 0)
            {
                await _clienteService.ReducirDeuda(ventaExistente.ClienteId, montoDeudaAReducir);
            }
            
            return eliminado;
        }

        public async Task<List<VentaDTO>> GetAllVentas()
        {
            var ventas = await _ventaRepository.GetAllVentas();
            var ventasDto = _mapper.Map<List<VentaDTO>>(ventas);
            
            // Load VentaPagos for each venta
            foreach (var ventaDto in ventasDto)
            {
                ventaDto.VentaPagos = await _ventaPagoService.GetVentaPagosByVentaId(ventaDto.Id);
            }
            
            return ventasDto;
        }

        public async Task<VentaDTO> GetVentaById(int id)
        {
            var venta = await _ventaRepository.GetVentaById(id);
            var ventaDto = _mapper.Map<VentaDTO>(venta);
            
            // Load VentaPagos for this venta
            if (venta != null)
            {
                ventaDto.VentaPagos = await _ventaPagoService.GetVentaPagosByVentaId(id);
            }
            
            return ventaDto;
        }

        public async Task UpdateVenta(int id, VentaDTO ventaDto)
        {
            // Get the existing venta to compare changes
            var ventaExistente = await _ventaRepository.GetVentaById(id);

            // Calculate the new total
            decimal nuevoTotal = 0;
            if (ventaDto.VentaProductos != null)
            {
                foreach (var productoDto in ventaDto.VentaProductos)
                {
                    var producto = await _productoRepository.GetProductoById(productoDto.ProductoId);
                    if (producto != null)
                    {
                        var precioFinal = await _precioEspecialService.GetPrecioFinalProductoAsync(ventaDto.ClienteId, producto.Id);
                        nuevoTotal += precioFinal * productoDto.Cantidad;
                    }
                }
            }

            // Apply global discount if exists
            var cliente = await _clienteService.GetClienteById(ventaDto.ClienteId);
            if (cliente != null && cliente.DescuentoGlobal > 0)
            {
                var descuento = nuevoTotal * (cliente.DescuentoGlobal / 100);
                nuevoTotal = nuevoTotal - descuento;
            }

            // Calculate the current debt amount from the existing venta
            decimal deudaAnterior = 0;
            if (ventaExistente.MetodoPago == MetodoPagoEnum.CuentaCorriente)
            {
                // Get the actual amount paid with cuenta corriente from existing VentaPagos
                var ventaPagosAnteriores = await _ventaPagoService.GetVentaPagosByVentaId(id);
                deudaAnterior = ventaPagosAnteriores
                    .Where(vp => vp.Metodo == MetodoPagoEnum.CuentaCorriente)
                    .Sum(vp => vp.Monto);
            }

            // Calculate the new debt amount from the new venta
            decimal deudaNueva = 0;
            if (ventaDto.MetodoPago == MetodoPagoEnum.CuentaCorriente && ventaDto.VentaPagos != null)
            {
                deudaNueva = ventaDto.VentaPagos
                    .Where(vp => vp.Metodo == MetodoPagoEnum.CuentaCorriente)
                    .Sum(vp => vp.Monto);
            }

            // Adjust debt based on the difference
            var diferenciaDeuda = deudaNueva - deudaAnterior;
            if (diferenciaDeuda > 0)
            {
                await _clienteService.AgregarDeuda(ventaDto.ClienteId, diferenciaDeuda);
            }
            else if (diferenciaDeuda < 0)
            {
                await _clienteService.ReducirDeuda(ventaDto.ClienteId, Math.Abs(diferenciaDeuda));
            }

            var venta = _mapper.Map<Venta>(ventaDto);
            venta.Id = id;
            venta.Total = nuevoTotal;
            await _ventaRepository.UpdateVenta(venta);
            
            // Update VentaPagos
            if (ventaDto.VentaPagos != null)
            {
                // Delete existing VentaPagos for this venta
                await _ventaPagoService.DeleteVentaPagosByVentaId(id);
                
                // Add new VentaPagos
                foreach (var ventaPagoDto in ventaDto.VentaPagos)
                {
                    ventaPagoDto.VentaId = id;
                    await _ventaPagoService.AddVentaPago(ventaPagoDto);
                }
            }
        }

        public async Task<object> GetVentasPaginadas(int page, int pageSize)
        {
            var result = await _ventaRepository.GetVentasPaginadas(page, pageSize);
            var ventasDto = _mapper.Map<List<VentaDTO>>(result.Ventas);
            
            // Load VentaPagos for each venta
            foreach (var ventaDto in ventasDto)
            {
                ventaDto.VentaPagos = await _ventaPagoService.GetVentaPagosByVentaId(ventaDto.Id);
            }
            
            return new
            {
                ventas = ventasDto,
                totalItems = result.TotalItems,
                totalPages = result.TotalPages,
                currentPage = page,
                pageSize = pageSize,
                hasNextPage = page < result.TotalPages,
                hasPreviousPage = page > 1
            };
        }

        public async Task<object> GetVentasPorCliente(int clienteId, int page, int pageSize, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var result = await _ventaRepository.GetVentasPorCliente(clienteId, page, pageSize, dateFrom, dateTo);
            var ventasDto = _mapper.Map<List<VentaDTO>>(result.Ventas);
            
            // Load VentaPagos for each venta
            foreach (var ventaDto in ventasDto)
            {
                ventaDto.VentaPagos = await _ventaPagoService.GetVentaPagosByVentaId(ventaDto.Id);
            }
            
            return new
            {
                ventas = ventasDto,
                totalItems = result.TotalItems,
                totalPages = result.TotalPages,
                currentPage = page,
                pageSize = pageSize,
                hasNextPage = page < result.TotalPages,
                hasPreviousPage = page > 1
            };
        }

        public async Task<object> GetVentasConCuentaCorrienteByClienteId(int clienteId, int page = 1, int pageSize = 20)
        {
            var result = await _ventaRepository.GetVentasConCuentaCorrienteByClienteId(clienteId, page, pageSize);
            var ventasDto = _mapper.Map<List<VentaDTO>>(result.Ventas);

            return new
            {
                ventas = ventasDto,
                totalItems = result.TotalItems,
                totalPages = result.TotalPages,
                currentPage = page,
                pageSize = pageSize,
                hasNextPage = page < result.TotalPages,
                hasPreviousPage = page > 1
            };
        }
    }
}

