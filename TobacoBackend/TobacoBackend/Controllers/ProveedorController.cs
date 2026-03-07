using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProveedorController : ControllerBase
    {
        private readonly IProveedorService _proveedorService;

        public ProveedorController(IProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProveedorDTO>>> GetAll()
        {
            var list = await _proveedorService.GetAllAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<ActionResult<ProveedorDTO>> Create([FromBody] CreateProveedorDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Los datos del proveedor son requeridos." });
                var created = await _proveedorService.CreateAsync(dto);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
