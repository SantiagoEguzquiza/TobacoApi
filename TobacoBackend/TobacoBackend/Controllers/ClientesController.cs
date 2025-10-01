using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        
        [HttpGet]
        public async Task<ActionResult<List<ClienteDTO>>> GetAllClientes()
        {
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
        public async Task<ActionResult<ClienteDTO>> AddCliente([FromBody] ClienteDTO clienteDto)
        {
            try
            {
                if (clienteDto == null)
                {
                    return BadRequest(new { message = "El cliente no puede ser nulo." });
                }

                var clienteCreado = await _clienteService.AddCliente(clienteDto);

                return Ok(clienteCreado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCliente(int id, [FromBody] ClienteDTO clienteDto)
        {
            if (clienteDto == null || (clienteDto.Id.HasValue && id != clienteDto.Id))
            {
                return BadRequest(new { message = "ID del cliente no coincide o el cliente es nulo." });
            }

            try
            {
                // Asegurar que el ID del DTO coincida con el ID de la ruta
                clienteDto.Id = id;
                await _clienteService.UpdateCliente(id, clienteDto);
                return Ok(new { message = "Cliente actualizado exitosamente." });
            }
            catch (Exception)
            {
                return NotFound(new { message = "Cliente no encontrado" });
            }
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCliente(int id)
        {
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
    }
}
