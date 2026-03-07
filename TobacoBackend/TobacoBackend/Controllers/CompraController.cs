using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;

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
            var list = await _compraService.GetAllAsync(desde, hasta);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompraDTO>> GetById(int id)
        {
            var compra = await _compraService.GetByIdAsync(id);
            if (compra == null)
                return NotFound(new { message = "Compra no encontrada." });
            return Ok(compra);
        }
    }
}
