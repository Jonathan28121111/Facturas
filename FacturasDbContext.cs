using Microsoft.EntityFrameworkCore;
using SistemaFacturas.Modelos;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SistemaFacturas.Data
{
    public class FacturasDbContext : DbContext
    {
        public FacturasDbContext(DbContextOptions<FacturasDbContext> options) : base(options)
        {
        }

        public DbSet<DocumentoVenta> Documentos { get; set; }
        public DbSet<ProductoLinea> Lineas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentoVenta>()
                .HasKey(d => d.NumeroDocumento);

            modelBuilder.Entity<ProductoLinea>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<DocumentoVenta>()
                .HasMany(d => d.LineasDetalle)
                .WithOne()
                .HasForeignKey(l => l.DocumentoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}