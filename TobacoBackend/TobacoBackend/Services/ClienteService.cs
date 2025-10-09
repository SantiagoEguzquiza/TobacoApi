using AutoMapper;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;

        public ClienteService(IClienteRepository clienteRepository, IMapper mapper)
        {
            _clienteRepository = clienteRepository;
            _mapper = mapper;
        }
        public async Task<ClienteDTO> AddCliente(ClienteDTO clienteDto)
        {
            var cliente = _mapper.Map<Cliente>(clienteDto);
            var clienteCreado = await _clienteRepository.AddCliente(cliente);
            return _mapper.Map<ClienteDTO>(clienteCreado);
        }

        public async Task<bool> DeleteCliente(int id)
        {
            return await _clienteRepository.DeleteCliente(id);
        }

        public async Task<List<ClienteDTO>> GetAllClientes()
        {
            var clientes = await _clienteRepository.GetAllClientes();
            return _mapper.Map<List<ClienteDTO>>(clientes);
        }

        public async Task<ClienteDTO> GetClienteById(int id)
        {
            var cliente = await _clienteRepository.GetClienteById(id);
            return _mapper.Map<ClienteDTO>(cliente);
        }

        public async Task UpdateCliente(int id, ClienteDTO clienteDto)
        {
            var cliente = _mapper.Map<Cliente>(clienteDto);
            cliente.Id = id;
            await _clienteRepository.UpdateCliente(cliente);
        }

        public async Task<IEnumerable<ClienteDTO>> BuscarClientesAsync(string query)
        {
            var clientes = await _clienteRepository.BuscarClientesAsync(query);
            return _mapper.Map<IEnumerable<ClienteDTO>>(clientes);
        }

        public async Task<List<ClienteDTO>> GetClientesConDeuda()
        {
            var clientes = await _clienteRepository.GetClientesConDeuda();
            return _mapper.Map<List<ClienteDTO>>(clientes);
        }

        public async Task<object> GetClientesPaginados(int page, int pageSize)
        {
            var result = await _clienteRepository.GetClientesPaginados(page, pageSize);
            var clientes = _mapper.Map<List<ClienteDTO>>(result.Clientes);
            
            return new
            {
                Clientes = clientes,
                TotalCount = result.TotalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize),
                HasNextPage = page < (int)Math.Ceiling((double)result.TotalCount / pageSize),
                HasPreviousPage = page > 1
            };
        }

        public async Task<object> GetClientesConDeudaPaginados(int page, int pageSize)
        {
            var result = await _clienteRepository.GetClientesConDeudaPaginados(page, pageSize);
            var clientes = _mapper.Map<List<ClienteDTO>>(result.Clientes);
            
            return new
            {
                clientes = clientes,
                totalItems = result.TotalCount,
                totalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize),
                currentPage = page,
                pageSize = pageSize,
                hasNextPage = page < (int)Math.Ceiling((double)result.TotalCount / pageSize),
                hasPreviousPage = page > 1
            };
        }

        // Métodos para manejo de deuda
        public async Task AgregarDeuda(int clienteId, decimal monto)
        {
            var cliente = await _clienteRepository.GetClienteById(clienteId);
            if (cliente == null)
            {
                throw new Exception($"Cliente con ID {clienteId} no encontrado");
            }

            var deudaActual = cliente.DeudaDecimal;
            var nuevaDeuda = deudaActual + monto;

            cliente.Deuda = nuevaDeuda.ToString();
            await _clienteRepository.UpdateCliente(cliente);
        }

        public async Task ReducirDeuda(int clienteId, decimal monto)
        {
            var cliente = await _clienteRepository.GetClienteById(clienteId);
            if (cliente == null)
            {
                throw new Exception($"Cliente con ID {clienteId} no encontrado");
            }

            var deudaActual = cliente.DeudaDecimal;
            var nuevaDeuda = deudaActual - monto;
            
            // No permitir deuda negativa
            if (nuevaDeuda < 0)
            {
                nuevaDeuda = 0;
            }

            cliente.Deuda = nuevaDeuda.ToString();
            await _clienteRepository.UpdateCliente(cliente);
        }

        public async Task<bool> ValidarMontoAbono(int clienteId, decimal montoAbono)
        {
            var cliente = await _clienteRepository.GetClienteById(clienteId);
            if (cliente == null)
            {
                return false;
            }

            var deudaActual = cliente.DeudaDecimal;
            return montoAbono <= deudaActual && montoAbono > 0;
        }

        public async Task<object> GetDetalleDeuda(int clienteId)
        {
            var cliente = await _clienteRepository.GetClienteById(clienteId);
            if (cliente == null)
            {
                throw new Exception($"Cliente con ID {clienteId} no encontrado");
            }

            // Solo devolver si tiene deuda
            if (cliente.DeudaDecimal <= 0)
            {
                return new
                {
                    clienteId = cliente.Id,
                    clienteNombre = cliente.Nombre,
                    deudaActual = 0,
                    deudaFormateada = "0",
                    fechaConsulta = DateTime.Now,
                    mensaje = "El cliente no tiene deuda pendiente"
                };
            }

            return new
            {
                clienteId = cliente.Id,
                clienteNombre = cliente.Nombre,
                deudaActual = cliente.DeudaDecimal,
                deudaFormateada = cliente.Deuda,
                fechaConsulta = DateTime.Now
            };
        }
    }
}
