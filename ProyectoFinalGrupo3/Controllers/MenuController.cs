using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinalGrupo3.Controllers
{
    public class MenuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
