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
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;

        public ProductosController(IProductoService productoService)
        {
            _productoService = productoService;
        }


        [HttpGet]
        public async Task<ActionResult<List<ProductoDTO>>> GetAllProductos()
        {
            var productos = await _productoService.GetAllProductos();
            return Ok(productos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDTO>> GetProductoById(int id)
        {
            try
            {
                var producto = await _productoService.GetProductoById(id);
                return Ok(producto);
            }
            catch (Exception)
            {
                return NotFound(new { message = "No se encontraron productos que coincidan con el id." });
            }
        }



        [HttpPost]
        public async Task<ActionResult> AddProducto([FromBody] ProductoDTO productoDto)
        {
            try
            {
                if (productoDto == null)
                {
                    return BadRequest(new { message = "El producto no puede ser nulo." });
                }

                // Validar el modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    
                    return BadRequest(new
                    {
                        message = "Datos de validación incorrectos",
                        errors = errors
                    });
                }

                await _productoService.AddProducto(productoDto);

                return CreatedAtAction(nameof(GetProductoById), new { id = productoDto.Id }, new { message = "Producto agregado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Error al guardar el producto.",
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }




        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProducto(int id, [FromBody] ProductoDTO productoDto)
        {
            if (productoDto == null || id != productoDto.Id)
            {
                return BadRequest(new { message = "ID del producto no coincide o el producto es nulo." });
            }

            // Validar el modelo
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                
                return BadRequest(new
                {
                    message = "Datos de validación incorrectos",
                    errors = errors
                });
            }

            try
            {
                await _productoService.UpdateProducto(id, productoDto);
                return Ok(new { message = "Producto actualizado exitosamente." });
            }
            catch (Exception)
            {
                return NotFound(new { message = "Producto no encontrado" });
            }
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProducto(int id)
        {
            try
            {
                var deleteResult = await _productoService.DeleteProducto(id);
                if (deleteResult)
                {
                    return Ok(new { message = "Producto eliminado exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "El producto no existe o no se pudo eliminar." });
                }
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { 
                    message = ex.Message,
                    canSoftDelete = true,
                    suggestion = "Este producto tiene ventas vinculadas. ¿Desea desactivarlo en lugar de eliminarlo?"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ocurrió un error al intentar eliminar el producto: {ex.Message}" });
            }
        }

        [HttpPost("{id}/deactivate")]
        public async Task<ActionResult> DeactivateProducto(int id)
        {
            try
            {
                var result = await _productoService.SoftDeleteProducto(id);
                if (result)
                {
                    return Ok(new { message = "Producto desactivado exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "El producto no existe o no se pudo desactivar." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ocurrió un error al intentar desactivar el producto: {ex.Message}" });
            }
        }

        [HttpPost("{id}/activate")]
        public async Task<ActionResult> ActivateProducto(int id)
        {
            try
            {
                var result = await _productoService.ActivateProducto(id);
                if (result)
                {
                    return Ok(new { message = "Producto activado exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "El producto no existe o no se pudo activar." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ocurrió un error al intentar activar el producto: {ex.Message}" });
            }
        }

        // GET: api/productos/paginados?page=1&pageSize=20
        [HttpGet("paginados")]
        public async Task<ActionResult<object>> GetProductosPaginados([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _productoService.GetProductosPaginados(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener productos paginados: {ex.Message}" });
            }
        }
    }
}
