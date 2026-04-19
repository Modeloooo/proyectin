using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinalGrupo3.Controllers
{
    public class InvoicesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
