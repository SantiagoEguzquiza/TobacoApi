using System;
using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.Models;

public class AplicationDbContext : DbContext
{
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

    public AplicationDbContext(DbContextOptions<AplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<Categoria>()
            .Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Categoria>()
            .HasIndex(c => c.Nombre)
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
            .WithMany()
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

        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

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

        // Índice único para evitar duplicados de cliente-producto
        modelBuilder.Entity<PrecioEspecial>()
            .HasIndex(pe => new { pe.ClienteId, pe.ProductoId })
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

        // Índice único compuesto para evitar cantidades duplicadas por producto
        modelBuilder.Entity<ProductQuantityPrice>()
            .HasIndex(pqp => new { pqp.ProductId, pqp.Quantity })
            .IsUnique();
        
        // Venta entity configuration
        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Cliente)
            .WithMany(c => c.Ventas)
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Usuario)
            .WithMany()
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        // Abonos entity configuration
        modelBuilder.Entity<Abonos>()
            .HasOne(a => a.Cliente)
            .WithMany(c => c.Abonos)
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Abonos>()
            .Property(a => a.Fecha)
            .HasDefaultValueSql("GETUTCDATE()");

    }

}
