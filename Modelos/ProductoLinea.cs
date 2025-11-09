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
        public string NombreProducto { get; set; } = "";
        public decimal PrecioUnitario { get; set; }
        public int CantidadProducto { get; set; } = 1;

        [NotMapped]
        public decimal ImporteLinea => PrecioUnitario * CantidadProducto;
    }
}