using System;
using System.Diagnostics.Contracts;
using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AplicationDbContext _context;

        public ClienteRepository(AplicationDbContext context)
        {
            this._context = context;
        }

        /// <summary>
        /// Obtiene el TenantId actual del contexto para filtrar las consultas
        /// Siempre incluye clientes con TenantId = 0 (Consumidor Final compartido)
        /// </summary>
        private IQueryable<Cliente> FilterByTenant(IQueryable<Cliente> query)
        {
            var tenantId = _context.GetCurrentTenantId();
            if (tenantId.HasValue)
            {
                // Incluir clientes del tenant actual Y clientes con TenantId = 0 (Consumidor Final compartido)
                return query.Where(c => c.TenantId == tenantId.Value || c.TenantId == 0);
            }
            return query; // Si no hay TenantId (SuperAdmin), no filtrar
        }

        public async Task<Cliente> AddCliente(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente; // El cliente ahora tiene el ID asignado por la base de datos
        }

        public async Task<bool> DeleteCliente(int id)
        {
            var cliente = await FilterByTenant(_context.Clientes).Where(c => c.Id == id).FirstOrDefaultAsync();
            if (cliente == null)
                return false;

            // Indicar qué debe eliminar primero si tiene datos asociados
            var pendientes = new List<string>();
            if (await _context.Ventas.AnyAsync(v => v.ClienteId == id))
                pendientes.Add("las ventas asociadas");
            if (await _context.Abonos.AnyAsync(a => a.ClienteId == id))
                pendientes.Add("los abonos registrados");
            if (await _context.PreciosEspeciales.AnyAsync(p => p.ClienteId == id))
                pendientes.Add("los precios especiales configurados");

            if (pendientes.Count > 0)
            {
                var lista = string.Join(", ", pendientes);
                throw new InvalidOperationException(
                    "No se pudo eliminar el cliente. Elimina primero " + lista + ".");
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateCliente(Cliente cliente)
        {
            // Verificar si la entidad ya está siendo rastreada
            var trackedEntity = _context.ChangeTracker.Entries<Cliente>()
                .FirstOrDefault(e => e.Entity.Id == cliente.Id);

            if (trackedEntity != null)
            {
                // Si ya está siendo rastreada, actualizar los valores de la entidad rastreada
                trackedEntity.CurrentValues.SetValues(cliente);
            }
            else
            {
                // Si no está siendo rastreada, usar Update
                _context.Clientes.Update(cliente);
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task<Cliente> GetClienteById(int id)
        {
            // Buscar primero en el tenant actual, luego en TenantId = 0 (Consumidor Final compartido)
            var tenantId = _context.GetCurrentTenantId();
            Cliente? cliente = null;
            
            if (tenantId.HasValue)
            {
                cliente = await _context.Clientes
                    .Where(c => c.Id == id && (c.TenantId == tenantId.Value || c.TenantId == 0))
                    .FirstOrDefaultAsync();
            }
            else
            {
                cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
            }
            
            if (cliente == null)
            {
                throw new Exception($"El cliente con id {id} no fue encontrado o no existe");
            }

            return cliente;
        }

        /// <summary>
        /// Busca el cliente "Consumidor Final" con TenantId = 0 (compartido entre todos los tenants)
        /// </summary>
        public async Task<Cliente?> BuscarConsumidorFinal()
        {
            return await _context.Clientes
                .Where(c => c.TenantId == 0 && c.Nombre.Trim().Equals("Consumidor Final", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefaultAsync();
        }

        public async Task<List<Cliente>> GetAllClientes()
        {
            return await FilterByTenant(_context.Clientes).ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> BuscarClientesAsync(string query)
        {
            return await FilterByTenant(_context.Clientes)
                .Where(c => c.Nombre.Contains(query))
                .ToListAsync();
        }

        public async Task<List<Cliente>> GetClientesConDeuda()
        {
            var tenantId = _context.GetCurrentTenantId();
            var baseQuery = tenantId.HasValue 
                ? _context.Clientes.Where(c => c.TenantId == tenantId.Value || c.TenantId == 0)
                : _context.Clientes.AsQueryable();
            
            // Obtener IDs de clientes que tienen ventas con cuenta corriente (filtrando por tenant si aplica)
            var ventasCCQuery = _context.Ventas
                .Where(v => v.MetodoPago == MetodoPagoEnum.CuentaCorriente);
            
            if (tenantId.HasValue)
            {
                ventasCCQuery = ventasCCQuery.Where(v => v.TenantId == tenantId.Value);
            }
            
            var clienteIdsConVentasCC = await ventasCCQuery
                .Select(v => v.ClienteId)
                .Distinct()
                .ToListAsync();
            
            // Obtener IDs de clientes que tienen abonos (filtrando por tenant si aplica)
            var abonosQuery = _context.Abonos.AsQueryable();
            if (tenantId.HasValue)
            {
                abonosQuery = abonosQuery.Where(a => a.TenantId == tenantId.Value);
            }
            
            var clienteIdsConAbonos = await abonosQuery
                .Select(a => a.ClienteId)
                .Distinct()
                .ToListAsync();
            
            // Obtener todos los clientes del tenant
            var todosLosClientes = await baseQuery.ToListAsync();
            
            // Filtrar en memoria los clientes que tengan:
            // 1. Deuda actual > 0 (usando DeudaDecimal que es una propiedad calculada), O
            // 2. Alguna venta con cuenta corriente, O
            // 3. Algún abono registrado
            var todosLosIdsConCC = clienteIdsConVentasCC
                .Union(clienteIdsConAbonos)
                .ToHashSet();
            
            var clientesConDeuda = todosLosClientes
                .Where(c => 
                    c.DeudaDecimal > 0 || 
                    todosLosIdsConCC.Contains(c.Id)
                )
                .ToList();
            
            return clientesConDeuda;
        }

        public async Task<(List<Cliente> Clientes, int TotalCount)> GetClientesPaginados(int page, int pageSize)
        {
            var filteredQuery = FilterByTenant(_context.Clientes);
            var totalCount = await filteredQuery.CountAsync();
            
            var clientes = await filteredQuery
                .OrderByDescending(c => c.Id) // Ordenar por ID descendente para obtener los más recientes primero
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (clientes, totalCount);
        }

        public async Task<(List<Cliente> Clientes, int TotalCount)> GetClientesConDeudaPaginados(int page, int pageSize)
        {
            var tenantId = _context.GetCurrentTenantId();
            var baseQuery = tenantId.HasValue 
                ? _context.Clientes.Where(c => c.TenantId == tenantId.Value || c.TenantId == 0)
                : _context.Clientes.AsQueryable();
            
            // Obtener IDs de clientes que tienen ventas con cuenta corriente (filtrando por tenant si aplica)
            var ventasCCQuery = _context.Ventas
                .Where(v => v.MetodoPago == MetodoPagoEnum.CuentaCorriente);
            
            if (tenantId.HasValue)
            {
                ventasCCQuery = ventasCCQuery.Where(v => v.TenantId == tenantId.Value);
            }
            
            var clienteIdsConVentasCCList = await ventasCCQuery
                .Select(v => v.ClienteId)
                .Distinct()
                .ToListAsync();
            
            // Obtener IDs de clientes que tienen abonos (filtrando por tenant si aplica)
            var abonosQuery = _context.Abonos.AsQueryable();
            if (tenantId.HasValue)
            {
                abonosQuery = abonosQuery.Where(a => a.TenantId == tenantId.Value);
            }
            
            var clienteIdsConAbonosList = await abonosQuery
                .Select(a => a.ClienteId)
                .Distinct()
                .ToListAsync();
            
            // Obtener todos los clientes del tenant
            var todosLosClientes = await baseQuery.ToListAsync();
            
            // Filtrar en memoria los clientes que tengan:
            // 1. Deuda actual > 0 (usando DeudaDecimal que es una propiedad calculada), O
            // 2. Alguna venta con cuenta corriente, O
            // 3. Algún abono registrado
            var todosLosIdsConCC = clienteIdsConVentasCCList
                .Union(clienteIdsConAbonosList)
                .ToHashSet();
            
            var clientesConCC = todosLosClientes
                .Where(c => 
                    c.DeudaDecimal > 0 || 
                    todosLosIdsConCC.Contains(c.Id)
                )
                .OrderBy(c => c.Nombre)
                .ToList();
            
            var totalCount = clientesConCC.Count;
            
            var clientesPaginados = clientesConCC
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (clientesPaginados, totalCount);
        }
    }
}
