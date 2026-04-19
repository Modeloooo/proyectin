using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinalGrupo3.Models
{
    public class Producto
    {
        [Key]
        public int CodigoProducto { get; set; } // autogenerado por SQL (IDENTITY)

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; }

        [Required]
        public int CodigoCategoria { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Required]
        public decimal Precio { get; set; }

        public int RequiereEmpaque { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoEmpaque { get; set; } = 0;

        [Required]
        public int Cantidad { get; set; }

        public int Estado { get; set; } = 1;

        [MaxLength(300)]
        public string? UrlImagen { get; set; } // nuevo campo para imagen

        public virtual Categorias CodigoCategoriaNavigation { get; set; }

        public virtual ICollection<PedidoDetalle> PedidoDetalles { get; set; } = new List<PedidoDetalle>();

    }
}
