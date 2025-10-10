using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ProductoAFavorController : ControllerBase
    {
        private readonly IProductoAFavorService _productoAFavorService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductoAFavorController(IProductoAFavorService productoAFavorService, IHttpContextAccessor httpContextAccessor)
        {
            _productoAFavorService = productoAFavorService;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: api/ProductoAFavor
        [HttpGet]
        public async Task<ActionResult<List<ProductoAFavorDTO>>> GetAllProductosAFavor()
        {
            try
            {
                var productos = await _productoAFavorService.GetAllProductosAFavor();
                return Ok(productos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener productos a favor: {ex.Message}" });
            }
        }

        // GET: api/ProductoAFavor/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoAFavorDTO>> GetProductoAFavorById(int id)
        {
            try
            {
                var producto = await _productoAFavorService.GetProductoAFavorById(id);
                if (producto == null)
                {
                    return NotFound(new { message = "Producto a favor no encontrado." });
                }
                return Ok(producto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener producto a favor: {ex.Message}" });
            }
        }

        // GET: api/ProductoAFavor/cliente/{clienteId}?soloNoEntregados=true
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<List<ProductoAFavorDTO>>> GetProductosAFavorByClienteId(
            int clienteId, 
            [FromQuery] bool? soloNoEntregados = null)
        {
            try
            {
                var productos = await _productoAFavorService.GetProductosAFavorByClienteId(clienteId, soloNoEntregados);
                return Ok(productos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener productos a favor del cliente: {ex.Message}" });
            }
        }

        // GET: api/ProductoAFavor/venta/{ventaId}
        [HttpGet("venta/{ventaId}")]
        public async Task<ActionResult<List<ProductoAFavorDTO>>> GetProductosAFavorByVentaId(int ventaId)
        {
            try
            {
                var productos = await _productoAFavorService.GetProductosAFavorByVentaId(ventaId);
                return Ok(productos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener productos a favor de la venta: {ex.Message}" });
            }
        }

        // PUT: api/ProductoAFavor/{id}/marcar-entregado
        [HttpPut("{id}/marcar-entregado")]
        public async Task<ActionResult> MarcarComoEntregado(int id)
        {
            try
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                int usuarioId = 0;
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    usuarioId = userId;
                }

                await _productoAFavorService.MarcarComoEntregado(id, usuarioId);
                return Ok(new { message = "Producto marcado como entregado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al marcar producto como entregado: {ex.Message}" });
            }
        }

        // DELETE: api/ProductoAFavor/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProductoAFavor(int id)
        {
            try
            {
                var result = await _productoAFavorService.DeleteProductoAFavor(id);
                if (!result)
                {
                    return NotFound(new { message = "Producto a favor no encontrado." });
                }
                return Ok(new { message = "Producto a favor eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al eliminar producto a favor: {ex.Message}" });
            }
        }
    }
}

