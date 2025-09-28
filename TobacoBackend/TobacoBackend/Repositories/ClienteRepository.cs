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

        public async Task<Cliente> AddCliente(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente; // El cliente ahora tiene el ID asignado por la base de datos
        }

        public async Task<bool> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;

        }

        public async Task UpdateCliente(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task<Cliente> GetClienteById(int id)
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id ==id);
            if (cliente == null)
            {
                throw new Exception($"El cliente con id {id} no fue encontrado o no existe");
            }

            return cliente;
        }

        public async Task<List<Cliente>> GetAllClientes()
        {
            return await _context.Clientes.ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> BuscarClientesAsync(string query)
        {
            return await _context.Clientes
                .Where(c => c.Nombre.Contains(query))
                .ToListAsync();
        }

        public async Task<List<Cliente>> GetClientesConDeuda()
        {
            var allClientes = await _context.Clientes.ToListAsync();
            var clientesConDeuda = allClientes.Where(c => c.DeudaDecimal > 0).ToList();
            return clientesConDeuda;
        }

        public async Task<(List<Cliente> Clientes, int TotalCount)> GetClientesPaginados(int page, int pageSize)
        {
            var totalCount = await _context.Clientes.CountAsync();
            
            var clientes = await _context.Clientes
                .OrderByDescending(c => c.Id) // Ordenar por ID descendente para obtener los más recientes primero
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (clientes, totalCount);
        }

        public async Task<(List<Cliente> Clientes, int TotalCount)> GetClientesConDeudaPaginados(int page, int pageSize)
        {
            // Obtener todos los clientes y filtrar en memoria usando DeudaDecimal
            var allClientes = await _context.Clientes.ToListAsync();
            var clientesConDeuda = allClientes.Where(c => c.DeudaDecimal > 0).ToList();
            
            var totalCount = clientesConDeuda.Count;
            
            var clientesPaginados = clientesConDeuda
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (clientesPaginados, totalCount);
        }
    }
}
