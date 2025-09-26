using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class PreciosEspecialesController : ControllerBase
    {
        private readonly IPrecioEspecialService _precioEspecialService;

        public PreciosEspecialesController(IPrecioEspecialService precioEspecialService)
        {
            _precioEspecialService = precioEspecialService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PrecioEspecialDTO>>> GetAllPreciosEspeciales()
        {
            var preciosEspeciales = await _precioEspecialService.GetAllPreciosEspecialesAsync();
            return Ok(preciosEspeciales);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PrecioEspecialDTO>> GetPrecioEspecialById(int id)
        {
            try
            {
                var precioEspecial = await _precioEspecialService.GetPrecioEspecialByIdAsync(id);
                if (precioEspecial == null)
                {
                    return NotFound(new { message = "No se encontró el precio especial." });
                }
                return Ok(precioEspecial);
            }
            catch (Exception)
            {
                return NotFound(new { message = "No se encontró el precio especial." });
            }
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<List<PrecioEspecialDTO>>> GetPreciosEspecialesByCliente(int clienteId)
        {
            var preciosEspeciales = await _precioEspecialService.GetPreciosEspecialesByClienteIdAsync(clienteId);
            return Ok(preciosEspeciales);
        }

        [HttpGet("cliente/{clienteId}/producto/{productoId}")]
        public async Task<ActionResult<PrecioEspecialDTO>> GetPrecioEspecialByClienteAndProducto(int clienteId, int productoId)
        {
            try
            {
                var precioEspecial = await _precioEspecialService.GetPrecioEspecialByClienteAndProductoAsync(clienteId, productoId);
                if (precioEspecial == null)
                {
                    return NotFound(new { message = "No se encontró precio especial para este cliente y producto." });
                }
                return Ok(precioEspecial);
            }
            catch (Exception)
            {
                return NotFound(new { message = "No se encontró precio especial para este cliente y producto." });
            }
        }

        [HttpGet("precio-final/cliente/{clienteId}/producto/{productoId}")]
        public async Task<ActionResult<decimal>> GetPrecioFinalProducto(int clienteId, int productoId)
        {
            try
            {
                var precioFinal = await _precioEspecialService.GetPrecioFinalProductoAsync(clienteId, productoId);
                return Ok(new { precio = precioFinal });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Error al obtener el precio del producto." });
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddPrecioEspecial([FromBody] PrecioEspecialDTO precioEspecialDto)
        {
            try
            {
                if (precioEspecialDto == null)
                {
                    return BadRequest(new { message = "El precio especial no puede ser nulo." });
                }

                await _precioEspecialService.AddPrecioEspecialAsync(precioEspecialDto);
                return Ok(new { message = "Precio especial agregado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePrecioEspecial(int id, [FromBody] PrecioEspecialDTO precioEspecialDto)
        {
            try
            {
                if (precioEspecialDto == null || (precioEspecialDto.Id.HasValue && id != precioEspecialDto.Id))
                {
                    return BadRequest(new { message = "ID del precio especial no coincide o el precio especial es nulo." });
                }

                precioEspecialDto.Id = id;
                await _precioEspecialService.UpdatePrecioEspecialAsync(precioEspecialDto);
                return Ok(new { message = "Precio especial actualizado exitosamente." });
            }
            catch (Exception)
            {
                return NotFound(new { message = "Precio especial no encontrado" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePrecioEspecial(int id)
        {
            try
            {
                var deleteResult = await _precioEspecialService.DeletePrecioEspecialAsync(id);
                if (deleteResult)
                {
                    return Ok(new { message = "Precio especial eliminado exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "El precio especial no existe o no se pudo eliminar." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ocurrió un error al intentar eliminar el precio especial: {ex.Message}" });
            }
        }

        [HttpDelete("cliente/{clienteId}/producto/{productoId}")]
        public async Task<ActionResult> DeletePrecioEspecialByClienteAndProducto(int clienteId, int productoId)
        {
            try
            {
                var deleteResult = await _precioEspecialService.DeletePrecioEspecialByClienteAndProductoAsync(clienteId, productoId);
                if (deleteResult)
                {
                    return Ok(new { message = "Precio especial eliminado exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "El precio especial no existe o no se pudo eliminar." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ocurrió un error al intentar eliminar el precio especial: {ex.Message}" });
            }
        }

        [HttpPost("upsert")]
        public async Task<ActionResult> UpsertPrecioEspecial([FromBody] UpsertPrecioEspecialRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "La solicitud no puede ser nula." });
                }

                var result = await _precioEspecialService.UpsertPrecioEspecialAsync(request.ClienteId, request.ProductoId, request.Precio);
                if (result)
                {
                    return Ok(new { message = "Precio especial actualizado exitosamente." });
                }
                else
                {
                    return BadRequest(new { message = "No se pudo procesar el precio especial." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class UpsertPrecioEspecialRequest
    {
        public int ClienteId { get; set; }
        public int ProductoId { get; set; }
        public decimal Precio { get; set; }
    }
}
