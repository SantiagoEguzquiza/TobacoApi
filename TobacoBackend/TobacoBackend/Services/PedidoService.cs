using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;
using TobacoBackend.Repositories;

namespace TobacoBackend.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IVentaPagosService _ventaPagosService;
        private readonly IPrecioEspecialService _precioEspecialService;
        private readonly IMapper _mapper;

        public PedidoService(IPedidoRepository pedidoRepository, IMapper mapper, IProductoRepository productoRepository, IVentaPagosService ventaPagosService, IPrecioEspecialService precioEspecialService)
        {
            _pedidoRepository = pedidoRepository;
            _mapper = mapper;
            _productoRepository = productoRepository;
            _ventaPagosService = ventaPagosService;
            _precioEspecialService = precioEspecialService;
        }

        public async Task AddPedido(PedidoDTO pedidoDto)
        {
            var pedido = new Pedido
            {
                ClienteId = pedidoDto.ClienteId,
                Fecha = DateTime.Now,
                PedidoProductos = new List<PedidoProducto>(),
                MetodoPago = pedidoDto.MetodoPago
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
        }




        public async Task<bool> DeletePedido(int id)
        {
            // Delete associated VentaPagos first
            await _ventaPagosService.DeleteVentaPagosByPedidoId(id);
            
            // Then delete the pedido
            return await _pedidoRepository.DeletePedido(id);
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
            var pedido = _mapper.Map<Pedido>(pedidoDto);
            pedido.Id = id;
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
    }
}
