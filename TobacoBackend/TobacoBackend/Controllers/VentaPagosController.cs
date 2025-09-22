using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class VentaPagosController : ControllerBase
    {
        private readonly IVentaPagosService _ventaPagosService;

        public VentaPagosController(IVentaPagosService ventaPagosService)
        {
            _ventaPagosService = ventaPagosService;
        }

        [HttpGet]
        public async Task<ActionResult<List<VentaPagosDTO>>> GetAllVentaPagos()
        {
            var ventaPagos = await _ventaPagosService.GetAllVentaPagos();
            return Ok(ventaPagos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VentaPagosDTO>> GetVentaPagosById(int id)
        {
            try
            {
                var ventaPagos = await _ventaPagosService.GetVentaPagosById(id);
                return Ok(ventaPagos);
            }
            catch (Exception)
            {
                return NotFound(new { message = "No se encontraron pagos que coincidan con el id." });
            }
        }

        [HttpGet("pedido/{pedidoId}")]
        public async Task<ActionResult<List<VentaPagosDTO>>> GetVentaPagosByPedidoId(int pedidoId)
        {
            var ventaPagos = await _ventaPagosService.GetVentaPagosByPedidoId(pedidoId);
            return Ok(ventaPagos);
        }

        [HttpPost]
        public async Task<ActionResult> AddVentaPagos([FromBody] VentaPagosDTO ventaPagosDto)
        {
            try
            {
                if (ventaPagosDto == null)
                {
                    return BadRequest(new { message = "El pago no puede ser nulo." });
                }

                await _ventaPagosService.AddVentaPagos(ventaPagosDto);
                return Ok(new { message = "Pago agregado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateVentaPagos(int id, [FromBody] VentaPagosDTO ventaPagosDto)
        {
            if (ventaPagosDto == null || id != ventaPagosDto.Id)
            {
                return BadRequest(new { message = "ID del pago no coincide o el pago es nulo." });
            }

            try
            {
                await _ventaPagosService.UpdateVentaPagos(ventaPagosDto);
                return Ok(new { message = "Pago actualizado exitosamente." });
            }
            catch (Exception)
            {
                return NotFound(new { message = "Pago no encontrado" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVentaPagos(int id)
        {
            try
            {
                var deleteResult = await _ventaPagosService.DeleteVentaPagos(id);
                if (deleteResult)
                {
                    return Ok(new { message = "Pago eliminado exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "El pago no existe o no se pudo eliminar." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ocurrió un error al intentar eliminar el pago: {ex.Message}" });
            }
        }
    }
}
