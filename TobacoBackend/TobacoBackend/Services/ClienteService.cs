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
        private readonly AplicationDbContext _context;

        public ClienteService(IClienteRepository clienteRepository, IMapper mapper, AplicationDbContext context)
        {
            _clienteRepository = clienteRepository;
            _mapper = mapper;
            _context = context;
        }
        public async Task<ClienteDTO> AddCliente(ClienteDTO clienteDto)
        {
            var cliente = _mapper.Map<Cliente>(clienteDto);
            
            // Set TenantId from current context
            var tenantId = _context.GetCurrentTenantId();
            if (!tenantId.HasValue)
                throw new InvalidOperationException("No se pudo determinar el TenantId del contexto actual.");
            cliente.TenantId = tenantId.Value;
            
            var clienteCreado = await _clienteRepository.AddCliente(cliente);
            return _mapper.Map<ClienteDTO>(clienteCreado);
        }

        /// <summary>
        /// Obtiene o crea el cliente "Consumidor Final" del tenant actual (un Consumidor Final por tenant)
        /// </summary>
        public async Task<ClienteDTO> ObtenerOCrearConsumidorFinal()
        {
            var tenantId = _context.GetCurrentTenantId();
            if (!tenantId.HasValue)
                throw new InvalidOperationException("No se pudo determinar el tenant. Inicia sesión con un usuario de tenant.");

            var consumidorFinal = await _clienteRepository.BuscarConsumidorFinal(tenantId.Value);
            if (consumidorFinal != null)
                return _mapper.Map<ClienteDTO>(consumidorFinal);

            // Crear con datos mínimos válidos para el tenant actual
            var nuevoConsumidorFinal = new Cliente
            {
                Nombre = "Consumidor Final",
                Direccion = "Sin dirección",
                Telefono = "Sin teléfono",
                Deuda = "0",
                DescuentoGlobal = 0,
                Visible = true,
                HasCCTE = false,
                TenantId = tenantId.Value
            };

            var consumidorCreado = await _clienteRepository.AddCliente(nuevoConsumidorFinal);
            return _mapper.Map<ClienteDTO>(consumidorCreado);
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
            // Obtener el cliente existente para preservar el TenantId
            var clienteExistente = await _clienteRepository.GetClienteById(id);
            if (clienteExistente == null)
            {
                throw new Exception($"Cliente con ID {id} no encontrado");
            }

            var cliente = _mapper.Map<Cliente>(clienteDto);
            cliente.Id = id;
            // Preservar el TenantId del cliente existente
            cliente.TenantId = clienteExistente.TenantId;
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
            // Permitir valores negativos = saldo a favor del cliente
            cliente.Deuda = nuevaDeuda.ToString();
            await _clienteRepository.UpdateCliente(cliente);
        }

        public async Task<bool> ValidarMontoAbono(int clienteId, decimal montoAbono)
        {
            if (montoAbono <= 0) return false;
            var cliente = await _clienteRepository.GetClienteById(clienteId);
            if (cliente == null) return false;
            // Permitir abonos mayores a la deuda: el excedente se convierte en saldo a favor
            return true;
        }

        public async Task<object> GetDetalleDeuda(int clienteId)
        {
            var cliente = await _clienteRepository.GetClienteById(clienteId);
            if (cliente == null)
            {
                throw new Exception($"Cliente con ID {clienteId} no encontrado");
            }

            var saldo = cliente.DeudaDecimal;
            if (saldo == 0)
            {
                return new
                {
                    clienteId = cliente.Id,
                    clienteNombre = cliente.Nombre,
                    deudaActual = 0m,
                    saldoAFavor = 0m,
                    deudaFormateada = "0",
                    saldoAFavorFormateado = "0",
                    fechaConsulta = DateTime.Now,
                    mensaje = "El cliente no tiene deuda ni saldo a favor"
                };
            }

            if (saldo < 0)
            {
                return new
                {
                    clienteId = cliente.Id,
                    clienteNombre = cliente.Nombre,
                    deudaActual = 0m,
                    saldoAFavor = Math.Abs(saldo),
                    deudaFormateada = "0",
                    saldoAFavorFormateado = Math.Abs(saldo).ToString("F2"),
                    fechaConsulta = DateTime.Now
                };
            }

            return new
            {
                clienteId = cliente.Id,
                clienteNombre = cliente.Nombre,
                deudaActual = saldo,
                saldoAFavor = 0m,
                deudaFormateada = cliente.Deuda,
                saldoAFavorFormateado = "0",
                fechaConsulta = DateTime.Now
            };
        }
    }
}
