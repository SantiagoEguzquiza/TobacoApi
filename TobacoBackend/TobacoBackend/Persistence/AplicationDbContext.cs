using System;
using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class AplicationDbContext : DbContext
{
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<VentaProducto> VentasProductos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<VentaPago> VentaPagos { get; set; }
    public DbSet<PrecioEspecial> PreciosEspeciales { get; set; }
    public DbSet<ProductQuantityPrice> ProductQuantityPrices { get; set; }
    public DbSet<Abonos> Abonos { get; set; }
    public DbSet<ProductoAFavor> ProductosAFavor { get; set; }
    public DbSet<Asistencia> Asistencias { get; set; }
    public DbSet<RecorridoProgramado> RecorridosProgramados { get; set; }
    public DbSet<PermisosEmpleado> PermisosEmpleados { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    private readonly IHttpContextAccessor? _httpContextAccessor;
    private const string TenantIdClaim = "tenant_id";

    public AplicationDbContext(DbContextOptions<AplicationDbContext> options) : base(options)
    {
    }

    public AplicationDbContext(DbContextOptions<AplicationDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<Categoria>()
            .Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        // Tenant entity configuration
        modelBuilder.Entity<Tenant>()
            .Property(t => t.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Tenant>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Configurar relaciones con Tenant
        modelBuilder.Entity<User>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Categoria>()
            .HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Tenant)
            .WithMany()
            .HasForeignKey(v => v.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Abonos>()
            .HasOne(a => a.Tenant)
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PrecioEspecial>()
            .HasOne(pe => pe.Tenant)
            .WithMany()
            .HasForeignKey(pe => pe.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductoAFavor>()
            .HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Asistencia>()
            .HasOne(a => a.Tenant)
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecorridoProgramado>()
            .HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PermisosEmpleado>()
            .HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductQuantityPrice>()
            .HasOne(pqp => pqp.Tenant)
            .WithMany()
            .HasForeignKey(pqp => pqp.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Modificar índice único de Categoria para incluir TenantId
        modelBuilder.Entity<Categoria>()
            .HasIndex(c => new { c.TenantId, c.Nombre })
            .IsUnique();

        modelBuilder.Entity<VentaProducto>()
            .HasKey(vp => new { vp.VentaId, vp.ProductoId });

        modelBuilder.Entity<VentaProducto>()
            .HasOne(vp => vp.Venta)
            .WithMany(v => v.VentaProductos)
            .HasForeignKey(vp => vp.VentaId);

        modelBuilder.Entity<VentaProducto>()
            .HasOne(vp => vp.Producto)
            .WithMany(p => p.VentaProductos)
            .HasForeignKey(vp => vp.ProductoId);

        modelBuilder.Entity<Producto>()
            .Property(p => p.Precio)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Venta>()
            .Property(v => v.Total)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VentaProducto>()
            .Property(vp => vp.Cantidad)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Producto>()
            .Property(p => p.Stock)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // User entity configuration
        modelBuilder.Entity<User>()
            .Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<User>()
            .Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Employee");

        // Índice único de UserName incluyendo TenantId (permite mismo username en diferentes tenants)
        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.TenantId, u.UserName })
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<User>()
            .Property(u => u.Plan)
            .IsRequired()
            .HasDefaultValue(PlanType.FREE);

        modelBuilder.Entity<User>()
            .HasOne(u => u.CreatedBy)
            .WithMany()
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.NoAction);

        // VentaPago entity configuration
        modelBuilder.Entity<VentaPago>()
            .Property(v => v.Monto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VentaPago>()
            .HasOne(v => v.Venta)
            .WithMany(vt => vt.VentaPagos)
            .HasForeignKey(v => v.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        // VentaProducto entity configuration
        modelBuilder.Entity<VentaProducto>()
            .Property(vp => vp.PrecioFinalCalculado)
            .HasPrecision(18, 2);

        // PrecioEspecial entity configuration
        modelBuilder.Entity<PrecioEspecial>()
            .Property(pe => pe.Precio)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PrecioEspecial>()
            .HasOne(pe => pe.Cliente)
            .WithMany(c => c.PreciosEspeciales)
            .HasForeignKey(pe => pe.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PrecioEspecial>()
            .HasOne(pe => pe.Producto)
            .WithMany()
            .HasForeignKey(pe => pe.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice único para evitar duplicados de cliente-producto (incluyendo TenantId)
        modelBuilder.Entity<PrecioEspecial>()
            .HasIndex(pe => new { pe.TenantId, pe.ClienteId, pe.ProductoId })
            .IsUnique();

        // ProductQuantityPrice entity configuration
        modelBuilder.Entity<ProductQuantityPrice>()
            .Property(pqp => pqp.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ProductQuantityPrice>()
            .HasOne(pqp => pqp.Producto)
            .WithMany(p => p.QuantityPrices)
            .HasForeignKey(pqp => pqp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice único compuesto para evitar cantidades duplicadas por producto (incluyendo TenantId)
        modelBuilder.Entity<ProductQuantityPrice>()
            .HasIndex(pqp => new { pqp.TenantId, pqp.ProductId, pqp.Quantity })
            .IsUnique();
        
        // Venta entity configuration
        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Cliente)
            .WithMany(c => c.Ventas)
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venta>()
            .HasOne(v => v.UsuarioCreador)
            .WithMany()
            .HasForeignKey(v => v.UsuarioIdCreador)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Venta>()
            .HasOne(v => v.UsuarioAsignado)
            .WithMany()
            .HasForeignKey(v => v.UsuarioIdAsignado)
            .OnDelete(DeleteBehavior.NoAction);

        // Abonos entity configuration
        modelBuilder.Entity<Abonos>()
            .HasOne(a => a.Cliente)
            .WithMany(c => c.Abonos)
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Abonos>()
            .Property(a => a.Fecha)
            .HasDefaultValueSql("GETUTCDATE()");

        // ProductoAFavor entity configuration
        modelBuilder.Entity<ProductoAFavor>()
            .Property(p => p.Cantidad)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ProductoAFavor>()
            .HasOne(p => p.Cliente)
            .WithMany()
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductoAFavor>()
            .HasOne(p => p.Producto)
            .WithMany()
            .HasForeignKey(p => p.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductoAFavor>()
            .HasOne(p => p.Venta)
            .WithMany()
            .HasForeignKey(p => p.VentaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductoAFavor>()
            .HasOne(p => p.UsuarioRegistro)
            .WithMany()
            .HasForeignKey(p => p.UsuarioRegistroId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ProductoAFavor>()
            .HasOne(p => p.UsuarioEntrega)
            .WithMany()
            .HasForeignKey(p => p.UsuarioEntregaId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ProductoAFavor>()
            .Property(p => p.FechaRegistro)
            .HasDefaultValueSql("GETUTCDATE()");

        // VentaProducto - Usuario Chequeo relationship
        modelBuilder.Entity<VentaProducto>()
            .HasOne(vp => vp.UsuarioChequeo)
            .WithMany()
            .HasForeignKey(vp => vp.UsuarioChequeoId)
            .OnDelete(DeleteBehavior.NoAction);

        // Asistencia entity configuration
        modelBuilder.Entity<Asistencia>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Asistencia>()
            .Property(a => a.FechaHoraEntrada)
            .IsRequired();

        // RecorridoProgramado entity configuration
        modelBuilder.Entity<RecorridoProgramado>()
            .HasOne(r => r.Vendedor)
            .WithMany()
            .HasForeignKey(r => r.VendedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecorridoProgramado>()
            .HasOne(r => r.Cliente)
            .WithMany()
            .HasForeignKey(r => r.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecorridoProgramado>()
            .HasIndex(r => new { r.TenantId, r.VendedorId, r.DiaSemana, r.ClienteId })
            .IsUnique();

        modelBuilder.Entity<RecorridoProgramado>()
            .Property(r => r.FechaCreacion)
            .HasDefaultValueSql("GETUTCDATE()");

        // PermisosEmpleado entity configuration
        modelBuilder.Entity<PermisosEmpleado>()
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<PermisosEmpleado>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PermisosEmpleado>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        modelBuilder.Entity<PermisosEmpleado>()
            .Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // RefreshToken entity configuration
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.Tenant)
            .WithMany()
            .HasForeignKey(rt => rt.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RefreshToken>()
            .Property(rt => rt.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        // NOTA: Los filtros por TenantId se aplican manualmente en los repositorios
        // porque EF Core no puede traducir expresiones que acceden a HttpContext a SQL
        // Los repositorios deben filtrar por TenantId obtenido del token JWT

    }

    /// <summary>
    /// Obtiene el TenantId del token JWT actual
    /// </summary>
    public int? GetCurrentTenantId()
    {
        if (_httpContextAccessor?.HttpContext == null)
            return null;

        var tenantIdClaim = _httpContextAccessor.HttpContext.User?.FindFirst(TenantIdClaim)?.Value;
        if (!string.IsNullOrEmpty(tenantIdClaim) && int.TryParse(tenantIdClaim, out int tenantId))
        {
            return tenantId;
        }

        return null;
    }

}
