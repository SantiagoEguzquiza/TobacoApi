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
        private readonly AplicationDbContext _context;

        public PedidoService(IPedidoRepository pedidoRepository, IMapper mapper, IProductoRepository productoRepository, IVentaPagosService ventaPagosService, IPrecioEspecialService precioEspecialService, IClienteService clienteService, IHttpContextAccessor httpContextAccessor, AplicationDbContext context)
        {
            _pedidoRepository = pedidoRepository;
            _mapper = mapper;
            _productoRepository = productoRepository;
            _ventaPagosService = ventaPagosService;
            _precioEspecialService = precioEspecialService;
            _clienteService = clienteService;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
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

                // Obtener el precio final (especial si existe, estándar si no)
                var precioFinal = await _precioEspecialService.GetPrecioFinalProductoAsync(pedidoDto.ClienteId, producto.Id);

                var pedidoProducto = new PedidoProducto
                {
                    ProductoId = producto.Id,
                    Cantidad = productoDto.Cantidad
                };

                total += precioFinal * productoDto.Cantidad;

                pedido.PedidoProductos.Add(pedidoProducto);
            }

            // Aplicar descuento global del cliente si existe
            var cliente = await _clienteService.GetClienteById(pedidoDto.ClienteId);
            if (cliente != null && cliente.DescuentoGlobal > 0)
            {
                var descuento = total * (cliente.DescuentoGlobal / 100);
                total = total - descuento;
            }

            pedido.Total = total;

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

            // Si el método de pago es CuentaCorriente, sumar el total a la deuda del cliente
            if (pedido.MetodoPago == MetodoPagoEnum.CuentaCorriente)
            {
                await _clienteService.AgregarDeuda(pedido.ClienteId, total);
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

            // Store debt adjustment info before deletion
            bool necesitaAjustarDeuda = pedidoExistente.MetodoPago == MetodoPagoEnum.CuentaCorriente;
            int clienteId = pedidoExistente.ClienteId;
            decimal monto = pedidoExistente.Total;
            
            // Delete associated VentaPagos first
            await _ventaPagosService.DeleteVentaPagosByPedidoId(id);
            
            // Then delete the pedido
            bool eliminado = await _pedidoRepository.DeletePedido(id);
            
            // Only adjust debt if the pedido was successfully deleted
            if (eliminado && necesitaAjustarDeuda)
            {
                await _clienteService.ReducirDeuda(clienteId, monto);
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

            // Handle debt changes based on payment method changes
            if (pedidoExistente.MetodoPago == MetodoPagoEnum.CuentaCorriente && pedidoDto.MetodoPago != MetodoPagoEnum.CuentaCorriente)
            {
                // Was CuentaCorriente, now is not -> reduce debt
                await _clienteService.ReducirDeuda(pedidoExistente.ClienteId, pedidoExistente.Total);
            }
            else if (pedidoExistente.MetodoPago != MetodoPagoEnum.CuentaCorriente && pedidoDto.MetodoPago == MetodoPagoEnum.CuentaCorriente)
            {
                // Was not CuentaCorriente, now is -> add debt
                await _clienteService.AgregarDeuda(pedidoDto.ClienteId, nuevoTotal);
            }
            else if (pedidoExistente.MetodoPago == MetodoPagoEnum.CuentaCorriente && pedidoDto.MetodoPago == MetodoPagoEnum.CuentaCorriente)
            {
                // Both are CuentaCorriente, adjust debt based on total difference
                var diferencia = nuevoTotal - pedidoExistente.Total;
                if (diferencia > 0)
                {
                    await _clienteService.AgregarDeuda(pedidoDto.ClienteId, diferencia);
                }
                else if (diferencia < 0)
                {
                    await _clienteService.ReducirDeuda(pedidoDto.ClienteId, Math.Abs(diferencia));
                }
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
    }
}
