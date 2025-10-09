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
        private readonly IVentaPagoService _ventaPagoService;

        public VentaPagosController(IVentaPagoService ventaPagoService)
        {
            _ventaPagoService = ventaPagoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<VentaPagoDTO>>> GetAllVentaPagos()
        {
            var ventaPagos = await _ventaPagoService.GetAllVentaPagos();
            return Ok(ventaPagos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VentaPagoDTO>> GetVentaPagoById(int id)
        {
            try
            {
                var ventaPago = await _ventaPagoService.GetVentaPagoById(id);
                return Ok(ventaPago);
            }
            catch (Exception)
            {
                return NotFound(new { message = "No se encontraron pagos que coincidan con el id." });
            }
        }

        [HttpGet("venta/{ventaId}")]
        public async Task<ActionResult<List<VentaPagoDTO>>> GetVentaPagosByVentaId(int ventaId)
        {
            var ventaPagos = await _ventaPagoService.GetVentaPagosByVentaId(ventaId);
            return Ok(ventaPagos);
        }

        [HttpPost]
        public async Task<ActionResult> AddVentaPago([FromBody] VentaPagoDTO ventaPagoDto)
        {
            try
            {
                if (ventaPagoDto == null)
                {
                    return BadRequest(new { message = "El pago no puede ser nulo." });
                }

                await _ventaPagoService.AddVentaPago(ventaPagoDto);
                return Ok(new { message = "Pago agregado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateVentaPago(int id, [FromBody] VentaPagoDTO ventaPagoDto)
        {
            if (ventaPagoDto == null || id != ventaPagoDto.Id)
            {
                return BadRequest(new { message = "ID del pago no coincide o el pago es nulo." });
            }

            try
            {
                await _ventaPagoService.UpdateVentaPago(ventaPagoDto);
                return Ok(new { message = "Pago actualizado exitosamente." });
            }
            catch (Exception)
            {
                return NotFound(new { message = "Pago no encontrado" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVentaPago(int id)
        {
            try
            {
                var deleteResult = await _ventaPagoService.DeleteVentaPago(id);
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
