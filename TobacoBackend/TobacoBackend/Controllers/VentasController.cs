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
    public class VentasController : ControllerBase
    {
        private readonly IVentaService _ventasService;

        public VentasController(IVentaService ventasService)
        {
            _ventasService = ventasService;
        }


        [HttpGet]
        public async Task<ActionResult<List<VentaDTO>>> GetAllVentas()
        {
            var ventas = await _ventasService.GetAllVentas();
            return Ok(ventas);
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<VentaDTO>> GetVentaById(int id)
        {
            try
            {
                var venta = await _ventasService.GetVentaById(id);
                return Ok(venta);
            }
            catch (Exception)
            {
                return NotFound(new { message = "No se encontró la venta que coincida con el id." });
            }
        }



        [HttpPost]
        public async Task<ActionResult<CreateVentaResponseDTO>> AddVenta([FromBody] VentaDTO ventaDto)
        {
            try
            {
                if (ventaDto == null)
                {
                    return BadRequest(new { message = "La venta no puede ser nula." });
                }

                var response = await _ventasService.AddVenta(ventaDto);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }




        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateVenta(int id, [FromBody] VentaDTO ventaDto)
        {
            if (ventaDto == null || id != ventaDto.Id)
            {
                return BadRequest(new { message = "ID de la venta no coincide o la venta es nula." });
            }

            try
            {
                await _ventasService.UpdateVenta(id, ventaDto);
                return Ok(new { message = "Venta actualizada exitosamente." });
            }
            catch (Exception)
            {
                return NotFound(new { message = "Venta no encontrada" });
            }
        }



        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVenta(int id)
        {
            try
            {
                var deleteResult = await _ventasService.DeleteVenta(id);
                if (deleteResult)
                {
                    return Ok(new { message = "Venta eliminada exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "La venta no existe o no se pudo eliminar." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ocurrió un error al intentar eliminar la venta: {ex.Message}" });
            }
        }

        // GET: api/ventas/paginados?page=1&pageSize=20
        [HttpGet("paginados")]
        public async Task<ActionResult<object>> GetVentasPaginadas([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _ventasService.GetVentasPaginadas(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener ventas paginadas: {ex.Message}" });
            }
        }

        // GET: api/ventas/por-cliente/{clienteId}?pageNumber=1&pageSize=20&dateFrom=&dateTo=
        [HttpGet("por-cliente/{clienteId}")]
        public async Task<ActionResult<object>> GetVentasPorCliente(
            int clienteId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            try
            {
                var result = await _ventasService.GetVentasPorCliente(clienteId, pageNumber, pageSize, dateFrom, dateTo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/ventas/{id}/estado-entrega
        [HttpPut("{id}/estado-entrega")]
        public async Task<ActionResult> UpdateEstadoEntregaItems(int id, [FromBody] List<VentaProductoDTO> items)
        {
            try
            {
                if (items == null || !items.Any())
                {
                    return BadRequest(new { message = "La lista de items no puede estar vacía." });
                }

                await _ventasService.UpdateEstadoEntregaItems(id, items);
                return Ok(new { message = "Estado de entrega actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al actualizar el estado de entrega: {ex.Message}" });
            }
        }

        // POST: api/Ventas/asignar
        [HttpPost("asignar")]
        public async Task<ActionResult> AsignarVentaAUsuario([FromBody] AsignarVentaDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new { message = "La solicitud no puede ser nula." });
                }

                var asignado = await _ventasService.AsignarVentaAUsuario(dto.VentaId, dto.UsuarioId);
                if (asignado)
                {
                    return Ok(new { message = "Venta asignada exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "Venta no encontrada." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al asignar la venta: {ex.Message}" });
            }
        }

        // POST: api/Ventas/asignar-automaticamente
        [HttpPost("asignar-automaticamente")]
        public async Task<ActionResult> AsignarVentaAutomaticamente([FromBody] AsignarVentaAutomaticaDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new { message = "La solicitud no puede ser nula." });
                }

                var resultado = await _ventasService.AsignarVentaAutomaticamente(dto.VentaId, dto.UsuarioIdExcluir);
                
                if (resultado.Asignada)
                {
                    return Ok(resultado);
                }
                else
                {
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al asignar la venta automáticamente: {ex.Message}" });
            }
        }
    }

    public class AsignarVentaAutomaticaDTO
    {
        public int VentaId { get; set; }
        public int UsuarioIdExcluir { get; set; }
    }
}

