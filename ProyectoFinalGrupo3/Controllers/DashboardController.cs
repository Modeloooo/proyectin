using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalGrupo3.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProyectoFinalGrupo3.Controllers
{
    public class DashboardController : Controller
    {
        private readonly RestauranteDbContext _context;
        public DashboardController(RestauranteDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var token = HttpContext.Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            ViewData["Role"] = role;

            if (role != null && role.Equals("Cliente", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("HomeUser", "Users");
            }

            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Identificacion == id);

            return View(usuario);
        }


    }
}

