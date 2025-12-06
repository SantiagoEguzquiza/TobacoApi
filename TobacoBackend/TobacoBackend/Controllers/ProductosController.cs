using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;
using TobacoBackend.Services;
using TobacoBackend.Helpers;
using System.Security.Claims;
using TobacoBackend.Authorization;
using TobacoBackend.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = AuthorizationPolicies.AdminOrEmployeeOnly)] // Admin o Employee, NO SuperAdmin
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;
        private readonly PricingService _pricingService;
        private readonly AuditService _auditService;

        public ProductosController(IProductoService productoService, PricingService pricingService, AuditService auditService)
        {
            _productoService = productoService;
            _pricingService = pricingService;
            _auditService = auditService;
        }


        [HttpGet]
        public async Task<ActionResult<List<ProductoDTO>>> GetAllProductos()
        {
            // Validar permiso de visualizar productos
            var canView = await PermissionHelper.CanViewModuleAsync(User, HttpContext.RequestServices, "Productos");
            if (!canView)
            {
                return Forbid("No tienes permiso para visualizar productos.");
            }

            var productos = await _productoService.GetAllProductos();
            return Ok(productos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDTO>> GetProductoById(int id)
        {
            // Validar permiso de visualizar productos
            var canView = await PermissionHelper.CanViewModuleAsync(User, HttpContext.RequestServices, "Productos");
            if (!canView)
            {
                return Forbid("No tienes permiso para visualizar productos.");
            }

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
        [Authorize(Policy = AuthorizationPolicies.AdminOrEmployee)] // Validación de permisos se hace dentro
        public async Task<ActionResult> AddProducto([FromBody] ProductoDTO productoDto)
        {
            // Validar permiso de crear productos
            var hasPermission = await PermissionHelper.HasPermissionAsync(User, HttpContext.RequestServices, "Productos_Crear");
            if (!hasPermission)
            {
                return Forbid("No tienes permiso para crear productos.");
            }

            try
            {
                Console.WriteLine("=== ADD PRODUCTO ENDPOINT CALLED ===");
                Console.WriteLine($"Producto DTO recibido: {productoDto != null}");
                
                if (productoDto == null)
                {
                    Console.WriteLine("ERROR: Producto DTO es null");
                    return BadRequest(new { message = "El producto no puede ser nulo." });
                }

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ERROR: ModelState no es válido");
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"  - {error.Key}: {string.Join(", ", error.Value)}");
                    }
                    
                    return BadRequest(new
                    {
                        message = "Datos de validación incorrectos",
                        errors = errors
                    });
                }

                // Sanitizar entrada
                productoDto.Nombre = InputSanitizer.SanitizeString(productoDto.Nombre, 100);
                if (!string.IsNullOrEmpty(productoDto.Marca))
                {
                    productoDto.Marca = InputSanitizer.SanitizeString(productoDto.Marca, 50);
                }
                
                if (InputSanitizer.ContainsSqlInjection(productoDto.Nombre) || 
                    InputSanitizer.ContainsXss(productoDto.Nombre))
                {
                    return BadRequest(new { message = "Entrada inválida detectada." });
                }

                // Validar precios por cantidad
                if (productoDto.QuantityPrices != null && productoDto.QuantityPrices.Any())
                {
                    if (!_pricingService.ValidateQuantityPrices(productoDto.QuantityPrices))
                    {
                        return BadRequest(new
                        {
                            message = "Los precios por cantidad no son válidos. Solo se permiten packs (cantidad >= 2), no debe haber cantidades duplicadas, y todos los valores deben ser positivos."
                        });
                    }
                }

                var producto = await _productoService.AddProducto(productoDto);

                // Auditoría
                _auditService.LogCreate("Producto", producto.Id, User,
                    SecurityLoggingService.GetClientIpAddress(HttpContext));

                return CreatedAtAction(nameof(GetProductoById), new { id = producto.Id }, new { message = "Producto agregado exitosamente." });
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
        [Authorize(Policy = AuthorizationPolicies.AdminOrEmployee)] // Validación de permisos se hace dentro
        public async Task<ActionResult> UpdateProducto(int id, [FromBody] ProductoDTO productoDto)
        {
            // Validar permiso de editar productos
            var hasPermission = await PermissionHelper.HasPermissionAsync(User, HttpContext.RequestServices, "Productos_Editar");
            if (!hasPermission)
            {
                return Forbid("No tienes permiso para editar productos.");
            }

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

            // Validar precios por cantidad
            if (productoDto.QuantityPrices != null && productoDto.QuantityPrices.Any())
            {
                if (!_pricingService.ValidateQuantityPrices(productoDto.QuantityPrices))
                {
                    return BadRequest(new
                    {
                        message = "Los precios por cantidad no son válidos. Solo se permiten packs (cantidad >= 2), no debe haber cantidades duplicadas, y todos los valores deben ser positivos."
                    });
                }
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
        [Authorize(Policy = AuthorizationPolicies.AdminOrEmployee)] // Validación de permisos se hace dentro
        public async Task<ActionResult> DeleteProducto(int id)
        {
            // Validar permiso de eliminar productos
            var hasPermission = await PermissionHelper.HasPermissionAsync(User, HttpContext.RequestServices, "Productos_Eliminar");
            if (!hasPermission)
            {
                return Forbid("No tienes permiso para eliminar productos.");
            }

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
