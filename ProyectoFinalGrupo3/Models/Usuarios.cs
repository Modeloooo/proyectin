using System.ComponentModel.DataAnnotations;

namespace ProyectoFinalGrupo3.Models
{
    public class Usuarios
    {
        [Key]
        [Required(ErrorMessage = "La identificación es obligatoria")]
        public string Identificacion { get; set; }
        [Required(ErrorMessage = "El Nombre es obligatorio")]
        public string NombreCompleto { get; set; }
        [Required(ErrorMessage = "El genero es obligatorio")]
        public string? Genero { get; set; }
        [Required(ErrorMessage = "El Correo es obligatorio")]
        public string Correo { get; set; }
        public string? TipoTarjeta { get; set; }
        public string? NumeroTarjeta { get; set; }
        public decimal? DineroDisponible { get; set; }
        [Required(ErrorMessage = "La Contraseña es obligatoria")]
        public string Contrasena { get; set; }
        public string Perfil { get; set; }
        public string? CodigoRecuperacion { get; set; }
        public DateTime? CodigoExpira { get; set; }
        public DateTime? UltimoLogin { get; set; }

        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}

