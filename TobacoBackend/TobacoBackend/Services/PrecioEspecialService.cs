using AutoMapper;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Services
{
    public class PrecioEspecialService : IPrecioEspecialService
    {
        private readonly IPrecioEspecialRepository _precioEspecialRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IMapper _mapper;

        public PrecioEspecialService(
            IPrecioEspecialRepository precioEspecialRepository,
            IProductoRepository productoRepository,
            IMapper mapper)
        {
            _precioEspecialRepository = precioEspecialRepository;
            _productoRepository = productoRepository;
            _mapper = mapper;
        }

        public async Task<List<PrecioEspecialDTO>> GetAllPreciosEspecialesAsync()
        {
            var preciosEspeciales = await _precioEspecialRepository.GetAllPreciosEspecialesAsync();
            var dtos = new List<PrecioEspecialDTO>();

            foreach (var precio in preciosEspeciales)
            {
                var dto = _mapper.Map<PrecioEspecialDTO>(precio);
                dto.ClienteNombre = precio.Cliente?.Nombre;
                dto.ProductoNombre = precio.Producto?.Nombre;
                dto.PrecioEstandar = precio.Producto?.Precio;
                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<PrecioEspecialDTO?> GetPrecioEspecialByIdAsync(int id)
        {
            var precioEspecial = await _precioEspecialRepository.GetPrecioEspecialByIdAsync(id);
            if (precioEspecial == null)
                return null;

            var dto = _mapper.Map<PrecioEspecialDTO>(precioEspecial);
            dto.ClienteNombre = precioEspecial.Cliente?.Nombre;
            dto.ProductoNombre = precioEspecial.Producto?.Nombre;
            dto.PrecioEstandar = precioEspecial.Producto?.Precio;

            return dto;
        }

        public async Task<List<PrecioEspecialDTO>> GetPreciosEspecialesByClienteIdAsync(int clienteId)
        {
            var preciosEspeciales = await _precioEspecialRepository.GetPreciosEspecialesByClienteIdAsync(clienteId);
            var dtos = new List<PrecioEspecialDTO>();

            foreach (var precio in preciosEspeciales)
            {
                var dto = _mapper.Map<PrecioEspecialDTO>(precio);
                dto.ClienteNombre = precio.Cliente?.Nombre;
                dto.ProductoNombre = precio.Producto?.Nombre;
                dto.PrecioEstandar = precio.Producto?.Precio;
                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<PrecioEspecialDTO?> GetPrecioEspecialByClienteAndProductoAsync(int clienteId, int productoId)
        {
            var precioEspecial = await _precioEspecialRepository.GetPrecioEspecialByClienteAndProductoAsync(clienteId, productoId);
            if (precioEspecial == null)
                return null;

            var dto = _mapper.Map<PrecioEspecialDTO>(precioEspecial);
            dto.ClienteNombre = precioEspecial.Cliente?.Nombre;
            dto.ProductoNombre = precioEspecial.Producto?.Nombre;
            dto.PrecioEstandar = precioEspecial.Producto?.Precio;

            return dto;
        }

        public async Task<PrecioEspecialDTO> AddPrecioEspecialAsync(PrecioEspecialDTO precioEspecialDto)
        {
            var precioEspecial = _mapper.Map<PrecioEspecial>(precioEspecialDto);
            var result = await _precioEspecialRepository.AddPrecioEspecialAsync(precioEspecial);
            return _mapper.Map<PrecioEspecialDTO>(result);
        }

        public async Task<PrecioEspecialDTO> UpdatePrecioEspecialAsync(PrecioEspecialDTO precioEspecialDto)
        {
            var precioEspecial = _mapper.Map<PrecioEspecial>(precioEspecialDto);
            var result = await _precioEspecialRepository.UpdatePrecioEspecialAsync(precioEspecial);
            return _mapper.Map<PrecioEspecialDTO>(result);
        }

        public async Task<bool> DeletePrecioEspecialAsync(int id)
        {
            return await _precioEspecialRepository.DeletePrecioEspecialAsync(id);
        }

        public async Task<bool> DeletePrecioEspecialByClienteAndProductoAsync(int clienteId, int productoId)
        {
            return await _precioEspecialRepository.DeletePrecioEspecialByClienteAndProductoAsync(clienteId, productoId);
        }

        public async Task<bool> ExistsPrecioEspecialAsync(int clienteId, int productoId)
        {
            return await _precioEspecialRepository.ExistsPrecioEspecialAsync(clienteId, productoId);
        }

        public async Task<List<PrecioEspecialDTO>> GetPreciosEspecialesByProductoIdAsync(int productoId)
        {
            var preciosEspeciales = await _precioEspecialRepository.GetPreciosEspecialesByProductoIdAsync(productoId);
            var dtos = new List<PrecioEspecialDTO>();

            foreach (var precio in preciosEspeciales)
            {
                var dto = _mapper.Map<PrecioEspecialDTO>(precio);
                dto.ClienteNombre = precio.Cliente?.Nombre;
                dto.ProductoNombre = precio.Producto?.Nombre;
                dto.PrecioEstandar = precio.Producto?.Precio;
                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<decimal> GetPrecioFinalProductoAsync(int clienteId, int productoId)
        {
            // Buscar si existe un precio especial para este cliente y producto
            var precioEspecial = await _precioEspecialRepository.GetPrecioEspecialByClienteAndProductoAsync(clienteId, productoId);
            
            if (precioEspecial != null)
            {
                return precioEspecial.Precio;
            }

            // Si no hay precio especial, obtener el precio estándar del producto
            var producto = await _productoRepository.GetProductoById(productoId);
            return producto?.Precio ?? 0;
        }

        public async Task<bool> UpsertPrecioEspecialAsync(int clienteId, int productoId, decimal precio)
        {
            var precioEstandar = await _productoRepository.GetProductoById(productoId);
            if (precioEstandar == null)
                return false;

            // Si el precio es igual al estándar, eliminar el precio especial si existe
            if (precio == precioEstandar.Precio)
            {
                return await _precioEspecialRepository.DeletePrecioEspecialByClienteAndProductoAsync(clienteId, productoId);
            }

            // Si el precio es diferente, crear o actualizar el precio especial
            var precioEspecialExistente = await _precioEspecialRepository.GetPrecioEspecialByClienteAndProductoAsync(clienteId, productoId);
            
            if (precioEspecialExistente != null)
            {
                // Actualizar precio especial existente
                precioEspecialExistente.Precio = precio;
                precioEspecialExistente.FechaActualizacion = DateTime.UtcNow;
                await _precioEspecialRepository.UpdatePrecioEspecialAsync(precioEspecialExistente);
            }
            else
            {
                // Crear nuevo precio especial
                var nuevoPrecioEspecial = new PrecioEspecial
                {
                    ClienteId = clienteId,
                    ProductoId = productoId,
                    Precio = precio,
                    FechaCreacion = DateTime.UtcNow
                };
                await _precioEspecialRepository.AddPrecioEspecialAsync(nuevoPrecioEspecial);
            }

            return true;
        }
    }
}
