using System.ComponentModel.DataAnnotations;

namespace ProyectoFinalGrupo3.Models.ViewModel
{
    public class LoginViewModel
    {
        public string Correo { get; set; }
        public string Contrasena { get; set; }
    }
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "El código es obligatorio")]
        public string CodigoRecuperacion { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [DataType(DataType.Password)]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasena { get; set; }
    }
}
