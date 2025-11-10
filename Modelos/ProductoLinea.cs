using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFacturas.Modelos
{
    public class ProductoLinea
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int DocumentoId { get; set; }

        [Required]
        public string NombreProducto { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal PrecioUnitario { get; set; }

        [Range(0, int.MaxValue)]
        public int CantidadProducto { get; set; } = 1;      

        [NotMapped]
        public decimal ImporteLinea => PrecioUnitario * CantidadProducto;
    }
}