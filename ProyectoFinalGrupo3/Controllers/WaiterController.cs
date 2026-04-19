using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalGrupo3.Data;
using ProyectoFinalGrupo3.Models;
using System.Security.Claims;

namespace ProyectoFinalGrupo3.Controllers
{
    [Authorize(Roles = "Salonero")]
    public class WaiterController : Controller
    {
        private readonly RestauranteDbContext _context;

        public WaiterController(RestauranteDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var mesas = _context.Mesas
                .Where(m => m.Estado == "Libre")
                .OrderBy(m => m.NumeroMesa)
                .ToList();

            var productos = _context.Productos
                .Where(p => p.Estado == 1 && p.Cantidad > 0)
                .Include(p => p.CodigoCategoriaNavigation)
                .ToList();

            ViewBag.Mesas = mesas;
            ViewBag.Productos = productos;

            return View();
        }

        [HttpPost]
        public IActionResult CrearPedido(int numeroMesa, List<int> productoIds, List<int> cantidades, string observaciones)
        {
            try
            {
                if (numeroMesa <= 0)
                {
                    TempData["Error"] = "Debe seleccionar una mesa";
                    return RedirectToAction("Index");
                }

                if (productoIds == null || !productoIds.Any())
                {
                    TempData["Error"] = "Debe agregar al menos un producto";
                    return RedirectToAction("Index");
                }

                var mesa = _context.Mesas.Find(numeroMesa);
                if (mesa == null)
                {
                    TempData["Error"] = "Mesa no encontrada";
                    return RedirectToAction("Index");
                }

                if (mesa.Estado != "Libre")
                {
                    TempData["Error"] = "La mesa no está disponible";
                    return RedirectToAction("Index");
                }

                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var pedido = new Pedido
                {
                    IdUsuario = usuarioId,
                    TipoPedido = "Dine-in",
                    NumeroMesa = numeroMesa,
                    Estado = "Pendiente",
                    Observaciones = observaciones,
                    Fecha = DateTime.Now,
                    Total = 0
                };

                _context.Pedidos.Add(pedido);
                _context.SaveChanges();

                decimal total = 0;

                for (int i = 0; i < productoIds.Count; i++)
                {
                    var producto = _context.Productos.Find(productoIds[i]);
                    if (producto == null) continue;

                    if (producto.Cantidad < cantidades[i])
                    {
                        TempData["Error"] = $"Stock insuficiente para {producto.Nombre}";
                        return RedirectToAction("Index");
                    }

                    var detalle = new PedidoDetalle
                    {
                        CodigoPedido = pedido.CodigoPedido,
                        CodigoProducto = producto.CodigoProducto,
                        Cantidad = cantidades[i],
                        PrecioUnitario = producto.Precio,
                        Estado = "Pendiente"
                    };

                    _context.PedidoDetalles.Add(detalle);
                    total += producto.Precio * cantidades[i];
                }

                pedido.Total = total;
                mesa.Estado = "Ocupada";

                _context.SaveChanges();

                TempData["Success"] = $"Pedido #{pedido.CodigoPedido} creado y enviado a cocina";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear pedido: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        public IActionResult MisPedidos()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pedidos = _context.Pedidos
                .Where(p => p.IdUsuario == usuarioId)
                .Include(p => p.NumeroMesaNavigation)
                .OrderByDescending(p => p.Fecha)
                .ToList();

            return View(pedidos);
        }
        public IActionResult DetallePedido(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.NumeroMesaNavigation)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(d => d.CodigoProductoNavigation)
                .FirstOrDefault(p => p.CodigoPedido == id);

            if (pedido == null)
            {
                TempData["Error"] = "Pedido no encontrado";
                return RedirectToAction("MisPedidos");
            }

            return View(pedido);
        }
    }
}