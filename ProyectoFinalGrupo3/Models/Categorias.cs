using System.ComponentModel.DataAnnotations;

namespace ProyectoFinalGrupo3.Models
{
    public partial class Categorias
    {
        [Key]
        public int CodigoCategoria { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(100)]
        public string Descripcion { get; set; }
        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
