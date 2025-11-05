namespace SistemaFacturas.Modelos
{
    public class ProductoLinea
    {
        public string NombreProducto { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int CantidadProducto { get; set; } = 1;

        public decimal ImporteLinea => PrecioUnitario * CantidadProducto;
    }
}
