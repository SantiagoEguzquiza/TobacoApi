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
        /// </summary>
        private IQueryable<Cliente> FilterByTenant(IQueryable<Cliente> query)
        {
            var tenantId = _context.GetCurrentTenantId();
            if (tenantId.HasValue)
                return query.Where(c => c.TenantId == tenantId.Value);
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
            var tenantId = _context.GetCurrentTenantId();
            Cliente? cliente = tenantId.HasValue
                ? await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId.Value)
                : await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                throw new Exception($"El cliente con id {id} no fue encontrado o no existe");
            return cliente;
        }

        /// <summary>
        /// Busca el cliente "Consumidor Final" del tenant indicado (un Consumidor Final por tenant)
        /// </summary>
        public async Task<Cliente?> BuscarConsumidorFinal(int tenantId)
        {
            return await _context.Clientes
                .Where(c => c.TenantId == tenantId && c.Nombre.ToLower() == "consumidor final")
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

        /// <summary>
        /// Devuelve todos los clientes con cuenta corriente habilitada (HasCCTE == true).
        /// Incluye clientes con saldo 0 para poder ver historial completo.
        /// </summary>
        public async Task<List<Cliente>> GetClientesConDeuda()
        {
            var query = FilterByTenant(_context.Clientes)
                .Where(c => c.HasCCTE)
                .OrderBy(c => c.Nombre);
            return await query.ToListAsync();
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

        /// <summary>
        /// Devuelve clientes con cuenta corriente habilitada (HasCCTE == true), paginados.
        /// Incluye clientes con saldo 0 para poder ver historial completo.
        /// </summary>
        public async Task<(List<Cliente> Clientes, int TotalCount)> GetClientesConDeudaPaginados(int page, int pageSize)
        {
            var baseQuery = FilterByTenant(_context.Clientes).Where(c => c.HasCCTE);
            var totalCount = await baseQuery.CountAsync();
            var clientesPaginados = await baseQuery
                .OrderBy(c => c.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (clientesPaginados, totalCount);
        }
    }
}
