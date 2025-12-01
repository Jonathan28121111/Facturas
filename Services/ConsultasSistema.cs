using Microsoft.EntityFrameworkCore;
using SistemaFacturas.Data;
using SistemaFacturas.Modelos;

namespace SistemaFacturas.Services
{
    public class ConsultasSistema
    {
        private readonly FacturasDbContext _context;

        public ConsultasSistema(FacturasDbContext context)
        {
            _context = context;
        }

        public async Task<ProductoTopDto?> GetProductoMasVendidoAsync()
        {
            var resultado = await _context.Lineas
                .Where(l => _context.Documentos.Any(d => d.NumeroDocumento == l.DocumentoId && !d.Archivada))
                .GroupBy(l => l.NombreProducto)
                .Select(g => new ProductoTopDto
                {
                    NombreProducto = g.Key,
                    TotalCantidad = g.Sum(l => l.CantidadProducto),
                    TotalRevenue = g.Sum(l => l.ImporteLinea)
                })
                .OrderByDescending(p => p.TotalCantidad)
                .FirstOrDefaultAsync();

            return resultado;
        }

        public async Task<MesVentasDto?> GetMesConMasVentasAsync()
        {
            var resultado = await _context.Documentos
                .Where(d => !d.Archivada)
                .GroupBy(d => new { d.FechaEmision.Year, d.FechaEmision.Month })
                .Select(g => new MesVentasDto
                {
                    Anno = g.Key.Year,
                    Mes = g.Key.Month,
                    TotalRevenue = g.Sum(d => d.ImporteTotal),
                    DocumentCount = g.Count()
                })
                .OrderByDescending(m => m.TotalRevenue)
                .FirstOrDefaultAsync();

            return resultado;
        }

        public async Task<List<MesVentasDto>> GetTendenciaMensualAsync()
        {
            var resultado = await _context.Documentos
                .Where(d => !d.Archivada)
                .GroupBy(d => new { d.FechaEmision.Year, d.FechaEmision.Month })
                .Select(g => new MesVentasDto
                {
                    Anno = g.Key.Year,
                    Mes = g.Key.Month,
                    TotalRevenue = g.Sum(d => d.ImporteTotal),
                    DocumentCount = g.Count()
                })
                .OrderBy(m => m.Anno)
                .ThenBy(m => m.Mes)
                .ToListAsync();

            return resultado;
        }

        public async Task<List<ClienteTopDto>> GetClientesTopAsync()
        {
            var resultado = await _context.Documentos
                .Where(d => !d.Archivada)
                .GroupBy(d => d.NombreReceptor)
                .Select(g => new ClienteTopDto
                {
                    NombreReceptor = g.Key,
                    TotalRevenue = g.Sum(d => d.ImporteTotal),
                    DocumentCount = g.Count()
                })
                .OrderByDescending(c => c.TotalRevenue)
                .Take(10)
                .ToListAsync();

            return resultado;
        }

        public async Task<List<ResumenDocumentoDto>> GetResumenDocumentosAsync()
        {
            var resultado = await _context.Documentos
                .Include(d => d.LineasDetalle)
                .Where(d => !d.Archivada)
                .Select(d => new ResumenDocumentoDto
                {
                    NumeroDocumento = d.NumeroDocumento,
                    FechaEmision = d.FechaEmision,
                    LineCount = d.LineasDetalle.Count,
                    Total = d.ImporteTotal
                })
                .OrderByDescending(r => r.FechaEmision)
                .Take(20)
                .ToListAsync();

            return resultado;
        }

