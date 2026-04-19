using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalGrupo3.Data;
using ProyectoFinalGrupo3.Models;

namespace ProyectoFinalGrupo3.Controllers
{
    [Authorize(Roles = "Cocinero")]
    public class KitchenController : Controller
    {
        private readonly RestauranteDbContext _context;

        public KitchenController(RestauranteDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var pedidos = _context.Pedidos
                .Where(p => p.Estado == "Pendiente" || p.Estado == "Preparación" || p.Estado == "Preparacion")
                .Include(p => p.NumeroMesaNavigation)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(d => d.CodigoProductoNavigation)
                .OrderBy(p => p.Fecha)
                .ToList();

            return View(pedidos);
        }

        [HttpPost]
        public IActionResult CambiarEstadoDetalle(int detalleId, string nuevoEstado)
        {
            try
            {
                var detalle = _context.PedidoDetalles
                    .Include(d => d.CodigoPedidoNavigation)
                    .FirstOrDefault(d => d.Id == detalleId);

                if (detalle == null)
                {
                    TempData["Error"] = "Detalle no encontrado";
                    return RedirectToAction("Index");
                }

                var estadosValidos = new[] { "Pendiente", "En preparación", "Listo" };
                if (!estadosValidos.Contains(nuevoEstado))
                {
                    TempData["Error"] = "Estado no válido";
                    return RedirectToAction("Index");
                }

                detalle.Estado = nuevoEstado;
                _context.SaveChanges();

                // Verificar si todos los detalles están listos
                ActualizarEstadoPedido(detalle.CodigoPedido);

                TempData["Success"] = $"Ítem actualizado a: {nuevoEstado}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cambiar estado: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private void ActualizarEstadoPedido(int codigoPedido)
        {
            var pedido = _context.Pedidos
                .Include(p => p.PedidoDetalles)
                .FirstOrDefault(p => p.CodigoPedido == codigoPedido);

            if (pedido == null) return;

            var detalles = pedido.PedidoDetalles.ToList();

            if (detalles.All(d => d.Estado == "Listo"))
            {
                pedido.Estado = "Servido";
            }
            else if (detalles.Any(d => d.Estado == "En preparación"))
            {
                pedido.Estado = "Preparación";
            }

            _context.SaveChanges();
        }

        public IActionResult Historial()
        {
            var pedidos = _context.Pedidos
                .Where(p => p.Estado == "Servido" || p.Estado == "Cancelado")
                .Include(p => p.NumeroMesaNavigation)
                .OrderByDescending(p => p.Fecha)
                .Take(50)
                .ToList();

            return View(pedidos);
        }
    }
}