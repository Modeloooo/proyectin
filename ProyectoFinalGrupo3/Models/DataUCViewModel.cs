namespace ProyectoFinalGrupo3.Models
{
    public class DataUCViewModel
    {
        public Usuarios Usuario { get; set; }
        public List<CarritoItem> Carrito { get; set; }
        public decimal Subtotal { get; set; }
        public decimal IVA { get; set; }
        public decimal Envio { get; set; }
        public decimal Total { get; set; }
        public string MetodoEntrega { get; set; }

    }
}
