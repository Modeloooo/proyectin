using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalGrupo3.Data;
using ProyectoFinalGrupo3.Models;

namespace ProyectoFinalGrupo3.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly RestauranteDbContext _context;

        public HistoryController(RestauranteDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Administrador,Contador")]
        public IActionResult Index(string? usuarioId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.Facturas
                .Include(f => f.CodigoPedidoNavigation)
                    .ThenInclude(p => p.IdUsuarioNavigation)
                .Include(f => f.CodigoPedidoNavigation)
                    .ThenInclude(p => p.NumeroMesaNavigation)
                .AsQueryable();

            // Filtro por usuario (Admin y Contador)
            if (!string.IsNullOrEmpty(usuarioId))
            {
                query = query.Where(f => f.IdUsuario == usuarioId);
                ViewBag.UsuarioFiltro = usuarioId;
            }

            // Filtro por rango de fechas (Contador también puede)
            if (User.IsInRole("Contador") || User.IsInRole("Administrador"))
            {
                if (fechaInicio.HasValue)
                {
                    query = query.Where(f => f.Fecha >= fechaInicio.Value);
                    ViewBag.FechaInicio = fechaInicio.Value.ToString("yyyy-MM-dd");
                }

                if (fechaFin.HasValue)
                {
                    var fechaFinConHora = fechaFin.Value.AddDays(1).AddSeconds(-1);
                    query = query.Where(f => f.Fecha <= fechaFinConHora);
                    ViewBag.FechaFin = fechaFin.Value.ToString("yyyy-MM-dd");
                }
            }

            var facturas = query.OrderByDescending(f => f.Fecha).ToList();

            // Cargar lista de usuarios para el filtro
            ViewBag.Usuarios = _context.Usuarios
                .OrderBy(u => u.NombreCompleto)
                .ToList();

            return View(facturas);
        }

        [Authorize(Roles = "Administrador,Contador")]
        public IActionResult DetalleFactura(int id)
        {
            var factura = _context.Facturas
                .Include(f => f.CodigoPedidoNavigation)
                    .ThenInclude(p => p.IdUsuarioNavigation)
                .Include(f => f.CodigoPedidoNavigation)
                    .ThenInclude(p => p.NumeroMesaNavigation)
                .Include(f => f.CodigoPedidoNavigation)
                    .ThenInclude(p => p.PedidoDetalles)
                        .ThenInclude(d => d.CodigoProductoNavigation)
                .FirstOrDefault(f => f.NumeroFactura == id);

            if (factura == null)
            {
                TempData["Error"] = "Factura no encontrada";
                return RedirectToAction("Index");
            }

            return View(factura);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Contador")]
        public IActionResult Reversar(int facturaId)
        {
            try
            {
                var factura = _context.Facturas
                    .Include(f => f.CodigoPedidoNavigation)
                        .ThenInclude(p => p.IdUsuarioNavigation)
                    .Include(f => f.CodigoPedidoNavigation)
                        .ThenInclude(p => p.PedidoDetalles)
                            .ThenInclude(d => d.CodigoProductoNavigation)
                    .FirstOrDefault(f => f.NumeroFactura == facturaId);

                if (factura == null)
                {
                    TempData["Error"] = "Factura no encontrada";
                    return RedirectToAction("Index");
                }

                // Validar 24 horas
                if (factura.Fecha == null || (DateTime.Now - factura.Fecha.Value).TotalHours > 24)
                {
                    TempData["Error"] = "Solo se pueden reversar facturas con menos de 24 horas de antigüedad";
                    return RedirectToAction("Index");
                }

                var pedido = factura.CodigoPedidoNavigation;
                if (pedido == null)
                {
                    TempData["Error"] = "Pedido no encontrado";
                    return RedirectToAction("Index");
                }

                // Devolver stock
                foreach (var detalle in pedido.PedidoDetalles)
                {
                    var producto = detalle.CodigoProductoNavigation;
                    if (producto != null)
                    {
                        producto.Cantidad += detalle.Cantidad;
                    }
                }

                // Devolver dinero como saldo a favor
                var usuario = pedido.IdUsuarioNavigation;
                if (usuario != null && factura.Total > 0)
                {
                    usuario.DineroDisponible = (usuario.DineroDisponible ?? 0) + factura.Total;
                }

                // Liberar mesa si era dine-in
                if (pedido.TipoPedido == "Dine-in" && pedido.NumeroMesa != null)
                {
                    var mesa = _context.Mesas.Find(pedido.NumeroMesa);
                    if (mesa != null)
                    {
                        mesa.Estado = "Libre";
                    }
                }

                // Cambiar estado del pedido
                pedido.Estado = "Cancelado";

                // Eliminar factura
                _context.Facturas.Remove(factura);

                _context.SaveChanges();

                TempData["Success"] = $"Factura #{facturaId} reversada correctamente. El monto de ₡{factura.Total:N2} queda como saldo a favor del cliente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al reversar factura: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Cliente")]
        public IActionResult MisCompras()
        {
            var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var facturas = _context.Facturas
                .Where(f => f.IdUsuario == usuarioId)
                .Include(f => f.CodigoPedidoNavigation)
                    .ThenInclude(p => p.PedidoDetalles)
                .OrderByDescending(f => f.Fecha)
                .ToList();

            return View(facturas);
        }
    }
}