using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;
using System.Security.Claims;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecorridosProgramadosController : ControllerBase
    {
        private readonly IRecorridoProgramadoService _recorridoService;
        private readonly ILogger<RecorridosProgramadosController> _logger;

        public RecorridosProgramadosController(IRecorridoProgramadoService recorridoService, ILogger<RecorridosProgramadosController> logger)
        {
            _recorridoService = recorridoService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene los recorridos programados de un vendedor específico
        /// </summary>
        [HttpGet("vendedor/{vendedorId}")]
        public async Task<ActionResult<List<RecorridoProgramadoDTO>>> GetByVendedor(int vendedorId)
        {
            try
            {
                var recorridos = await _recorridoService.GetRecorridosByVendedor(vendedorId);
                return Ok(recorridos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener recorridos del vendedor");
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Crea un nuevo recorrido programado (Admin)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RecorridoProgramadoDTO>> Create([FromBody] CreateRecorridoProgramadoDTO dto)
        {
            try
            {
                _logger.LogInformation($"Creando recorrido programado - VendedorId: {dto.VendedorId}, ClienteId: {dto.ClienteId}, DiaSemana: {dto.DiaSemana} (valor: {(int)dto.DiaSemana}), Orden: {dto.Orden}");
                
                var recorrido = await _recorridoService.Create(dto);
                
                _logger.LogInformation($"Recorrido programado creado exitosamente - Id: {recorrido.Id}");
                
                return CreatedAtAction(nameof(GetById), new { id = recorrido.Id }, recorrido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear recorrido programado");
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene un recorrido programado por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RecorridoProgramadoDTO>> GetById(int id)
        {
            try
            {
                var recorrido = await _recorridoService.GetById(id);
                if (recorrido == null)
                {
                    return NotFound();
                }
                return Ok(recorrido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener recorrido programado");
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Actualiza un recorrido programado (Admin)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateRecorridoProgramadoDTO dto)
        {
            try
            {
                var resultado = await _recorridoService.Update(id, dto);
                if (resultado == null)
                {
                    return NotFound();
                }
                return Ok(new { message = "Recorrido programado actualizado exitosamente", recorrido = resultado });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar recorrido programado");
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Elimina un recorrido programado (Admin)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var resultado = await _recorridoService.Delete(id);
                if (!resultado)
                {
                    return NotFound();
                }
                return Ok(new { message = "Recorrido programado eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar recorrido programado");
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }
    }
}
