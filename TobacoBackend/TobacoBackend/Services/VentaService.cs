using AutoMapper;
using System.Security.Claims;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace TobacoBackend.Services
{
    public class VentaService : IVentaService
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IVentaPagoService _ventaPagoService;
        private readonly IPrecioEspecialService _precioEspecialService;
        private readonly IClienteService _clienteService;
        private readonly IProductoAFavorRepository _productoAFavorRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PricingService _pricingService;
        private readonly IUserService _userService;

        public VentaService(IVentaRepository ventaRepository, IMapper mapper, IProductoRepository productoRepository, IVentaPagoService ventaPagoService, IPrecioEspecialService precioEspecialService, IClienteService clienteService, IHttpContextAccessor httpContextAccessor, PricingService pricingService, IProductoAFavorRepository productoAFavorRepository, IUserService userService)
        {
            _ventaRepository = ventaRepository;
            _mapper = mapper;
            _productoRepository = productoRepository;
            _ventaPagoService = ventaPagoService;
            _precioEspecialService = precioEspecialService;
            _clienteService = clienteService;
            _httpContextAccessor = httpContextAccessor;
            _pricingService = pricingService;
            _productoAFavorRepository = productoAFavorRepository;
            _userService = userService;
        }

        public async Task<CreateVentaResponseDTO> AddVenta(VentaDTO ventaDto)
        {
            // Obtener el ID del usuario actual para registrar quién creó la venta
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            int? usuarioIdCreador = null;
            int? usuarioIdAsignado = null;
            string? nombreRepartidorAsignado = null;
            
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                usuarioIdCreador = userId;
                // No auto-asignamos. El usuario elegirá en el frontend si quiere asignarse la venta o asignarla a otro repartidor
            }

            var venta = new Venta
            {
                ClienteId = ventaDto.ClienteId,
                Fecha = DateTime.Now,
                VentaProductos = new List<VentaProducto>(),
                MetodoPago = ventaDto.MetodoPago,
                UsuarioIdCreador = usuarioIdCreador, // Quien creó la venta
                UsuarioIdAsignado = usuarioIdAsignado // Auto-asignado si es vendedor-repartidor, null si no
            };

            decimal total = 0;

            foreach (var productoDto in ventaDto.VentaProductos)
            {
                var producto = await _productoRepository.GetProductoById(productoDto.ProductoId);
                if (producto == null)
                {
                    throw new Exception($"Producto con ID {productoDto.ProductoId} no encontrado.");
                }

                // Aplicar lógica de expiración de descuentos antes de calcular el precio
                await AplicarLogicaExpiracionDescuento(producto);

                // Obtener el precio especial si existe
                decimal? specialPrice = null;
                try
                {
                    specialPrice = await _precioEspecialService.GetPrecioFinalProductoAsync(ventaDto.ClienteId, producto.Id);
                }
                catch
                {
                    // No special price available, use regular pricing
                }

                // Calculate optimal pricing using quantity-based pricing
                var pricingResult = _pricingService.CalculateOptimalPricing(
                    producto, 
                    (int)productoDto.Cantidad, 
                    specialPrice, 
                    null // Global discount will be applied later
                );

                // Aplicar descuento del producto si está activo
                decimal precioConDescuentoProducto = pricingResult.TotalPrice;
                if (TieneDescuentoActivo(producto))
                {
                    // Aplicar descuento del producto sobre el precio final (ya incluye packs)
                    precioConDescuentoProducto = pricingResult.TotalPrice * (1 - producto.Descuento / 100);
                }

                var ventaProducto = new VentaProducto
                {
                    ProductoId = producto.Id,
                    Cantidad = productoDto.Cantidad,
                    PrecioFinalCalculado = precioConDescuentoProducto // Precio después de descuentos por cantidad y descuento del producto
                };

                total += precioConDescuentoProducto;

                venta.VentaProductos.Add(ventaProducto);
            }

            // Aplicar descuento global del cliente si existe
            var cliente = await _clienteService.GetClienteById(ventaDto.ClienteId);
            decimal descuentoGlobal = 0;
            if (cliente != null && cliente.DescuentoGlobal > 0)
            {
                descuentoGlobal = total * (cliente.DescuentoGlobal / 100);
                total = total - descuentoGlobal;
            }

            venta.Total = total;

            // Calcular y asignar precios finales a cada producto después del descuento global
            if (descuentoGlobal > 0)
            {
                foreach (var ventaProducto in venta.VentaProductos)
                {
                    // Calcular la proporción de este producto en el total antes del descuento global
                    var precioAntesDescuentoGlobal = ventaProducto.PrecioFinalCalculado;
                    var proporcionProducto = precioAntesDescuentoGlobal / (total + descuentoGlobal);
                    
                    // Aplicar el descuento global proporcionalmente a este producto
                    var descuentoProducto = descuentoGlobal * proporcionProducto;
                    ventaProducto.PrecioFinalCalculado = precioAntesDescuentoGlobal - descuentoProducto;
                }
            }

            // Add the venta first to get the ID
            await _ventaRepository.AddVenta(venta);

            // Add VentaPagos if provided
            if (ventaDto.VentaPagos != null && ventaDto.VentaPagos.Any())
            {
                foreach (var ventaPagoDto in ventaDto.VentaPagos)
                {
                    ventaPagoDto.VentaId = venta.Id;
                    await _ventaPagoService.AddVentaPago(ventaPagoDto);
                }
            }

            // Si el método de pago es CuentaCorriente, sumar SOLO la parte pagada con cuenta corriente
            if (venta.MetodoPago == MetodoPagoEnum.CuentaCorriente)
            {
                // Calcular el monto pagado con cuenta corriente
                decimal montoCuentaCorriente = 0;
                if (ventaDto.VentaPagos != null && ventaDto.VentaPagos.Any())
                {
                    montoCuentaCorriente = ventaDto.VentaPagos
                        .Where(vp => vp.Metodo == MetodoPagoEnum.CuentaCorriente)
                        .Sum(vp => vp.Monto);
                }
                
                // Solo agregar a la deuda si hay monto pagado con cuenta corriente
                if (montoCuentaCorriente > 0)
                {
                    await _clienteService.AgregarDeuda(venta.ClienteId, montoCuentaCorriente);
                }
            }

            // Preparar respuesta - nunca auto-asignamos, siempre se pregunta en el frontend
            var response = new CreateVentaResponseDTO
            {
                VentaId = venta.Id,
                Message = "Venta creada exitosamente",
                Asignada = false, // Nunca auto-asignamos
                UsuarioAsignadoId = null,
                UsuarioAsignadoNombre = null
            };

            return response;
        }




        public async Task<bool> DeleteVenta(int id)
        {
            Console.WriteLine($"=== DeleteVenta - VentaId: {id} ===");
            
            try
            {
                // Get the venta before deleting to check if it affects debt
                var ventaExistente = await _ventaRepository.GetVentaById(id);
                if (ventaExistente == null)
                {
                    Console.WriteLine("Venta no encontrada");
                    return false; // Venta no encontrada
                }

                Console.WriteLine($"Venta encontrada: ClienteId={ventaExistente.ClienteId}, MetodoPago={ventaExistente.MetodoPago}");

                // Calculate the debt amount that needs to be reduced
                decimal montoDeudaAReducir = 0;
                if (ventaExistente.MetodoPago == MetodoPagoEnum.CuentaCorriente)
                {
                    // Get the actual amount paid with cuenta corriente from existing VentaPagos
                    var ventaPagosAnteriores = await _ventaPagoService.GetVentaPagosByVentaId(id);
                    montoDeudaAReducir = ventaPagosAnteriores
                        .Where(vp => vp.Metodo == MetodoPagoEnum.CuentaCorriente)
                        .Sum(vp => vp.Monto);
                    Console.WriteLine($"Monto deuda a reducir: {montoDeudaAReducir}");
                }
                
                // Delete associated VentaPagos first
                Console.WriteLine("Eliminando VentaPagos...");
                await _ventaPagoService.DeleteVentaPagosByVentaId(id);
                
                // Delete associated ProductosAFavor
                Console.WriteLine("Eliminando ProductosAFavor...");
                await _productoAFavorRepository.DeleteProductosAFavorByVentaId(id);
                
                // Then delete the venta
                Console.WriteLine("Eliminando venta...");
                bool eliminado = await _ventaRepository.DeleteVenta(id);
                Console.WriteLine($"Venta eliminada: {eliminado}");
                
                // Reduce debt only if there was actual debt from cuenta corriente payments
                if (eliminado && montoDeudaAReducir > 0)
                {
                    Console.WriteLine($"Reduciendo deuda del cliente {ventaExistente.ClienteId} en {montoDeudaAReducir}");
                    await _clienteService.ReducirDeuda(ventaExistente.ClienteId, montoDeudaAReducir);
                }
                
                return eliminado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar venta: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<List<VentaDTO>> GetAllVentas()
        {
            var ventas = await _ventaRepository.GetAllVentas();
            var ventasDto = _mapper.Map<List<VentaDTO>>(ventas);
            
            // Load VentaPagos for each venta
            foreach (var ventaDto in ventasDto)
            {
                ventaDto.VentaPagos = await _ventaPagoService.GetVentaPagosByVentaId(ventaDto.Id);
            }
            
            return ventasDto;
        }

        public async Task<VentaDTO> GetVentaById(int id)
        {
            var venta = await _ventaRepository.GetVentaById(id);
            var ventaDto = _mapper.Map<VentaDTO>(venta);
            
            // Load VentaPagos for this venta
            if (venta != null)
            {
                ventaDto.VentaPagos = await _ventaPagoService.GetVentaPagosByVentaId(id);
            }
            
            return ventaDto;
        }

        public async Task UpdateVenta(int id, VentaDTO ventaDto)
        {
            // Get the existing venta to compare changes
            var ventaExistente = await _ventaRepository.GetVentaById(id);

            // Calculate the new total and update PrecioFinalCalculado for each product
            decimal nuevoTotal = 0;
            if (ventaDto.VentaProductos != null)
            {
                foreach (var productoDto in ventaDto.VentaProductos)
                {
                    var producto = await _productoRepository.GetProductoById(productoDto.ProductoId);
                    if (producto != null)
                    {
                        // Aplicar lógica de expiración de descuentos antes de calcular el precio
                        await AplicarLogicaExpiracionDescuento(producto);

                        // Obtener el precio especial si existe
                        decimal? specialPrice = null;
                        try
                        {
                            specialPrice = await _precioEspecialService.GetPrecioFinalProductoAsync(ventaDto.ClienteId, producto.Id);
                        }
                        catch
                        {
                            // No special price available, use regular pricing
                        }

                        // Calculate optimal pricing using quantity-based pricing
                        var pricingResult = _pricingService.CalculateOptimalPricing(
                            producto, 
                            (int)productoDto.Cantidad, 
                            specialPrice, 
                            null // Global discount will be applied later
                        );

                        // Aplicar descuento del producto si está activo
                        decimal precioConDescuentoProducto = pricingResult.TotalPrice;
                        if (TieneDescuentoActivo(producto))
                        {
                            // Aplicar descuento del producto sobre el precio final (ya incluye packs)
                            precioConDescuentoProducto = pricingResult.TotalPrice * (1 - producto.Descuento / 100);
                        }

                        // Actualizar el PrecioFinalCalculado en el DTO
                        productoDto.PrecioFinalCalculado = precioConDescuentoProducto;

                        nuevoTotal += precioConDescuentoProducto;
                    }
                }
            }

            // Apply global discount if exists
            var cliente = await _clienteService.GetClienteById(ventaDto.ClienteId);
            decimal descuentoGlobal = 0;
            if (cliente != null && cliente.DescuentoGlobal > 0)
            {
                descuentoGlobal = nuevoTotal * (cliente.DescuentoGlobal / 100);
                nuevoTotal = nuevoTotal - descuentoGlobal;
            }

            // Aplicar descuento global proporcionalmente a cada producto
            if (descuentoGlobal > 0 && ventaDto.VentaProductos != null)
            {
                foreach (var productoDto in ventaDto.VentaProductos)
                {
                    // Calcular la proporción de este producto en el total antes del descuento global
                    var precioAntesDescuentoGlobal = productoDto.PrecioFinalCalculado;
                    var proporcionProducto = precioAntesDescuentoGlobal / (nuevoTotal + descuentoGlobal);
                    
                    // Aplicar el descuento global proporcionalmente a este producto
                    var descuentoProducto = descuentoGlobal * proporcionProducto;
                    productoDto.PrecioFinalCalculado = precioAntesDescuentoGlobal - descuentoProducto;
                }
            }

            // Calculate the current debt amount from the existing venta
            decimal deudaAnterior = 0;
            if (ventaExistente.MetodoPago == MetodoPagoEnum.CuentaCorriente)
            {
                // Get the actual amount paid with cuenta corriente from existing VentaPagos
                var ventaPagosAnteriores = await _ventaPagoService.GetVentaPagosByVentaId(id);
                deudaAnterior = ventaPagosAnteriores
                    .Where(vp => vp.Metodo == MetodoPagoEnum.CuentaCorriente)
                    .Sum(vp => vp.Monto);
            }

            // Calculate the new debt amount from the new venta
            decimal deudaNueva = 0;
            if (ventaDto.MetodoPago == MetodoPagoEnum.CuentaCorriente && ventaDto.VentaPagos != null)
            {
                deudaNueva = ventaDto.VentaPagos
                    .Where(vp => vp.Metodo == MetodoPagoEnum.CuentaCorriente)
                    .Sum(vp => vp.Monto);
            }

            // Adjust debt based on the difference
            var diferenciaDeuda = deudaNueva - deudaAnterior;
            if (diferenciaDeuda > 0)
            {
                await _clienteService.AgregarDeuda(ventaDto.ClienteId, diferenciaDeuda);
            }
            else if (diferenciaDeuda < 0)
            {
                await _clienteService.ReducirDeuda(ventaDto.ClienteId, Math.Abs(diferenciaDeuda));
            }

            var venta = _mapper.Map<Venta>(ventaDto);
            venta.Id = id;
            venta.Total = nuevoTotal;
            await _ventaRepository.UpdateVenta(venta);
            
            // Update VentaPagos
            if (ventaDto.VentaPagos != null)
            {
                // Delete existing VentaPagos for this venta
                await _ventaPagoService.DeleteVentaPagosByVentaId(id);
                
                // Add new VentaPagos
                foreach (var ventaPagoDto in ventaDto.VentaPagos)
                {
                    ventaPagoDto.VentaId = id;
                    await _ventaPagoService.AddVentaPago(ventaPagoDto);
                }
            }
        }

        public async Task<object> GetVentasPaginadas(int page, int pageSize)
        {
            var result = await _ventaRepository.GetVentasPaginadas(page, pageSize);
            var ventasDto = _mapper.Map<List<VentaDTO>>(result.Ventas);
            
            // Load VentaPagos for each venta
            foreach (var ventaDto in ventasDto)
            {
                ventaDto.VentaPagos = await _ventaPagoService.GetVentaPagosByVentaId(ventaDto.Id);
            }
            
            return new
            {
                ventas = ventasDto,
                totalItems = result.TotalItems,
                totalPages = result.TotalPages,
                currentPage = page,
                pageSize = pageSize,
                hasNextPage = page < result.TotalPages,
                hasPreviousPage = page > 1
            };
        }

        public async Task<object> GetVentasPorCliente(int clienteId, int page, int pageSize, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var result = await _ventaRepository.GetVentasPorCliente(clienteId, page, pageSize, dateFrom, dateTo);
            var ventasDto = _mapper.Map<List<VentaDTO>>(result.Ventas);
            
            // Load VentaPagos for each venta
            foreach (var ventaDto in ventasDto)
            {
                ventaDto.VentaPagos = await _ventaPagoService.GetVentaPagosByVentaId(ventaDto.Id);
            }
            
            return new
            {
                ventas = ventasDto,
                totalItems = result.TotalItems,
                totalPages = result.TotalPages,
                currentPage = page,
                pageSize = pageSize,
                hasNextPage = page < result.TotalPages,
                hasPreviousPage = page > 1
            };
        }

        public async Task<object> GetVentasConCuentaCorrienteByClienteId(int clienteId, int page = 1, int pageSize = 20)
        {
            var result = await _ventaRepository.GetVentasConCuentaCorrienteByClienteId(clienteId, page, pageSize);
            var ventasDto = _mapper.Map<List<VentaDTO>>(result.Ventas);

            return new
            {
                ventas = ventasDto,
                totalItems = result.TotalItems,
                totalPages = result.TotalPages,
                currentPage = page,
                pageSize = pageSize,
                hasNextPage = page < result.TotalPages,
                hasPreviousPage = page > 1
            };
        }

        public async Task UpdateEstadoEntregaItems(int ventaId, List<VentaProductoDTO> items)
        {
            Console.WriteLine($"=== UpdateEstadoEntregaItems - VentaId: {ventaId}, Items: {items.Count} ===");
            
            var venta = await _ventaRepository.GetVentaById(ventaId);
            if (venta == null)
            {
                throw new Exception($"Venta con ID {ventaId} no encontrada.");
            }

            // Obtener el ID del usuario actual
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            int? usuarioId = null;
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                usuarioId = userId;
                Console.WriteLine($"Usuario ID: {usuarioId}");
            }
            else
            {
                Console.WriteLine("No se pudo obtener el ID del usuario");
            }

            // Obtener productos a favor existentes para esta venta
            var productosAFavorExistentes = await _productoAFavorRepository.GetProductosAFavorByVentaId(ventaId);
            Console.WriteLine($"Productos a favor existentes: {productosAFavorExistentes.Count}");

            // Actualizar el estado de entrega de cada item
            foreach (var itemDto in items)
            {
                Console.WriteLine($"Procesando item - ProductoId: {itemDto.ProductoId}, Entregado: {itemDto.Entregado}, Motivo: {itemDto.Motivo}");
                
                var ventaProducto = venta.VentaProductos.FirstOrDefault(vp => vp.ProductoId == itemDto.ProductoId);
                if (ventaProducto != null)
                {
                    bool estadoAnterior = ventaProducto.Entregado;
                    bool esPrimeraVez = ventaProducto.FechaChequeo == null;
                    
                    Console.WriteLine($"Estado anterior: {estadoAnterior}, Es primera vez: {esPrimeraVez}");
                    
                    ventaProducto.Entregado = itemDto.Entregado;
                    ventaProducto.Motivo = itemDto.Motivo;
                    ventaProducto.Nota = itemDto.Nota;
                    ventaProducto.FechaChequeo = DateTime.Now;
                    ventaProducto.UsuarioChequeoId = usuarioId;

                    // Si cambió de entregado a no entregado, crear producto a favor
                    if (estadoAnterior == true && itemDto.Entregado == false)
                    {
                        Console.WriteLine("CASO 1: Cambió de entregado a no entregado - Creando producto a favor");
                        await CrearProductoAFavor(venta, ventaProducto, itemDto, usuarioId);
                    }
                    // Si cambió de no entregado a entregado, eliminar el producto a favor
                    else if (estadoAnterior == false && itemDto.Entregado == true)
                    {
                        Console.WriteLine("CASO 2: Cambió de no entregado a entregado - Eliminando producto a favor");
                        await EliminarProductoAFavor(ventaId, ventaProducto.ProductoId);
                    }
                    // Si es la primera vez que se marca como no entregado
                    else if (esPrimeraVez && itemDto.Entregado == false)
                    {
                        Console.WriteLine("CASO 3: Primera vez marcando como no entregado - Creando producto a favor");
                        await CrearProductoAFavor(venta, ventaProducto, itemDto, usuarioId);
                    }
                    else
                    {
                        Console.WriteLine("CASO 4: No se ejecutó ninguna acción - Estado anterior: " + estadoAnterior + ", Nuevo estado: " + itemDto.Entregado + ", Es primera vez: " + esPrimeraVez);
                    }
                }
                else
                {
                    Console.WriteLine($"No se encontró VentaProducto para ProductoId: {itemDto.ProductoId}");
                }
            }

            // Calcular y actualizar el estado de entrega de la venta
            venta.EstadoEntrega = CalcularEstadoEntrega(venta.VentaProductos);

            // Guardar los cambios directamente usando SaveChanges
            // NO usamos UpdateVenta porque ese método elimina y recrea los productos
            await _ventaRepository.SaveChangesAsync();
        }

        public async Task<bool> UpdateEstadoEntrega(int ventaId, EstadoEntrega nuevoEstado)
        {
            var venta = await _ventaRepository.GetVentaById(ventaId);
            if (venta == null)
            {
                return false;
            }
            venta.EstadoEntrega = nuevoEstado;
            await _ventaRepository.UpdateVenta(venta);
            return true;
        }

        private async Task CrearProductoAFavor(Venta venta, VentaProducto ventaProducto, VentaProductoDTO itemDto, int? usuarioId)
        {
            Console.WriteLine($"=== CrearProductoAFavor - VentaId: {venta.Id}, ProductoId: {ventaProducto.ProductoId} ===");
            
            // Verificar si ya existe un producto a favor para este item que no esté entregado
            var productosExistentes = await _productoAFavorRepository.GetProductosAFavorByVentaId(venta.Id);
            var existe = productosExistentes.Any(p => 
                p.ProductoId == ventaProducto.ProductoId && 
                !p.Entregado);

            Console.WriteLine($"Productos existentes para esta venta: {productosExistentes.Count}");
            Console.WriteLine($"Ya existe producto a favor para este producto: {existe}");

            if (!existe)
            {
                try
                {
                    var productoAFavor = new ProductoAFavor
                    {
                        ClienteId = venta.ClienteId,
                        ProductoId = ventaProducto.ProductoId,
                        Cantidad = ventaProducto.Cantidad,
                        FechaRegistro = DateTime.Now,
                        Motivo = itemDto.Motivo ?? "No especificado",
                        Nota = itemDto.Nota,
                        VentaId = venta.Id,
                        VentaProductoId = ventaProducto.ProductoId, // Como VentaProducto no tiene ID propio, usamos ProductoId
                        UsuarioRegistroId = usuarioId,
                        Entregado = false
                    };

                    Console.WriteLine($"Creando ProductoAFavor: ClienteId={productoAFavor.ClienteId}, ProductoId={productoAFavor.ProductoId}, Cantidad={productoAFavor.Cantidad}, Motivo={productoAFavor.Motivo}");
                    
                    await _productoAFavorRepository.AddProductoAFavor(productoAFavor);
                    Console.WriteLine("ProductoAFavor creado exitosamente");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al crear ProductoAFavor: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw;
                }
            }
            else
            {
                Console.WriteLine("No se creó ProductoAFavor porque ya existe uno para este producto");
            }
        }

        private async Task EliminarProductoAFavor(int ventaId, int productoId)
        {
            Console.WriteLine($"=== EliminarProductoAFavor - VentaId: {ventaId}, ProductoId: {productoId} ===");
            
            var productosAFavor = await _productoAFavorRepository.GetProductosAFavorByVentaId(ventaId);
            var productoAFavor = productosAFavor.FirstOrDefault(p => p.ProductoId == productoId);

            if (productoAFavor != null)
            {
                Console.WriteLine($"Eliminando ProductoAFavor con Id: {productoAFavor.Id}");
                await _productoAFavorRepository.DeleteProductoAFavor(productoAFavor.Id);
                Console.WriteLine("ProductoAFavor eliminado exitosamente");
            }
            else
            {
                Console.WriteLine("No se encontró ProductoAFavor para eliminar");
            }
        }

        private EstadoEntrega CalcularEstadoEntrega(List<VentaProducto> ventaProductos)
        {
            if (ventaProductos == null || !ventaProductos.Any())
            {
                return EstadoEntrega.NO_ENTREGADA;
            }

            var totalItems = ventaProductos.Count;
            var itemsEntregados = ventaProductos.Count(vp => vp.Entregado);

            if (itemsEntregados == 0)
            {
                return EstadoEntrega.NO_ENTREGADA;
            }
            else if (itemsEntregados == totalItems)
            {
                return EstadoEntrega.ENTREGADA;
            }
            else
            {
                return EstadoEntrega.PARCIAL;
            }
        }

        public async Task<bool> AsignarVentaAUsuario(int ventaId, int usuarioId)
        {
            return await _ventaRepository.AsignarVentaAUsuario(ventaId, usuarioId);
        }

        public async Task<AsignarVentaAutomaticaResponseDTO> AsignarVentaAutomaticamente(int ventaId, int usuarioIdExcluir)
        {
            // Buscar un repartidor disponible (empleado activo, excluyendo al usuario actual)
            var repartidores = await _userService.GetAllUsersAsync();
            var repartidorDisponible = repartidores
                .FirstOrDefault(u => u.Role == "Employee" 
                    && u.Id != usuarioIdExcluir
                    && u.IsActive);

            if (repartidorDisponible == null)
            {
                return new AsignarVentaAutomaticaResponseDTO
                {
                    Asignada = false,
                    Message = "No hay repartidores disponibles para asignar la venta."
                };
            }

            var asignado = await _ventaRepository.AsignarVentaAUsuario(ventaId, repartidorDisponible.Id);

            return new AsignarVentaAutomaticaResponseDTO
            {
                Asignada = asignado,
                UsuarioAsignadoId = repartidorDisponible.Id,
                UsuarioAsignadoNombre = repartidorDisponible.UserName,
                Message = asignado 
                    ? $"Venta asignada exitosamente a {repartidorDisponible.UserName}"
                    : "Error al asignar la venta."
            };
        }

        /// <summary>
        /// Aplica la lógica de expiración de descuentos (similar a ProductoService)
        /// </summary>
        private async Task AplicarLogicaExpiracionDescuento(Producto producto)
        {
            // Si el descuento es indefinido, no hacer nada
            if (producto.descuentoIndefinido)
            {
                return;
            }

            // Si el descuento no es indefinido y tiene fecha de expiración
            if (producto.fechaExpiracionDescuento.HasValue)
            {
                // Si la fecha ya venció, poner descuento en 0 y guardar
                if (producto.fechaExpiracionDescuento.Value < DateTime.UtcNow)
                {
                    // Guardar el cambio en la base de datos
                    await _productoRepository.UpdateProductoDiscount(producto.Id, 0, null, false);
                    // Actualizar el objeto en memoria
                    producto.Descuento = 0;
                    producto.fechaExpiracionDescuento = null;
                }
            }
        }

        /// <summary>
        /// Verifica si un producto tiene un descuento activo (no vencido)
        /// </summary>
        private bool TieneDescuentoActivo(Producto producto)
        {
            // Si no hay descuento, retornar false
            if (producto.Descuento <= 0)
            {
                return false;
            }

            // Si el descuento es indefinido, está activo
            if (producto.descuentoIndefinido)
            {
                return true;
            }

            // Si tiene fecha de expiración, verificar si no venció
            if (producto.fechaExpiracionDescuento.HasValue)
            {
                var ahora = DateTime.UtcNow;
                var fechaExpiracion = producto.fechaExpiracionDescuento.Value;
                return fechaExpiracion > ahora;
            }

            // Si tiene descuento pero no es indefinido y no tiene fecha, considerar activo
            // (aunque esto no debería pasar según la lógica del backend)
            return true;
        }
    }
}

