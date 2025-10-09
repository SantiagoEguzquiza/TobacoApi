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
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IVentaPagosService _ventaPagosService;
        private readonly IPrecioEspecialService _precioEspecialService;
        private readonly IClienteService _clienteService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PricingService _pricingService;

        public PedidoService(IPedidoRepository pedidoRepository, IMapper mapper, IProductoRepository productoRepository, IVentaPagosService ventaPagosService, IPrecioEspecialService precioEspecialService, IClienteService clienteService, IHttpContextAccessor httpContextAccessor, PricingService pricingService)
        {
            _pedidoRepository = pedidoRepository;
            _mapper = mapper;
            _productoRepository = productoRepository;
            _ventaPagosService = ventaPagosService;
            _precioEspecialService = precioEspecialService;
            _clienteService = clienteService;
            _httpContextAccessor = httpContextAccessor;
            _pricingService = pricingService;
        }

        public async Task AddPedido(PedidoDTO pedidoDto)
        {
            // Obtener el ID del usuario actual del contexto de autenticación
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            int? usuarioId = null;
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                usuarioId = userId;
            }

            var pedido = new Pedido
            {
                ClienteId = pedidoDto.ClienteId,
                Fecha = DateTime.Now,
                PedidoProductos = new List<PedidoProducto>(),
                MetodoPago = pedidoDto.MetodoPago,
                UsuarioId = usuarioId
            };

            decimal total = 0;

            foreach (var productoDto in pedidoDto.PedidoProductos)
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
                    specialPrice = await _precioEspecialService.GetPrecioFinalProductoAsync(pedidoDto.ClienteId, producto.Id);
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

                var pedidoProducto = new PedidoProducto
                {
                    ProductoId = producto.Id,
                    Cantidad = productoDto.Cantidad,
                    PrecioFinalCalculado = pricingResult.TotalPrice // Precio después de descuentos por cantidad
                };

                total += pricingResult.TotalPrice;

                pedido.PedidoProductos.Add(pedidoProducto);
            }

            // Aplicar descuento global del cliente si existe
            var cliente = await _clienteService.GetClienteById(pedidoDto.ClienteId);
            decimal descuentoGlobal = 0;
            if (cliente != null && cliente.DescuentoGlobal > 0)
            {
                descuentoGlobal = total * (cliente.DescuentoGlobal / 100);
                total = total - descuentoGlobal;
            }

            pedido.Total = total;

            // Calcular y asignar precios finales a cada producto después del descuento global
            if (descuentoGlobal > 0)
            {
                foreach (var pedidoProducto in pedido.PedidoProductos)
                {
                    // Calcular la proporción de este producto en el total antes del descuento global
                    var precioAntesDescuentoGlobal = pedidoProducto.PrecioFinalCalculado;
                    var proporcionProducto = precioAntesDescuentoGlobal / (total + descuentoGlobal);
                    
                    // Aplicar el descuento global proporcionalmente a este producto
                    var descuentoProducto = descuentoGlobal * proporcionProducto;
                    pedidoProducto.PrecioFinalCalculado = precioAntesDescuentoGlobal - descuentoProducto;
                }
            }

            // Add the pedido first to get the ID
            await _pedidoRepository.AddPedido(pedido);

            // Add VentaPagos if provided
            if (pedidoDto.VentaPagos != null && pedidoDto.VentaPagos.Any())
            {
                foreach (var ventaPagoDto in pedidoDto.VentaPagos)
                {
                    ventaPagoDto.PedidoId = pedido.Id;
                    await _ventaPagosService.AddVentaPagos(ventaPagoDto);
                }
            }

            // Si el método de pago es CuentaCorriente, sumar SOLO la parte pagada con cuenta corriente
            if (pedido.MetodoPago == MetodoPagoEnum.CuentaCorriente)
            {
                // Calcular el monto pagado con cuenta corriente
                decimal montoCuentaCorriente = 0;
                if (pedidoDto.VentaPagos != null && pedidoDto.VentaPagos.Any())
                {
                    montoCuentaCorriente = pedidoDto.VentaPagos
                        .Where(vp => vp.Metodo == MetodoPagoEnum.CuentaCorriente)
                        .Sum(vp => vp.Monto);
                }
                
                // Solo agregar a la deuda si hay monto pagado con cuenta corriente
                if (montoCuentaCorriente > 0)
                {
                    await _clienteService.AgregarDeuda(pedido.ClienteId, montoCuentaCorriente);
                }
            }
        }




        public async Task<bool> DeletePedido(int id)
        {
            // Get the pedido before deleting to check if it affects debt
            var pedidoExistente = await _pedidoRepository.GetPedidoById(id);
            if (pedidoExistente == null)
            {
                return false; // Pedido no encontrado
            }

            // Calculate the debt amount that needs to be reduced
            decimal montoDeudaAReducir = 0;
            if (pedidoExistente.MetodoPago == MetodoPagoEnum.CuentaCorriente)
            {
                // Get the actual amount paid with cuenta corriente from existing VentaPagos
                var ventaPagosAnteriores = await _ventaPagosService.GetVentaPagosByPedidoId(id);
                montoDeudaAReducir = ventaPagosAnteriores
                    .Where(vp => vp.Metodo == MetodoPagoEnum.CuentaCorriente)
                    .Sum(vp => vp.Monto);
            }
            
            // Delete associated VentaPagos first
            await _ventaPagosService.DeleteVentaPagosByPedidoId(id);
            
            // Then delete the pedido
            bool eliminado = await _pedidoRepository.DeletePedido(id);
            
            // Reduce debt only if there was actual debt from cuenta corriente payments
            if (eliminado && montoDeudaAReducir > 0)
            {
                await _clienteService.ReducirDeuda(pedidoExistente.ClienteId, montoDeudaAReducir);
            }
            
            return eliminado;
        }

        public async Task<List<PedidoDTO>> GetAllPedidos()
        {
            var pedidos = await _pedidoRepository.GetAllPedidos();
            var pedidosDto = _mapper.Map<List<PedidoDTO>>(pedidos);
            
            // Load VentaPagos for each pedido
            foreach (var pedidoDto in pedidosDto)
            {
                pedidoDto.VentaPagos = await _ventaPagosService.GetVentaPagosByPedidoId(pedidoDto.Id);
            }
            
            return pedidosDto;
        }

        public async Task<PedidoDTO> GetPedidoById(int id)
        {
            var pedido = await _pedidoRepository.GetPedidoById(id);
            var pedidoDto = _mapper.Map<PedidoDTO>(pedido);
            
            // Load VentaPagos for this pedido
            if (pedido != null)
            {
                pedidoDto.VentaPagos = await _ventaPagosService.GetVentaPagosByPedidoId(id);
            }
            
            return pedidoDto;
        }

        public async Task UpdatePedido(int id, PedidoDTO pedidoDto)
        {
            // Get the existing pedido to compare changes
            var pedidoExistente = await _pedidoRepository.GetPedidoById(id);

            // Calculate the new total
            decimal nuevoTotal = 0;
            if (pedidoDto.PedidoProductos != null)
            {
                foreach (var productoDto in pedidoDto.PedidoProductos)
                {
                    var producto = await _productoRepository.GetProductoById(productoDto.ProductoId);
                    if (producto != null)
                    {
                        var precioFinal = await _precioEspecialService.GetPrecioFinalProductoAsync(pedidoDto.ClienteId, producto.Id);
                        nuevoTotal += precioFinal * productoDto.Cantidad;
                    }
                }
            }

            // Apply global discount if exists
            var cliente = await _clienteService.GetClienteById(pedidoDto.ClienteId);
            if (cliente != null && cliente.DescuentoGlobal > 0)
            {
                var descuento = nuevoTotal * (cliente.DescuentoGlobal / 100);
                nuevoTotal = nuevoTotal - descuento;
            }

            // Calculate the current debt amount from the existing pedido
            decimal deudaAnterior = 0;
            if (pedidoExistente.MetodoPago == MetodoPagoEnum.CuentaCorriente)
            {
                // Get the actual amount paid with cuenta corriente from existing VentaPagos
                var ventaPagosAnteriores = await _ventaPagosService.GetVentaPagosByPedidoId(id);
                deudaAnterior = ventaPagosAnteriores
                    .Where(vp => vp.Metodo == MetodoPagoEnum.CuentaCorriente)
                    .Sum(vp => vp.Monto);
            }

            // Calculate the new debt amount from the new pedido
            decimal deudaNueva = 0;
            if (pedidoDto.MetodoPago == MetodoPagoEnum.CuentaCorriente && pedidoDto.VentaPagos != null)
            {
                deudaNueva = pedidoDto.VentaPagos
                    .Where(vp => vp.Metodo == MetodoPagoEnum.CuentaCorriente)
                    .Sum(vp => vp.Monto);
            }

            // Adjust debt based on the difference
            var diferenciaDeuda = deudaNueva - deudaAnterior;
            if (diferenciaDeuda > 0)
            {
                await _clienteService.AgregarDeuda(pedidoDto.ClienteId, diferenciaDeuda);
            }
            else if (diferenciaDeuda < 0)
            {
                await _clienteService.ReducirDeuda(pedidoDto.ClienteId, Math.Abs(diferenciaDeuda));
            }

            var pedido = _mapper.Map<Pedido>(pedidoDto);
            pedido.Id = id;
            pedido.Total = nuevoTotal;
            await _pedidoRepository.UpdatePedido(pedido);
            
            // Update VentaPagos
            if (pedidoDto.VentaPagos != null)
            {
                // Delete existing VentaPagos for this pedido
                await _ventaPagosService.DeleteVentaPagosByPedidoId(id);
                
                // Add new VentaPagos
                foreach (var ventaPagoDto in pedidoDto.VentaPagos)
                {
                    ventaPagoDto.PedidoId = id;
                    await _ventaPagosService.AddVentaPagos(ventaPagoDto);
                }
            }
        }

        public async Task<object> GetPedidosPaginados(int page, int pageSize)
        {
            var result = await _pedidoRepository.GetPedidosPaginados(page, pageSize);
            var pedidosDto = _mapper.Map<List<PedidoDTO>>(result.Pedidos);
            
            // Load VentaPagos for each pedido
            foreach (var pedidoDto in pedidosDto)
            {
                pedidoDto.VentaPagos = await _ventaPagosService.GetVentaPagosByPedidoId(pedidoDto.Id);
            }
            
            return new
            {
                pedidos = pedidosDto,
                totalItems = result.TotalItems,
                totalPages = result.TotalPages,
                currentPage = page,
                pageSize = pageSize,
                hasNextPage = page < result.TotalPages,
                hasPreviousPage = page > 1
            };
        }

        public async Task<object> GetPedidosPorCliente(int clienteId, int page, int pageSize, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var result = await _pedidoRepository.GetPedidosPorCliente(clienteId, page, pageSize, dateFrom, dateTo);
            var pedidosDto = _mapper.Map<List<PedidoDTO>>(result.Pedidos);
            
            // Load VentaPagos for each pedido
            foreach (var pedidoDto in pedidosDto)
            {
                pedidoDto.VentaPagos = await _ventaPagosService.GetVentaPagosByPedidoId(pedidoDto.Id);
            }
            
            return new
            {
                pedidos = pedidosDto,
                totalItems = result.TotalItems,
                totalPages = result.TotalPages,
                currentPage = page,
                pageSize = pageSize,
                hasNextPage = page < result.TotalPages,
                hasPreviousPage = page > 1
            };
        }

        public async Task<object> GetPedidosConCuentaCorrienteByClienteId(int clienteId, int page = 1, int pageSize = 20)
        {
            var result = await _pedidoRepository.GetPedidosConCuentaCorrienteByClienteId(clienteId, page, pageSize);
            var pedidosDto = _mapper.Map<List<PedidoDTO>>(result.Pedidos);

            return new
            {
                pedidos = pedidosDto,
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
