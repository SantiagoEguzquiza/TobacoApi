using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;
using TobacoBackend.Services;
using TobacoBackend.Helpers;
using System.Security.Claims;
using TobacoBackend.Authorization;
using TobacoBackend.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = AuthorizationPolicies.AdminOrEmployeeOnly)] // Admin o Employee, NO SuperAdmin
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly IAbonosService _abonosService;
        private readonly IVentaService _ventaService;
        private readonly AuditService _auditService;

        public ClientesController(IClienteService clienteService, IAbonosService abonosService, IVentaService ventaService, AuditService auditService)
        {
            _clienteService = clienteService;
            _abonosService = abonosService;
            _ventaService = ventaService;
            _auditService = auditService;
        }

        
        [HttpGet]
        public async Task<ActionResult<List<ClienteDTO>>> GetAllClientes()
        {
            // Validar permiso de visualizar clientes
            var canView = await PermissionHelper.CanViewModuleAsync(User, HttpContext.RequestServices, "Clientes");
            if (!canView)
            {
                return Forbid("No tienes permiso para visualizar clientes.");
            }

            var clientes = await _clienteService.GetAllClientes();
            return Ok(clientes);
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDTO>> GetClienteById(int id)
        {
            try
            {
                var cliente = await _clienteService.GetClienteById(id);
                return Ok(cliente);
            }
            catch (Exception)
            {
                return NotFound(new { message = "No se encontraron clientes que coincidan con el id." });
            }
        }

       
        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.AdminOrEmployee)] // Validación de permisos se hace dentro
        public async Task<ActionResult<ClienteDTO>> AddCliente([FromBody] ClienteDTO clienteDto)
        {
            // Validar permiso de crear clientes
            var hasPermission = await PermissionHelper.HasPermissionAsync(User, HttpContext.RequestServices, "Clientes_Crear");
            if (!hasPermission)
            {
                return Forbid("No tienes permiso para crear clientes.");
            }

            try
            {
                if (clienteDto == null)
                {
                    return BadRequest(new { message = "El cliente no puede ser nulo." });
                }

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos del cliente inválidos.", errors = ModelState });
                }

                // Sanitizar y validar entrada
                clienteDto.Nombre = InputSanitizer.SanitizeString(clienteDto.Nombre, 100);
                clienteDto.Direccion = InputSanitizer.SanitizeString(clienteDto.Direccion, 200);
                
                if (InputSanitizer.ContainsSqlInjection(clienteDto.Nombre) || 
                    InputSanitizer.ContainsXss(clienteDto.Nombre) ||
                    InputSanitizer.ContainsSqlInjection(clienteDto.Direccion) ||
                    InputSanitizer.ContainsXss(clienteDto.Direccion))
                {
                    return BadRequest(new { message = "Entrada inválida detectada." });
                }

                var clienteCreado = await _clienteService.AddCliente(clienteDto);

                // Auditoría
                _auditService.LogCreate("Cliente", clienteCreado.Id ?? 0, User, 
                    SecurityLoggingService.GetClientIpAddress(HttpContext));

                return Ok(clienteCreado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOrEmployee)] // Validación de permisos se hace dentro
        public async Task<ActionResult> UpdateCliente(int id, [FromBody] ClienteDTO clienteDto)
        {
            // Validar permiso de editar clientes
            var hasPermission = await PermissionHelper.HasPermissionAsync(User, HttpContext.RequestServices, "Clientes_Editar");
            if (!hasPermission)
            {
                return Forbid("No tienes permiso para editar clientes.");
            }

            if (clienteDto == null || (clienteDto.Id.HasValue && id != clienteDto.Id))
            {
                return BadRequest(new { message = "ID del cliente no coincide o el cliente es nulo." });
            }

            // Validar modelo
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Datos del cliente inválidos.", errors = ModelState });
            }

            // Sanitizar y validar entrada
            clienteDto.Nombre = InputSanitizer.SanitizeString(clienteDto.Nombre, 100);
            clienteDto.Direccion = InputSanitizer.SanitizeString(clienteDto.Direccion, 200);
            
            if (InputSanitizer.ContainsSqlInjection(clienteDto.Nombre) || 
                InputSanitizer.ContainsXss(clienteDto.Nombre) ||
                InputSanitizer.ContainsSqlInjection(clienteDto.Direccion) ||
                InputSanitizer.ContainsXss(clienteDto.Direccion))
            {
                return BadRequest(new { message = "Entrada inválida detectada." });
            }

            try
            {
                // Asegurar que el ID del DTO coincida con el ID de la ruta
                clienteDto.Id = id;
                await _clienteService.UpdateCliente(id, clienteDto);
                
                // Auditoría
                _auditService.LogUpdate("Cliente", id, User, null,
                    SecurityLoggingService.GetClientIpAddress(HttpContext));

                return Ok(new { message = "Cliente actualizado exitosamente." });
            }
            catch (Exception)
            {
                return NotFound(new { message = "Cliente no encontrado" });
            }
        }


        [HttpDelete("{id}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOrEmployee)] // Validación de permisos se hace dentro
        public async Task<ActionResult> DeleteCliente(int id)
        {
            // Validar permiso de eliminar clientes
            var hasPermission = await PermissionHelper.HasPermissionAsync(User, HttpContext.RequestServices, "Clientes_Eliminar");
            if (!hasPermission)
            {
                return Forbid("No tienes permiso para eliminar clientes.");
            }

            try
            {
                var deleteResult = await _clienteService.DeleteCliente(id);
                if (deleteResult)
                {
                    return Ok(new { message = "Cliente eliminado exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "El cliente no existe o no se pudo eliminar." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ocurrió un error al intentar eliminar el cliente: {ex.Message}" });
            }
        }

        // GET: api/clientes/buscar?query=juan
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarClientes([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("El parámetro de búsqueda no puede estar vacío.");

            var clientes = await _clienteService.BuscarClientesAsync(query);
            return Ok(clientes);
        }

        [HttpGet("con-deuda")]
        public async Task<ActionResult<List<ClienteDTO>>> GetClientesConDeuda()
        {
            var clientes = await _clienteService.GetClientesConDeuda();
            return Ok(clientes);
        }

        // GET: api/clientes/paginados?page=1&pageSize=20
        [HttpGet("paginados")]
        public async Task<ActionResult<object>> GetClientesPaginados([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _clienteService.GetClientesPaginados(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener clientes paginados: {ex.Message}" });
            }
        }

        // GET: api/clientes/con-deuda/paginados?page=1&pageSize=20
        [HttpGet("con-deuda/paginados")]
        public async Task<ActionResult<object>> GetClientesConDeudaPaginados([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _clienteService.GetClientesConDeudaPaginados(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener clientes con deuda paginados: {ex.Message}" });
            }
        }

        [HttpGet("{id}/validar-abono")]
        public async Task<ActionResult<bool>> ValidarMontoAbono(int id, [FromQuery] decimal monto)
        {
            try
            {
                if (monto <= 0)
                {
                    return BadRequest(new { message = "El monto debe ser mayor a cero." });
                }

                var esValido = await _clienteService.ValidarMontoAbono(id, monto);
                return Ok(new { esValido = esValido });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al validar monto de abono: {ex.Message}" });
            }
        }

        [HttpGet("{id}/deuda")]
        public async Task<ActionResult<object>> GetDetalleDeuda(int id)
        {
            try
            {
                var detalle = await _clienteService.GetDetalleDeuda(id);
                return Ok(detalle);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener detalle de deuda: {ex.Message}" });
            }
        }

        [HttpGet("{id}/ventas-cc")]
        public async Task<ActionResult<object>> GetVentasCuentaCorriente(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var ventas = await _ventaService.GetVentasConCuentaCorrienteByClienteId(id, page, pageSize);
                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener ventas con cuenta corriente: {ex.Message}" });
            }
        }

        [HttpPost("{id}/saldarDeuda")]
        public async Task<ActionResult<AbonoDTO>> SaldarDeuda(int id, [FromBody] SaldarDeudaDTO saldarDeudaDto)
        {
            try
            {
                if (saldarDeudaDto == null)
                {
                    return BadRequest(new { message = "Los datos del abono no pueden ser nulos." });
                }

                if (saldarDeudaDto.Monto <= 0)
                {
                    return BadRequest(new { message = "El monto debe ser mayor a cero." });
                }

                // Validar que el monto no exceda la deuda
                var esValido = await _clienteService.ValidarMontoAbono(id, saldarDeudaDto.Monto);
                if (!esValido)
                {
                    return BadRequest(new { message = "El monto del abono no puede ser mayor a la deuda del cliente." });
                }

                // Crear el abono
                var abonoDto = new AbonoDTO
                {
                    ClienteId = id,
                    Monto = saldarDeudaDto.Monto.ToString(),
                    Fecha = saldarDeudaDto.Fecha,
                    Nota = saldarDeudaDto.Nota ?? ""
                };

                var abonoCreado = await _abonosService.AddAbono(abonoDto);
                return Ok(abonoCreado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al saldar deuda: {ex.Message}" });
            }
        }
    }
}
