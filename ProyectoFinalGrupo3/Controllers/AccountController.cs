using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinalGrupo3.Data;
using ProyectoFinalGrupo3.Models;
using ProyectoFinalGrupo3.Models.ViewModel;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace ProyectoFinalGrupo3.Controllers
{
    public class AccountController : Controller
    {
        private readonly RestauranteDbContext _context;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;

        public AccountController(RestauranteDbContext context, IConfiguration config, IWebHostEnvironment env, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _env = env;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == model.Correo);

            if (usuario == null)
            {
                ViewBag.LoginSuccess = false;
                return View(model);
            }

            var hasher = new PasswordHasher<object>();

            // Detectar si la contraseña guardada parece hash o texto plano
            bool esHash = usuario.Contrasena.Length > 50;
            // (los hashes generados por PasswordHasher suelen ser largos, >50 caracteres)

            if (esHash)
            {
                // Validar contra el hash
                var resultado = hasher.VerifyHashedPassword(null, usuario.Contrasena, model.Contrasena);
                if (resultado != PasswordVerificationResult.Success)
                {
                    ViewBag.LoginSuccess = false;
                    return View(model);
                }
            }
            else
            {
                // Comparar texto plano
                if (usuario.Contrasena != model.Contrasena)
                {
                    ViewBag.LoginSuccess = false;
                    return View(model);
                }

                // Migrar a hash
                usuario.Contrasena = hasher.HashPassword(null, model.Contrasena);
                _context.SaveChanges();
            }

            // Generar token y cookie
            var token = GenerateJwtToken(usuario);
            SetJwtCookie(token);

            ViewBag.LoginSuccess = true;
            return View(model);
        }
        private void SetJwtCookie(string token)
        {
            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = _env.IsProduction(),
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(30)
            });
        }
        private string GenerateJwtToken(Usuarios usuario)
        {
            var jwtConfig = _config.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]);

            var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Identificacion), // ID único
                    new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                    new Claim(ClaimTypes.Email, usuario.Correo),
                    new Claim(ClaimTypes.Role, usuario.Perfil),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };


            var token = new JwtSecurityToken(
             issuer: jwtConfig["Issuer"],
             audience: jwtConfig["Audience"],
             claims: claims,
             expires: DateTime.UtcNow.AddMinutes(30),
             signingCredentials: new SigningCredentials(
                 new SymmetricSecurityKey(key),
                 SecurityAlgorithms.HmacSha256
             )
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction(nameof(Login));
        }





        [HttpGet]
        public IActionResult Register()
        {
            return View(new Usuarios());
        }
        [HttpPost]
        public IActionResult CrearCliente(Usuarios usuario)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.RegisterSuccess = false;
                    ViewBag.RegisterMessage = "Por favor, revise los campos del formulario para corregir los errores.";
                    return View("Register", usuario);
                }
                usuario.DineroDisponible = 0;

                if (string.IsNullOrWhiteSpace(usuario.Perfil))
                    usuario.Perfil = "Cliente";

                usuario.NumeroTarjeta = EnmascararTarjeta(usuario.NumeroTarjeta);

                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                ViewBag.RegisterSuccess = true;
                return View("Register", usuario);
            }
            catch (Exception ex)
            {
                ViewBag.RegisterSuccess = false;
                ViewBag.RegisterMessage = "Ocurrió un error al guardar el usuario: " + ex.Message;
                return View("Register", usuario);
            }
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







        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ForgotSuccess = false;
                ViewBag.ForgotMessage = "Por favor, revise los campos.";
                return View("ForgotPassword", model);
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == model.Correo);
            if (usuario == null)
            {
                ViewBag.ForgotSuccess = false;
                ViewBag.ForgotMessage = "No existe un usuario con ese correo.";
                return View("ForgotPassword", model);
            }

            // Generar código temporal
            var codigo = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            // Guardar en BD o tabla temporal
            usuario.CodigoRecuperacion = codigo;
            usuario.CodigoExpira = DateTime.UtcNow.AddMinutes(15);
            _context.SaveChanges();

            // Enviar correo con el código (usando SMTP o servicio externo)
            await _emailService.SendAsync(
                                        model.Correo,
                                        "Recuperación de contraseña - Atelier 27",
                                        $@"
                                        <h2>Recuperación de contraseña</h2>
                                        <p>Hola {usuario.NombreCompleto},</p>
                                        <p>Has solicitado recuperar tu contraseña. 
                                        Ingresa el siguiente código en la aplicación:</p>
                                        <h3 style='color:#007bff;'>{codigo}</h3>
                                        <p>Este código expira en 15 minutos.</p>
                                        <br/>
                                        <p>Si no solicitaste este cambio, ignora este correo.</p>
                                        <p>Atelier 27</p>
                                        "
                                    );

            ViewBag.ForgotSuccess = true;
            return View("ForgotPassword", model);
        }
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View(new ResetPasswordViewModel());
        }
        [HttpPost]
        public IActionResult ConfirmCode(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ResetSuccess = false;
                ViewBag.ResetMessage = "Por favor, complete los campos correctamente.";
                return View("ResetPassword", model);
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.CodigoRecuperacion == model.CodigoRecuperacion);

            if (usuario == null || usuario.CodigoExpira < DateTime.UtcNow)
            {
                ViewBag.ResetSuccess = false;
                ViewBag.ResetMessage = "El código es inválido o ha expirado.";
                return View("ResetPassword", model);
            }

            usuario.Contrasena = model.Contrasena; // recuerda aplicar hash aquí
            usuario.CodigoRecuperacion = null;
            usuario.CodigoExpira = null;

            _context.SaveChanges();

            ViewBag.ResetSuccess = true;
            return View("ResetPassword", model);
        }
    }
}


