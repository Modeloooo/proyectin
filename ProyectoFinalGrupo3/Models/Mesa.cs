using System.ComponentModel.DataAnnotations;

namespace ProyectoFinalGrupo3.Models
{
    public class Mesa
    {
        [Key]
        public int NumeroMesa { get; set; } // autogenerado

        [Required]
        public int Capacidad { get; set; }

        [MaxLength(20)]
        public string Estado { get; set; } = "Libre";
        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