        public async Task<List<ProductoTopDto>> GetTopProductoPorMesAsync()
        {
            var resultado = await _context.Lineas
                .Where(l => _context.Documentos.Any(d => d.NumeroDocumento == l.DocumentoId && !d.Archivada))
                .GroupBy(l => l.NombreProducto)
                .Select(g => new ProductoTopDto
                {
                    NombreProducto = g.Key,
                    TotalCantidad = g.Sum(l => l.CantidadProducto),
                    TotalRevenue = g.Sum(l => l.ImporteLinea)
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(5)
                .ToListAsync();

            return resultado;
        }

        public async Task<List<VentasDiaSemanaDto>> GetVentasPorDiaSemanaAsync()
        {
            var documentos = await _context.Documentos
                .Where(d => !d.Archivada)
                .ToListAsync();

            var resultado = documentos
                .GroupBy(d => d.FechaEmision.DayOfWeek)
                .Select(g => new VentasDiaSemanaDto
                {
                    DiaSemana = ObtenerNombreDia(g.Key),
                    TotalRevenue = g.Sum(d => d.ImporteTotal),
                    DocumentCount = g.Count()
                })
                .OrderBy(v => OrdenDia(v.DiaSemana))
                .ToList();

            return resultado;
        }

        public async Task<PromediosDto> GetPromediosAsync()
        {
            var documentos = await _context.Documentos
                .Include(d => d.LineasDetalle)
                .Where(d => !d.Archivada)
                .ToListAsync();

            if (!documentos.Any())
                return new PromediosDto(0, 0m);

            var avgLines = documentos.Average(d => d.LineasDetalle.Count);
            var avgValue = documentos.Average(d => d.ImporteTotal);

            return new PromediosDto(avgLines, avgValue);
        }

        public async Task<List<ProductoDecliveDto>> GetProductosEnDecliveAsync()
        {
            var resultado = await _context.Lineas
                .Where(l => _context.Documentos.Any(d => d.NumeroDocumento == l.DocumentoId && !d.Archivada))
                .GroupBy(l => l.NombreProducto)
                .Select(g => new ProductoDecliveDto
                {
                    NombreProducto = g.Key,
                    TotalCantidad = g.Sum(l => l.CantidadProducto)
                })
                .OrderBy(p => p.TotalCantidad)
                .Take(5)
                .ToListAsync();

            return resultado;
        }

        public async Task<List<ClienteRecurrenteDto>> GetClientesRecurrentesAsync()
        {
            var resultado = await _context.Documentos
                .Where(d => !d.Archivada)
                .GroupBy(d => d.NombreReceptor)
                .Select(g => new ClienteRecurrenteDto
                {
                    NombreReceptor = g.Key,
                    DocumentCount = g.Count(),
                    TotalRevenue = g.Sum(d => d.ImporteTotal)
                })
                .OrderByDescending(c => c.DocumentCount)
                .Take(10)
                .ToListAsync();

            return resultado;
        }

        private string ObtenerNombreDia(DayOfWeek dia)
        {
            return dia switch
            {
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miercoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sabado",
                DayOfWeek.Sunday => "Domingo",
                _ => "Desconocido"
            };
        }

        private int OrdenDia(string nombreDia)
        {
            return nombreDia switch
            {
                "Lunes" => 1,
                "Martes" => 2,
                "Miercoles" => 3,
                "Jueves" => 4,
                "Viernes" => 5,
                "Sabado" => 6,
                "Domingo" => 7,
                _ => 8
            };
        }
    }

    public record ProductoTopDto
    {
        public string NombreProducto { get; set; } = string.Empty;
        public int TotalCantidad { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public record MesVentasDto
    {
        public int Anno { get; set; }
        public int Mes { get; set; }
        public decimal TotalRevenue { get; set; }
        public int DocumentCount { get; set; }
    }

    public record ClienteTopDto
    {
        public string NombreReceptor { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int DocumentCount { get; set; }
    }

    public record ResumenDocumentoDto
    {
        public int NumeroDocumento { get; set; }
        public DateTime FechaEmision { get; set; }
        public int LineCount { get; set; }
        public decimal Total { get; set; }
    }

    public record VentasDiaSemanaDto
    {
        public string DiaSemana { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int DocumentCount { get; set; }
    }

    public record PromediosDto(double AvgLinesPerDocument, decimal AvgDocumentValue);

    public record ProductoDecliveDto
    {
        public string NombreProducto { get; set; } = string.Empty;
        public int TotalCantidad { get; set; }
    }

    public record ClienteRecurrenteDto
    {
        public string NombreReceptor { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}