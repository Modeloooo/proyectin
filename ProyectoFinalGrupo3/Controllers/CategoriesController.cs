using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinalGrupo3.Data;
using ProyectoFinalGrupo3.Models;

namespace ProyectoFinalGrupo3.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly RestauranteDbContext _context;
        public CategoriesController(RestauranteDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Index()
        {
            var lista = _context.Categorias.ToList();
            return View(lista);
        }
        [HttpPost]
        public IActionResult Crear(Categorias categoria)
        {
            try
            {
                if (string.IsNullOrEmpty(categoria.Descripcion))
                {
                    TempData["Error"] = "La descripción es obligatoria";
                    return RedirectToAction("Index");
                }

                if (_context.Categorias.Any(c => c.CodigoCategoria == categoria.CodigoCategoria))
                {
                    TempData["Error"] = "Ya existe una categoría con ese código";
                    return RedirectToAction("Index");
                }

                _context.Categorias.Add(categoria);
                _context.SaveChanges();

                TempData["Success"] = "Categoría guardada correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al guardar la categoría: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var categoria = _context.Categorias.Find(id);

                if (categoria == null)
                {
                    TempData["Error"] = "Categoría no encontrada";
                    return RedirectToAction("Index");
                }

                bool tieneProductos = _context.Productos.Any(p => p.CodigoCategoria == id);

                if (tieneProductos)
                {
                    TempData["Error"] = "No se puede eliminar, tiene productos asociados";
                    return RedirectToAction("Index");
                }

                _context.Categorias.Remove(categoria);
                _context.SaveChanges();

                TempData["Success"] = "Categoría eliminada correctamente";
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al eliminar la categoría.";
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Editar(Categorias categoria)
        {
            try
            {
                if (string.IsNullOrEmpty(categoria.Descripcion))
                {
                    TempData["Error"] = "La descripción es obligatoria";
                    return RedirectToAction("Index");
                }

                var cat = _context.Categorias.Find(categoria.CodigoCategoria);

                if (cat == null)
                {
                    TempData["Error"] = "Categoría no encontrada";
                    return RedirectToAction("Index");
                }

                cat.Descripcion = categoria.Descripcion;

                _context.SaveChanges();

                TempData["Success"] = "Categoría actualizada correctamente";
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al actualizar la categoría.";
            }

            return RedirectToAction("Index");
        }
    }

}


