using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinalGrupo3.Data;
using ProyectoFinalGrupo3.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ProyectoFinalGrupo3.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly RestauranteDbContext _context;
        private readonly PasswordHasher<object> _hasher = new PasswordHasher<object>();

        public UsersController(RestauranteDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Usuarios.ToList());
        }

        public IActionResult HomeUser()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Identificacion == id);

            var pedidos = _context.Pedidos
                .Where(p => p.IdUsuario == id && p.Fecha >= DateTime.Now.AddDays(-30))
                .OrderByDescending(p => p.Fecha)
                .ToList();

            return View(new HomeUserViewModel
            {
                Usuario = usuario,
                Pedidos = pedidos
            });
        }

        public IActionResult DetailsUser()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Identificacion == id);
            return View(usuario);
        }

        [HttpPost]
        public IActionResult Crear(Usuarios usuario)
        {
            try
            {
                if (!ModelState.IsValid)
                    return RedirectToAction("Index");

                if (!EsCorreoValido(usuario.Correo))
                {
                    TempData["Error"] = "Correo inválido";
                    return RedirectToAction("Index");
                }

                if (_context.Usuarios.Any(u => u.Identificacion == usuario.Identificacion))
                {
                    TempData["Error"] = "Identificación ya existe";
                    return RedirectToAction("Index");
                }

                if (_context.Usuarios.Any(u => u.Correo == usuario.Correo))
                {
                    TempData["Error"] = "Correo ya registrado";
                    return RedirectToAction("Index");
                }

                usuario.Contrasena = _hasher.HashPassword(null, usuario.Contrasena);
                usuario.DineroDisponible = 0;
                usuario.Perfil ??= "Usuario";

                // Guardar número REAL
                usuario.NumeroTarjeta = LimpiarTarjeta(usuario.NumeroTarjeta);

                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                TempData["Success"] = "Usuario creado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Editar(Usuarios usuario)
        {
            try
            {
                var usuarioDb = _context.Usuarios.Find(usuario.Identificacion);

                if (usuarioDb == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction("Index");
                }

                if (!EsCorreoValido(usuario.Correo))
                {
                    TempData["Error"] = "Correo inválido";
                    return RedirectToAction("Index");
                }

                bool correoDuplicado = _context.Usuarios.Any(u =>
                    u.Correo == usuario.Correo &&
                    u.Identificacion != usuario.Identificacion);

                if (correoDuplicado)
                {
                    TempData["Error"] = "Correo ya en uso";
                    return RedirectToAction("Index");
                }

                // Validar admin
                if (usuarioDb.Perfil == "Administrador" && usuario.Perfil != "Administrador")
                {
                    int totalAdmins = _context.Usuarios.Count(u => u.Perfil == "Administrador");
                    if (totalAdmins <= 1)
                    {
                        TempData["Error"] = "Debe existir al menos un administrador";
                        return RedirectToAction("Index");
                    }
                }

                // Actualizar datos
                usuarioDb.NombreCompleto = usuario.NombreCompleto;
                usuarioDb.Genero = usuario.Genero;
                usuarioDb.Correo = usuario.Correo;
                usuarioDb.TipoTarjeta = usuario.TipoTarjeta;
                usuarioDb.NumeroTarjeta = LimpiarTarjeta(usuario.NumeroTarjeta);
                usuarioDb.Perfil = usuario.Perfil;

                if (!string.IsNullOrWhiteSpace(usuario.Contrasena))
                {
                    usuarioDb.Contrasena = _hasher.HashPassword(null, usuario.Contrasena);
                }

                _context.SaveChanges();

                TempData["Success"] = "Usuario actualizado";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(string id)
        {
            try
            {
                var usuario = _context.Usuarios.Find(id);

                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction("Index");
                }

                // ✅ Validación 1: No se puede eliminar el último administrador (VA PRIMERO)
                if (usuario.Perfil == "Administrador")
                {
                    int totalAdmins = _context.Usuarios.Count(u => u.Perfil == "Administrador");
                    if (totalAdmins <= 1)
                    {
                        TempData["Error"] = "No se puede eliminar el último administrador del sistema";
                        return RedirectToAction("Index");
                    }
                }

                // Validación 2: No se puede eliminar un usuario que haya realizado pedidos
                bool tienePedidos = _context.Pedidos.Any(p => p.IdUsuario == id);

                // Validación 3: No se puede eliminar un usuario que se haya logueado al menos una vez
                bool haLogueado = usuario.UltimoLogin != null;

                if (tienePedidos || haLogueado)
                {
                    TempData["Error"] = "No se puede eliminar el usuario porque ya ha realizado pedidos o ha iniciado sesión";
                    return RedirectToAction("Index");
                }

                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();

                TempData["Success"] = "Usuario eliminado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al eliminar el usuario: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private bool EsCorreoValido(string correo)
        {
            return Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private string? LimpiarTarjeta(string? numeroTarjeta)
        {
            if (string.IsNullOrWhiteSpace(numeroTarjeta))
                return null;

            return new string(numeroTarjeta.Where(char.IsDigit).ToArray());
        }

        // Para la vista
        public static string EnmascararTarjeta(string? numero)
        {
            if (string.IsNullOrWhiteSpace(numero) || numero.Length < 4)
                return "****";

            return $"****-****-****-{numero[^4..]}";
        }
    }
}