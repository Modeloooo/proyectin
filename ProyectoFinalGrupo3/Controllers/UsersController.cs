using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalGrupo3.Data;
using ProyectoFinalGrupo3.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ProyectoFinalGrupo3.Controllers
{
    public class UsersController : Controller
    {
        private readonly RestauranteDbContext _context;

        public UsersController(RestauranteDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var lista = _context.Usuarios.ToList();
            return View(lista);
        }

        public IActionResult HomeUser()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Identificacion == id);

            var fechaLimite = DateTime.Now.AddDays(-30);

            var pedidos = _context.Pedidos
                .Where(p => p.IdUsuario == id && p.Fecha >= fechaLimite)
                .OrderByDescending(p => p.Fecha)
                .ToList();

            var model = new HomeUserViewModel
            {
                Usuario = usuario,
                Pedidos = pedidos
            };

            return View(model);
        }


        public IActionResult DetailsUser()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Identificacion == id);
            return View(usuario);
        }

        public IActionResult EditUser(Usuarios usuario)
        {
            var hasher = new PasswordHasher<object>();
            try
            {
                var usuarioDb = _context.Usuarios
                    .FirstOrDefault(u => u.Identificacion == usuario.Identificacion);

                if (usuarioDb == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction("HomeUser");
                }

                if (string.IsNullOrWhiteSpace(usuario.NombreCompleto))
                {
                    TempData["Error"] = "El nombre completo es obligatorio";
                    return View(usuarioDb); // mejor volver a la misma vista
                }

                if (string.IsNullOrWhiteSpace(usuario.Correo) || !EsCorreoValido(usuario.Correo))
                {
                    TempData["Error"] = "Debe ingresar un correo válido";
                    return View(usuarioDb);
                }

                bool correoDuplicado = _context.Usuarios.Any(u =>
                    u.Correo == usuario.Correo &&
                    u.Identificacion != usuario.Identificacion);

                if (correoDuplicado)
                {
                    TempData["Error"] = "Ya existe otro usuario con ese correo";
                    return View(usuarioDb);
                }

                if (usuarioDb.Perfil == "Administrador" &&
                    usuario.Perfil != "Administrador")
                {
                    int adminsActivos = _context.Usuarios.Count(u => u.Perfil == "Administrador");
                    if (adminsActivos <= 1)
                    {
                        TempData["Error"] = "Debe quedar al menos un administrador en el sistema";
                        return View(usuarioDb);
                    }
                }

                // Actualizar campos
                usuarioDb.NombreCompleto = usuario.NombreCompleto;
                usuarioDb.Genero = usuario.Genero;
                usuarioDb.Correo = usuario.Correo;
                usuarioDb.TipoTarjeta = usuario.TipoTarjeta;

                // ⚠️ Guardar número real, mostrar enmascarado en la vista
                usuarioDb.NumeroTarjeta = usuario.NumeroTarjeta;

                if (!string.IsNullOrWhiteSpace(usuario.Contrasena))
                {
                    usuarioDb.Contrasena = hasher.HashPassword(null, usuario.Contrasena);// usar hash
                }

                _context.SaveChanges();

                TempData["Success"] = "Usuario actualizado correctamente";
                return RedirectToAction("DetailsUser");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al actualizar el usuario: " + ex.Message;
                return RedirectToAction("HomeUser");
            }
        }


        [HttpPost]
        public IActionResult Crear(Usuarios usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario.Identificacion))
                {
                    TempData["Error"] = "La identificación es obligatoria";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(usuario.NombreCompleto))
                {
                    TempData["Error"] = "El nombre completo es obligatorio";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(usuario.Correo) || !EsCorreoValido(usuario.Correo))
                {
                    TempData["Error"] = "Debe ingresar un correo válido";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(usuario.Contrasena))
                {
                    TempData["Error"] = "La contraseña es obligatoria";
                    return RedirectToAction("Index");
                }

                if (_context.Usuarios.Any(u => u.Identificacion == usuario.Identificacion))
                {
                    TempData["Error"] = "Ya existe un usuario con esa identificación";
                    return RedirectToAction("Index");
                }

                if (_context.Usuarios.Any(u => u.Correo == usuario.Correo))
                {
                    TempData["Error"] = "Ya existe un usuario con ese correo";
                    return RedirectToAction("Index");
                }

                usuario.DineroDisponible = 0;

                if (string.IsNullOrWhiteSpace(usuario.Perfil))
                    usuario.Perfil = "Usuario";

                usuario.NumeroTarjeta = EnmascararTarjeta(usuario.NumeroTarjeta);

                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                TempData["Success"] = "Usuario creado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al guardar el usuario: " + ex.Message;
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

                if (string.IsNullOrWhiteSpace(usuario.NombreCompleto))
                {
                    TempData["Error"] = "El nombre completo es obligatorio";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(usuario.Correo) || !EsCorreoValido(usuario.Correo))
                {
                    TempData["Error"] = "Debe ingresar un correo válido";
                    return RedirectToAction("Index");
                }

                bool correoDuplicado = _context.Usuarios.Any(u =>
                    u.Correo == usuario.Correo &&
                    u.Identificacion != usuario.Identificacion);

                if (correoDuplicado)
                {
                    TempData["Error"] = "Ya existe otro usuario con ese correo";
                    return RedirectToAction("Index");
                }

                if (usuarioDb.Perfil == "Administrador" &&
                    usuario.Perfil != "Administrador")
                {
                    int adminsActivos = _context.Usuarios.Count(u => u.Perfil == "Administrador");
                    if (adminsActivos <= 1)
                    {
                        TempData["Error"] = "Debe quedar al menos un administrador en el sistema";
                        return RedirectToAction("Index");
                    }
                }

                usuarioDb.NombreCompleto = usuario.NombreCompleto;
                usuarioDb.Genero = usuario.Genero;
                usuarioDb.Correo = usuario.Correo;
                usuarioDb.TipoTarjeta = usuario.TipoTarjeta;
                usuarioDb.NumeroTarjeta = EnmascararTarjeta(usuario.NumeroTarjeta);
                usuarioDb.Perfil = usuario.Perfil;

                if (!string.IsNullOrWhiteSpace(usuario.Contrasena))
                    usuarioDb.Contrasena = usuario.Contrasena;

                _context.SaveChanges();

                TempData["Success"] = "Usuario actualizado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al actualizar el usuario: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

       
        private bool EsCorreoValido(string correo)
        {
            return Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private string? EnmascararTarjeta(string? numeroTarjeta)
        {
            if (string.IsNullOrWhiteSpace(numeroTarjeta))
                return null;

            string soloDigitos = new string(numeroTarjeta.Where(char.IsDigit).ToArray());

            if (soloDigitos.Length < 4)
                return numeroTarjeta;

            string ultimos4 = soloDigitos.Substring(soloDigitos.Length - 4);
            return $"****-****-****-{ultimos4}";
        }

    }
}
