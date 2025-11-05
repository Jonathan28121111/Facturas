using System;
using System.Collections.Generic;

namespace SistemaFacturas.Modelos
{
    public class DocumentoVenta
    {
        public int NumeroDocumento { get; set; }
        public DateTime FechaEmision { get; set; }
        public string NombreReceptor { get; set; }
        public List<ProductoLinea> LineasDetalle { get; set; } = new List<ProductoLinea>();
    }
}