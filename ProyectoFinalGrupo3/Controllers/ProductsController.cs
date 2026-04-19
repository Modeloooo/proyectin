using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalGrupo3.Data;
using ProyectoFinalGrupo3.Models;
using System;

namespace ProyectoFinalGrupo3.Controllers
{
    public class ProductsController : Controller
    {
        private readonly RestauranteDbContext _context;

        public ProductsController(RestauranteDbContext context)
        {
            _context = context;
        }

        // GET: Productos
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                                 .Include(p => p.CodigoCategoriaNavigation)
                                 .ToListAsync();

            // Aquí cargas las categorías para los modales
            ViewBag.Categorias = new SelectList(_context.Categorias, "CodigoCategoria", "Descripcion");

            return View(productos);
        }



        public IActionResult Create()
        {
            ViewBag.Categorias = new SelectList(_context.Categorias, "CodigoCategoria", "Descripcion");
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Producto producto)
        {
            try
            {
                // Validación básica: nombre obligatorio
                if (string.IsNullOrEmpty(producto.Nombre))
                {
                    TempData["Error"] = "El nombre del producto es obligatorio";
                    return RedirectToAction("Index");
                }

                // Validación: categoría obligatoria
                if (producto.CodigoCategoria <= 0)
                {
                    TempData["Error"] = "Debe seleccionar una categoría válida";
                    return RedirectToAction("Index");
                }

                // Validación: precio mayor a 0
                if (producto.Precio <= 0)
                {
                    TempData["Error"] = "El precio debe ser mayor a 0";
                    return RedirectToAction("Index");
                }

                // Validación: cantidad mayor o igual a 0
                if (producto.Cantidad < 0)
                {
                    TempData["Error"] = "La cantidad no puede ser negativa";
                    return RedirectToAction("Index");
                }

                // Verificar si ya existe un producto con el mismo nombre en la misma categoría
                if (_context.Productos.Any(p => p.Nombre == producto.Nombre && p.CodigoCategoria == producto.CodigoCategoria))
                {
                    TempData["Error"] = "Ya existe un producto con ese nombre en la categoría seleccionada";
                    return RedirectToAction("Index");
                }

                // Guardar producto
                _context.Productos.Add(producto);
                _context.SaveChanges();

                TempData["Success"] = "Producto guardado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al guardar el producto: " + ex.Message;
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var producto = _context.Productos.Find(id);

                if (producto == null)
                {
                    TempData["Error"] = "Producto no encontrado";
                    return RedirectToAction("Index");
                }

                // Verificar si el producto está asociado a algún pedido detalle
                bool tienePedidoDetalle = _context.PedidoDetalles.Any(pd => pd.CodigoProducto == id);

                if (tienePedidoDetalle)
                {
                    TempData["Error"] = "No se puede eliminar, el producto está asociado a pedidos.";
                    return RedirectToAction("Index");
                }

                _context.Productos.Remove(producto);
                _context.SaveChanges();

                TempData["Success"] = "Producto eliminado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al eliminar el producto: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Editar(Producto producto)
        {
            try
            {
                // Validaciones básicas
                if (string.IsNullOrEmpty(producto.Nombre))
                {
                    TempData["Error"] = "El nombre del producto es obligatorio";
                    return RedirectToAction("Index");
                }

                if (producto.Precio <= 0)
                {
                    TempData["Error"] = "El precio debe ser mayor a 0";
                    return RedirectToAction("Index");
                }

                if (producto.Cantidad < 0)
                {
                    TempData["Error"] = "La cantidad no puede ser negativa";
                    return RedirectToAction("Index");
                }

                // Buscar producto en DB
                var prod = _context.Productos.Find(producto.CodigoProducto);

                if (prod == null)
                {
                    TempData["Error"] = "Producto no encontrado";
                    return RedirectToAction("Index");
                }

                // Actualizar campos
                prod.Nombre = producto.Nombre;
                prod.CodigoCategoria = producto.CodigoCategoria;
                prod.Precio = producto.Precio;
                prod.RequiereEmpaque = producto.RequiereEmpaque;
                prod.CostoEmpaque = producto.CostoEmpaque;
                prod.Cantidad = producto.Cantidad;
                prod.Estado = producto.Estado;
                prod.UrlImagen = producto.UrlImagen;

                _context.SaveChanges();

                TempData["Success"] = "Producto actualizado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al actualizar el producto: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

    }
}