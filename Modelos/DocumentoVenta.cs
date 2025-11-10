using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SistemaFacturas.Modelos
{
    public class DocumentoVenta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NumeroDocumento { get; set; }

        public DateTime FechaEmision { get; set; } = DateTime.Now;

        [Required]
        public string NombreReceptor { get; set; } = string.Empty;

        public List<ProductoLinea> LineasDetalle { get; set; } = new List<ProductoLinea>();

        [NotMapped]
        public decimal ImporteTotal => LineasDetalle?.Sum(l => l.ImporteLinea) ?? 0m;
    }
}