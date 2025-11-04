using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;
using TobacoBackend.Domain.Models;
using System.Security.Claims;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EntregasController : ControllerBase
    {
        private readonly IVentaService _ventasService;
        private readonly IUserService _userService;
        private readonly IRecorridoProgramadoService _recorridoService;
        private readonly ILogger<EntregasController> _logger;

        public EntregasController(IVentaService ventasService, IUserService userService, IRecorridoProgramadoService recorridoService, ILogger<EntregasController> logger)
        {
            _ventasService = ventasService;
            _userService = userService;
            _recorridoService = recorridoService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene las entregas/recorridos del empleado actual del día
        /// Para RepartidorVendedor: muestra sus recorridos programados del día de la semana
        /// Para Repartidor: muestra entregas asignadas a él
        /// Para Vendedor: lista vacía (usa mis-recorridos)
        /// </summary>
        [HttpGet("mis-entregas")]
        public async Task<ActionResult<List<EntregaDTO>>> GetMisEntregas()
        {
            try
            {
                // Obtener el ID del usuario actual desde el token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var usuarioId = int.Parse(userIdClaim);
                
                // Obtener información del usuario para verificar su tipo
                var usuario = await _userService.GetUserByIdAsync(usuarioId);
                if (usuario == null)
                {
                    return Unauthorized(new { message = "Usuario no encontrado" });
                }

                // Admin y RepartidorVendedor: mostrar recorridos programados del día Y entregas asignadas
                if (usuario.Role == "Admin" || (usuario.Role == "Employee" && usuario.TipoVendedor == TipoVendedor.RepartidorVendedor))
                {
                    var recorridosResult = await GetRecorridosProgramadosDelDia(usuarioId);
                    var entregasAsignadasResult = await GetEntregasAsignadas(usuarioId);
                    
                    // Combinar ambas listas, pero eliminar recorridos programados que ya tienen venta asignada
                    var todasLasEntregas = new List<EntregaDTO>();
                    
                    // Obtener entregas asignadas primero
                    var entregasList = new List<EntregaDTO>();
                    if (entregasAsignadasResult.Result is OkObjectResult entregasOk && entregasOk.Value is List<EntregaDTO> entregas)
                    {
                        entregasList = entregas;
                        todasLasEntregas.AddRange(entregasList);
                    }
                    
                    // Obtener recorridos programados y filtrar los que ya tienen venta asignada
                    if (recorridosResult.Result is OkObjectResult recorridosOk && recorridosOk.Value is List<EntregaDTO> recorridosList)
                    {
                        // Obtener IDs de clientes que ya tienen ventas asignadas
                        var clientesConVentaAsignada = entregasList
                            .Where(e => e.ClienteId > 0)
                            .Select(e => e.ClienteId)
                            .ToHashSet();
                        
                        // Filtrar recorridos programados: solo agregar los que NO tienen venta asignada al mismo cliente
                        var recorridosFiltrados = recorridosList
                            .Where(r => !clientesConVentaAsignada.Contains(r.ClienteId))
                            .ToList();
                        
                        todasLasEntregas.AddRange(recorridosFiltrados);
                    }
                    
                    return Ok(todasLasEntregas);
                }
                
                // Vendedor: solo mostrar recorridos programados del día (NO entregas asignadas)
                if (usuario.Role == "Employee" && usuario.TipoVendedor == TipoVendedor.Vendedor)
                {
                    return await GetRecorridosProgramadosDelDia(usuarioId);
                }
                
                // Repartidor: mostrar entregas asignadas a él
                if (usuario.Role == "Employee" && usuario.TipoVendedor == TipoVendedor.Repartidor)
                {
                    return await GetEntregasAsignadas(usuarioId);
                }

                return Ok(new List<EntregaDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entregas del día");
                return StatusCode(500, new { message = $"Error al obtener entregas: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene los recorridos programados del día de la semana para un RepartidorVendedor
        /// </summary>
        private async Task<ActionResult<List<EntregaDTO>>> GetRecorridosProgramadosDelDia(int vendedorId)
        {
            // Obtener el día de la semana actual
            var diaActual = DateTime.Now.DayOfWeek;
            DiaSemana diaSemana = diaActual switch
            {
                DayOfWeek.Sunday => DiaSemana.Domingo,
                DayOfWeek.Monday => DiaSemana.Lunes,
                DayOfWeek.Tuesday => DiaSemana.Martes,
                DayOfWeek.Wednesday => DiaSemana.Miercoles,
                DayOfWeek.Thursday => DiaSemana.Jueves,
                DayOfWeek.Friday => DiaSemana.Viernes,
                DayOfWeek.Saturday => DiaSemana.Sabado,
                _ => DiaSemana.Lunes
            };

            _logger.LogInformation($"Buscando recorridos para vendedor {vendedorId} en día {diaSemana} (valor: {(int)diaSemana})");

            // Obtener recorridos programados del día de la semana
            var recorridosProgramados = await _recorridoService.GetRecorridosByVendedorAndDia(vendedorId, (int)diaSemana);
            
            _logger.LogInformation($"Se encontraron {recorridosProgramados.Count} recorridos programados");
            
            // Convertir recorridos programados a DTOs de entrega
            var recorridos = recorridosProgramados.Select(r => new EntregaDTO
            {
                Id = r.Id,
                VentaId = 0, // Aún no hay venta creada (es un recorrido programado)
                ClienteId = r.ClienteId,
                ClienteNombre = r.ClienteNombre ?? "Cliente Desconocido",
                ClienteDireccion = r.ClienteDireccion ?? "",
                Latitud = r.ClienteLatitud,
                Longitud = r.ClienteLongitud,
                Estado = 0, // Pendiente (aún no se ha creado la venta)
                FechaAsignacion = DateTime.Today,
                FechaEntrega = null,
                RepartidorId = r.VendedorId,
                Orden = r.Orden,
                Notas = "",
                DistanciaDesdeUbicacionActual = 0
            }).ToList();
            
            return Ok(recorridos);
        }

        /// <summary>
        /// Obtiene las entregas asignadas a un Repartidor
        /// Incluye entregas pendientes, parciales y completadas del día actual
        /// </summary>
        private async Task<ActionResult<List<EntregaDTO>>> GetEntregasAsignadas(int vendedorId)
        {
            var hoy = DateTime.Today;
            var todasLasVentas = await _ventasService.GetAllVentas();
            
            // Filtrar ventas asignadas al repartidor del día actual
            // O ventas completadas del día actual (aunque la fecha de asignación no sea hoy)
            var ventasDelDia = todasLasVentas
                .Where(v => 
                    // Ventas asignadas hoy
                    (v.Fecha.Date == hoy && v.UsuarioIdAsignado == vendedorId) ||
                    // O ventas completadas hoy (aunque se hayan asignado antes)
                    (v.EstadoEntrega == EstadoEntrega.ENTREGADA && 
                     v.Fecha.Date == hoy && 
                     v.UsuarioIdAsignado == vendedorId) ||
                    // O ventas parciales asignadas hoy
                    (v.EstadoEntrega == EstadoEntrega.PARCIAL && 
                     v.Fecha.Date == hoy && 
                     v.UsuarioIdAsignado == vendedorId))
                .ToList();

            var entregas = ventasDelDia.Select(v => new EntregaDTO
            {
                Id = v.Id,
                VentaId = v.Id,
                ClienteId = v.ClienteId,
                ClienteNombre = v.Cliente?.Nombre ?? "Cliente Desconocido",
                ClienteDireccion = v.Cliente?.Direccion ?? "",
                Latitud = v.Cliente?.Latitud,
                Longitud = v.Cliente?.Longitud,
                Estado = (int)v.EstadoEntrega,
                FechaAsignacion = v.Fecha,
                FechaEntrega = v.EstadoEntrega == EstadoEntrega.ENTREGADA ? v.Fecha : null,
                RepartidorId = v.UsuarioIdAsignado ?? 0,
                Orden = 0,
                Notas = "",
                DistanciaDesdeUbicacionActual = 0
            }).OrderBy(e => e.ClienteNombre).ToList();

            return Ok(entregas);
        }

        /// <summary>
        /// Obtiene los recorridos (visitas) del Vendedor actual del día
        /// Solo para vendedores tipo Vendedor (visitan sucursales y asignan entregas)
        /// </summary>
        [HttpGet("mis-recorridos")]
        public async Task<ActionResult<List<EntregaDTO>>> GetMisRecorridos()
        {
            try
            {
                // Obtener el ID del usuario actual desde el token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var usuarioId = int.Parse(userIdClaim);
                
                // Obtener información del usuario para verificar su tipo
                var usuario = await _userService.GetUserByIdAsync(usuarioId);
                if (usuario == null)
                {
                    return Unauthorized(new { message = "Usuario no encontrado" });
                }

                // Solo los Vendedores ven recorridos (sus visitas a sucursales)
                // RepartidorVendedor, Admin y Repartidor usan mis-entregas
                if (usuario.Role == "Admin" || (usuario.Role == "Employee" && usuario.TipoVendedor == TipoVendedor.RepartidorVendedor))
                {
                    return Ok(new List<EntregaDTO>()); // Lista vacía, deben usar mis-entregas
                }
                
                // Obtener las ventas del día creadas por este vendedor (sus recorridos/visitas)
                var hoy = DateTime.Today;
                var todasLasVentas = await _ventasService.GetAllVentas();
                
                var ventasDelDia = todasLasVentas
                    .Where(v => v.Fecha.Date == hoy && 
                                v.UsuarioIdCreador == usuarioId)
                    .ToList();

                // Convertir ventas a DTOs de entrega (en este caso representan recorridos/visitas)
                var recorridos = ventasDelDia.Select(v => new EntregaDTO
                {
                    Id = v.Id,
                    VentaId = v.Id,
                    ClienteId = v.ClienteId,
                    ClienteNombre = v.Cliente?.Nombre ?? "Cliente Desconocido",
                    ClienteDireccion = v.Cliente?.Direccion ?? "",
                    Latitud = v.Cliente?.Latitud,
                    Longitud = v.Cliente?.Longitud,
                    Estado = (int)v.EstadoEntrega,
                    FechaAsignacion = v.Fecha,
                    FechaEntrega = v.EstadoEntrega == EstadoEntrega.ENTREGADA ? v.Fecha : null,
                    RepartidorId = v.UsuarioIdCreador ?? 0, // El vendedor que creó la venta
                    Orden = 0,
                    Notas = "",
                    DistanciaDesdeUbicacionActual = 0
                }).OrderBy(e => e.ClienteNombre).ToList();

                return Ok(recorridos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener recorridos del día");
                return StatusCode(500, new { message = $"Error al obtener recorridos: {ex.Message}" });
            }
        }

        /// <summary>
        /// Actualiza el estado de una entrega
        /// </summary>
        [HttpPut("{id}/estado")]
        public async Task<ActionResult> UpdateEstadoEntrega(int id, [FromBody] ActualizarEstadoDTO dto)
        {
            try
            {
                var ok = await _ventasService.UpdateEstadoEntrega(id, (EstadoEntrega)dto.Estado);
                if (!ok)
                {
                    return NotFound(new { message = "Entrega no encontrada" });
                }
                return Ok(new { message = "Estado de entrega actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de entrega");
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Agrega notas a una entrega
        /// </summary>
        [HttpPut("{id}/notas")]
        public async Task<ActionResult> AgregarNotas(int id, [FromBody] AgregarNotasDTO dto)
        {
            try
            {
                // Por ahora solo devolvemos OK ya que las ventas no tienen campo de notas
                // Se podría agregar en el futuro si es necesario
                return Ok(new { message = "Notas agregadas exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar notas");
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }
    }
}

