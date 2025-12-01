using Microsoft.EntityFrameworkCore;
using SistemaFacturas.Data;
using SistemaFacturas.Modelos;

namespace SistemaFacturas.Services
{
    public class GestionArchivadas
    {
        private readonly FacturasDbContext _context;
        private const string CONTRASENA_ARCHIVADAS = "admin123";

        public GestionArchivadas(FacturasDbContext context)
        {
            _context = context;
        }

        public bool ValidarContrasena(string contrasena)
        {
            return contrasena == CONTRASENA_ARCHIVADAS;
        }

        public async Task<List<DocumentoVenta>> ObtenerArchivadas()
        {
            return await _context.Documentos
                .Include(d => d.LineasDetalle)
                .Where(d => d.Archivada)
                .OrderByDescending(d => d.NumeroDocumento)
                .ToListAsync();
        }

        public async Task<bool> ArchivarDocumento(int numeroDocumento)
        {
            var doc = await _context.Documentos.FindAsync(numeroDocumento);
            if (doc == null) return false;

            doc.Archivada = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DesarchivarDocumento(int numeroDocumento)
        {
            var doc = await _context.Documentos.FindAsync(numeroDocumento);
            if (doc == null) return false;

            doc.Archivada = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}