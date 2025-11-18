using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaFacturas.Data;

namespace SistemaFacturas.Services
{
    public record ProductoTopDto(string NombreProducto, int TotalCantidad, decimal TotalRevenue);
    public record MesVentasDto(int Anno, int Mes, decimal TotalRevenue, int DocumentCount);
    public record ClienteTopDto(string NombreReceptor, decimal TotalRevenue, int DocumentCount);
    public record ResumenDocumentoDto(string NumeroDocumento, DateTime FechaEmision, int LineCount, decimal Total);
    public record VentasDiaSemanaDto(string DiaSemana, decimal TotalRevenue, int DocumentCount);
    public record PromediosDto(double AvgLinesPerDocument, decimal AvgDocumentValue);
    public record ProductoDecliveDto(string NombreProducto, double Pendiente, List<MesVentasDto> Series);
    public record ClienteRecurrenteDto(string NombreReceptor, int ComprasDistinct, decimal TotalRevenue);

    public class ConsultasSistema
    {
        private readonly FacturasDbContext _db;
        public ConsultasSistema(FacturasDbContext db) => _db = db;

        public async Task<ProductoTopDto?> GetProductoMasVendidoAsync()
        {
            var datos = await _db.Lineas.ToListAsync();

            return datos
                .GroupBy(l => l.NombreProducto)
                .Select(g => new ProductoTopDto(g.Key!, g.Sum(x => x.CantidadProducto), g.Sum(x => x.CantidadProducto * x.PrecioUnitario)))
                .OrderByDescending(x => x.TotalCantidad)
                .FirstOrDefault();
        }

        public async Task<MesVentasDto?> GetMesConMasVentasAsync()
        {
            var datos = await _db.Lineas
                .Join(_db.Documentos, l => l.DocumentoId, d => d.NumeroDocumento, (l, d) => new { l, d })
                .ToListAsync();

            return datos
                .GroupBy(x => new { x.d.FechaEmision.Year, x.d.FechaEmision.Month })
                .Select(g => new MesVentasDto(g.Key.Year, g.Key.Month, g.Sum(x => x.l.CantidadProducto * x.l.PrecioUnitario), g.Select(x => x.d.NumeroDocumento).Distinct().Count()))
                .OrderByDescending(x => x.TotalRevenue)
                .FirstOrDefault();
        }

        public async Task<List<MesVentasDto>> GetTendenciaMensualAsync(int meses = 12)
        {
            var from = DateTime.UtcNow.AddMonths(-meses + 1);
            var datos = await _db.Lineas
                .Join(_db.Documentos, l => l.DocumentoId, d => d.NumeroDocumento, (l, d) => new { l, d })
                .Where(x => x.d.FechaEmision >= from)
                .ToListAsync();

            return datos
                .GroupBy(x => new { x.d.FechaEmision.Year, x.d.FechaEmision.Month })
                .Select(g => new MesVentasDto(g.Key.Year, g.Key.Month, g.Sum(x => x.l.CantidadProducto * x.l.PrecioUnitario), g.Select(x => x.d.NumeroDocumento).Distinct().Count()))
                .OrderBy(x => x.Anno).ThenBy(x => x.Mes)
                .ToList();
        }

        public async Task<List<ClienteTopDto>> GetClientesTopAsync(int top = 10)
        {
            var datos = await _db.Lineas
                .Join(_db.Documentos, l => l.DocumentoId, d => d.NumeroDocumento, (l, d) => new { l, d })
                .ToListAsync();

            return datos
                .GroupBy(x => x.d.NombreReceptor)
                .Select(g => new ClienteTopDto(g.Key!, g.Sum(x => x.l.CantidadProducto * x.l.PrecioUnitario), g.Select(x => x.d.NumeroDocumento).Distinct().Count()))
                .OrderByDescending(x => x.TotalRevenue)
                .Take(top)
                .ToList();
        }

        public async Task<List<ResumenDocumentoDto>> GetResumenDocumentosAsync(int top = 10)
        {
            var datos = await _db.Documentos
                .GroupJoin(_db.Lineas, d => d.NumeroDocumento, l => l.DocumentoId, (d, lines) => new { d.NumeroDocumento, d.FechaEmision, Lines = lines })
                .ToListAsync();

            var perDoc = datos
                .Select(x => new { x.NumeroDocumento, x.FechaEmision, LineCount = x.Lines.Count(), Total = x.Lines.Sum(l => l.CantidadProducto * l.PrecioUnitario) })
                .OrderByDescending(x => x.Total)
                .Take(top)
                .Select(x => new ResumenDocumentoDto(x.NumeroDocumento!.ToString(), x.FechaEmision, x.LineCount, x.Total))
                .ToList();

            return perDoc;
        }

        public async Task<List<ProductoTopDto>> GetTopProductoPorMesAsync(int meses = 6)
        {
            var from = DateTime.UtcNow.AddMonths(-meses + 1);
            var perProductMonth = await _db.Lineas
                .Join(_db.Documentos, l => l.DocumentoId, d => d.NumeroDocumento, (l, d) => new { l, d })
                .Where(x => x.d.FechaEmision >= from)
                .ToListAsync();

            var grouped = perProductMonth
                .GroupBy(x => new { x.d.FechaEmision.Year, x.d.FechaEmision.Month, x.l.NombreProducto })
                .Select(g => new { g.Key.Year, g.Key.Month, NombreProducto = g.Key.NombreProducto, TotalCantidad = g.Sum(x => x.l.CantidadProducto), TotalRevenue = g.Sum(x => x.l.CantidadProducto * x.l.PrecioUnitario) })
                .ToList();

            var topPerMonth = grouped
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => g.OrderByDescending(x => x.TotalCantidad).First())
                .Select(b => new ProductoTopDto(b.NombreProducto!, b.TotalCantidad, b.TotalRevenue))
                .ToList();

