using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinalGrupo3.Data;
using ProyectoFinalGrupo3.Models;

namespace ProyectoFinalGrupo3.Controllers
{
    public class TablesController : Controller
    {
        private readonly RestauranteDbContext _context;
        public TablesController(RestauranteDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Administrador")]
        public IActionResult Index()
        {
            var mesas = _context.Mesas.ToList();
            return View(mesas);
        }
        [HttpPost]
        public IActionResult Crear(Mesa mesa)
        {
            try
            {
                if (mesa.NumeroMesa <= 1)
                {
                    TempData["Error"] = "El número de mesa es obligatorio";
                    return RedirectToAction("Index");
                }

                if (mesa.Capacidad <= 1)
                {
                    TempData["Error"] = "La capacidad de mesa es obligatorio";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrEmpty(mesa.Estado))
                {
                    TempData["Error"] = "El estado es obligatorio";
                    return RedirectToAction("Index");
                }

                if (_context.Mesas.Any(x => x.NumeroMesa == mesa.NumeroMesa))
                {
                    TempData["Error"] = "Ya existe una mesa con ese número";
                    return RedirectToAction("Index");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.AbrirModal = true;
                    var mesas = _context.Mesas.ToList();
                    return View("Index", mesas);
                }

                _context.Mesas.Add(mesa);
                _context.SaveChanges();

                TempData["Success"] = "Mesa creada correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al guardar la mesa: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Editar(Mesa mesa)
        {
            try
            {
                if (mesa.Capacidad <= 0)
                {
                    TempData["Error"] = "La capacidad de mesa es obligatioria";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrEmpty(mesa.Estado))
                {
                    TempData["Error"] = "El estado es obligatorio";
                    return RedirectToAction("Index");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.AbrirModalEditar = true;

                    var mesas = _context.Mesas.ToList();
                    return View("Index", mesas);
                }

                var mesaDb = _context.Mesas.Find(mesa.NumeroMesa);

                if (mesaDb == null)
                {
                    TempData["Error"] = "Mesa no encontrada";
                    return RedirectToAction("Index");
                }

                mesaDb.Capacidad = mesa.Capacidad;
                mesaDb.Estado = mesa.Estado;

                _context.SaveChanges();

                TempData["Success"] = "Mesa actualizada correctamente";
            }
            catch
            {
                TempData["Error"] = "Error al actualizar la mesa";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var mesa = _context.Mesas.Find(id);

                if (mesa == null)
                {
                    TempData["Error"] = "Mesa no encontrada";
                    return RedirectToAction("Index");
                }

                _context.Mesas.Remove(mesa);
                _context.SaveChanges();

                TempData["Success"] = "Mesa eliminada correctamente";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Error al eliminar la mesa";
                return RedirectToAction("Index");
            }
        }
    }
}
