using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;
using TobacoBackend.Services;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AbonosController : ControllerBase
    {
        private readonly IAbonosService _abonosService;

        public AbonosController(IAbonosService abonosService)
        {
            this._abonosService = abonosService;
        }

        [HttpPost]
        public async Task<ActionResult<AbonoDTO>> AddAbono([FromBody] AbonoDTO abonoDto)
        {
            try
            {
                if (abonoDto == null)
                {
                    return BadRequest(new { message = "El abono no puede ser nulo." });
                }

                var abonoCreado = await _abonosService.AddAbono(abonoDto);

                return Ok(abonoCreado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAbono(int id, [FromBody] AbonoDTO abonoDto)
        {
            if (abonoDto == null || (abonoDto.Id.HasValue && id != abonoDto.Id))
            {
                return BadRequest(new { message = "ID del abono no coincide o el abono es nulo." });
            }

            try
            {
                // Asegurar que el ID del DTO coincida con el ID de la ruta
                abonoDto.Id = id;
                await _abonosService.UpdateAbono(id, abonoDto);
                return Ok(new { message = "Abono actualizado exitosamente." });
            }
            catch (Exception)
            {
                return NotFound(new { message = "Abono no encontrado" });
            }
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAbono(int id)
        {
            try
            {
                var deleteResult = await _abonosService.DeleteAbono(id);
                if (deleteResult)
                {
                    return Ok(new { message = "Abono eliminado exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "El abono no existe o no se pudo eliminar." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ocurrió un error al intentar eliminar el abono: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<AbonoDTO>>> GetAllAbonos()
        {
            var abonos = await _abonosService.GetAllAbonos();
            return Ok(abonos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AbonoDTO>> GetAbonoById(int id)
        {
            try
            {
                var abono = await _abonosService.GetAbonoById(id);
                return Ok(abono);
            }
            catch (Exception)
            {
                return NotFound(new { message = "No se encontró el abono con el ID especificado." });
            }
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<List<AbonoDTO>>> GetAbonosByClienteId(int clienteId)
        {
            try
            {
                var abonos = await _abonosService.GetAbonosByClienteId(clienteId);
                return Ok(abonos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener abonos del cliente: {ex.Message}" });
            }
        }
    }
}
