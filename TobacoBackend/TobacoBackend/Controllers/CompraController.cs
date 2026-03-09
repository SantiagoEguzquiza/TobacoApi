using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TobacoBackend.Authorization;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;
using TobacoBackend.Helpers;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompraController : ControllerBase
    {
        private readonly ICompraService _compraService;

        public CompraController(ICompraService compraService)
        {
            _compraService = compraService;
        }

        [HttpPost]
        public async Task<ActionResult<CompraDTO>> Create([FromBody] CreateCompraDTO dto)
        {
            var hasPermission = await PermissionHelper.HasPermissionAsync(User, HttpContext.RequestServices, "Compras_Crear");
            if (!hasPermission)
            {
                return Forbid("No tienes permiso para crear compras.");
            }
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Los datos de la compra son requeridos." });
                var created = await _compraService.CreateAsync(dto);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<CompraDTO>>> GetAll([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var hasPermission = await PermissionHelper.HasPermissionAsync(User, HttpContext.RequestServices, "Compras_Visualizar");
            if (!hasPermission)
            {
                return Forbid("No tienes permiso para visualizar compras.");
            }
            var list = await _compraService.GetAllAsync(desde, hasta);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompraDTO>> GetById(int id)
        {
            var hasPermission = await PermissionHelper.HasPermissionAsync(User, HttpContext.RequestServices, "Compras_Visualizar");
            if (!hasPermission)
            {
                return Forbid("No tienes permiso para visualizar compras.");
            }
            var compra = await _compraService.GetByIdAsync(id);
            if (compra == null)
                return NotFound(new { message = "Compra no encontrada." });
            return Ok(compra);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var hasPermission = await PermissionHelper.HasPermissionAsync(User, HttpContext.RequestServices, "Compras_Eliminar");
            if (!hasPermission)
            {
                return Forbid("No tienes permiso para eliminar compras.");
            }
            try
            {
                await _compraService.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