            return topPerMonth;
        }

        public async Task<List<VentasDiaSemanaDto>> GetVentasPorDiaSemanaAsync()
        {
            var raw = await _db.Lineas
                .Join(_db.Documentos, l => l.DocumentoId, d => d.NumeroDocumento, (l, d) => new { l, d })
                .ToListAsync();

            var grouped = raw
                .GroupBy(x => (int)x.d.FechaEmision.DayOfWeek)
                .Select(g => new { WeekDay = g.Key, TotalRevenue = g.Sum(x => x.l.CantidadProducto * x.l.PrecioUnitario), DocumentCount = g.Select(x => x.d.NumeroDocumento).Distinct().Count() })
                .ToList();

            return grouped.Select(x =>
            {
                string name = x.WeekDay switch
                {
                    0 => "Domingo",
                    1 => "Lunes",
                    2 => "Martes",
                    3 => "Miércoles",
                    4 => "Jueves",
                    5 => "Viernes",
                    6 => "Sábado",
                    _ => "Día"
                };
                return new VentasDiaSemanaDto(name, x.TotalRevenue, x.DocumentCount);
            }).ToList();
        }

        public async Task<PromediosDto> GetPromediosAsync()
        {
            var perDoc = await _db.Documentos
                .GroupJoin(_db.Lineas, d => d.NumeroDocumento, l => l.DocumentoId, (d, lines) => new { Lines = lines })
                .ToListAsync();

            if (!perDoc.Any()) return new PromediosDto(0, 0m);

            var stats = perDoc.Select(x => new { LineCount = x.Lines.Count(), Total = x.Lines.Sum(l => l.CantidadProducto * l.PrecioUnitario) }).ToList();

            double avgLines = stats.Average(x => (double)x.LineCount);
            decimal avgValue = stats.Average(x => x.Total);
            return new PromediosDto(avgLines, avgValue);
        }

        public async Task<List<ProductoDecliveDto>> GetProductosEnDecliveAsync(int meses = 6, int top = 5)
        {
            var from = DateTime.UtcNow.AddMonths(-meses + 1);
            var perProductMonth = await _db.Lineas
                .Join(_db.Documentos, l => l.DocumentoId, d => d.NumeroDocumento, (l, d) => new { l, d })
                .Where(x => x.d.FechaEmision >= from)
                .ToListAsync();

            var grouped = perProductMonth
                .GroupBy(x => new { x.l.NombreProducto, x.d.FechaEmision.Year, x.d.FechaEmision.Month })
                .Select(g => new { g.Key.NombreProducto, Year = g.Key.Year, Month = g.Key.Month, Total = g.Sum(x => x.l.CantidadProducto) })
                .ToList();

            var seriesByProduct = grouped
                .GroupBy(x => x.NombreProducto)
                .Select(g =>
                {
                    var series = g.OrderBy(x => x.Year).ThenBy(x => x.Month)
                                  .Select((v, i) => new { Index = i, Value = (double)v.Total, Year = v.Year, Month = v.Month })
                                  .ToList();
                    int n = series.Count;
                    if (n < 2) return null;
                    double avgX = series.Average(s => s.Index);
                    double avgY = series.Average(s => s.Value);
                    double num = series.Sum(s => (s.Index - avgX) * (s.Value - avgY));
                    double den = series.Sum(s => (s.Index - avgX) * (s.Index - avgX));
                    double slope = den == 0 ? 0 : num / den;
                    var serieDtos = series.Select(s => new MesVentasDto(series[s.Index].Year, series[s.Index].Month, 0m, 0)).ToList();
                    return new ProductoDecliveDto(g.Key!, slope, serieDtos);
                })
                .Where(x => x != null)
                .OrderBy(x => x!.Pendiente)
                .Take(top)
                .Select(x => x!)
                .ToList();

            return seriesByProduct;
        }

        public async Task<List<ClienteRecurrenteDto>> GetClientesRecurrentesAsync(int meses = 12, int minCompras = 3)
        {
            var from = DateTime.UtcNow.AddMonths(-meses + 1);

            var datos = await _db.Documentos
                .Where(d => d.FechaEmision >= from)
                .Join(_db.Lineas,
                    d => d.NumeroDocumento,
                    l => l.DocumentoId,
                    (d, l) => new { d.NombreReceptor, d.FechaEmision, l.CantidadProducto, l.PrecioUnitario })
                .ToListAsync();

            var resultado = datos
                .GroupBy(x => x.NombreReceptor)
                .Select(g => new ClienteRecurrenteDto(
                    g.Key!,
                    g.Select(x => x.FechaEmision.Date).Distinct().Count(),
                    g.Sum(x => x.CantidadProducto * x.PrecioUnitario)
                ))
                .Where(c => c.ComprasDistinct >= minCompras)
                .OrderByDescending(c => c.TotalRevenue)
                .ToList();

            return resultado;
        }
    }
}