using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalGrupo3.Data;
using ProyectoFinalGrupo3.Models;
using System.Security.Claims;

namespace ProyectoFinalGrupo3.Controllers
{
    [Authorize(Roles = "Cajero")]
    public class CashierController : Controller
    {
        private readonly RestauranteDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;

        // Configuración de delivery (puede moverse a appsettings.json)
        private const decimal COSTO_DELIVERY = 2000m;
        private const decimal IVA = 0.13m;
        private const decimal PROPINA = 0.10m;

        public CashierController(RestauranteDbContext context, IEmailService emailService, IWebHostEnvironment env)
        {
            _context = context;
            _emailService = emailService;
            _env = env;
        }

        public IActionResult Index()
        {
            var pedidos = _context.Pedidos
                .Where(p => p.Estado == "Servido")
                .Include(p => p.NumeroMesaNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(d => d.CodigoProductoNavigation)
                .OrderBy(p => p.Fecha)
                .ToList();

            return View(pedidos);
        }

        [HttpGet]
        public IActionResult Facturar(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.NumeroMesaNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(d => d.CodigoProductoNavigation)
                .FirstOrDefault(p => p.CodigoPedido == id);

            if (pedido == null)
            {
                TempData["Error"] = "Pedido no encontrado";
                return RedirectToAction("Index");
            }

            if (pedido.Estado != "Servido")
            {
                TempData["Error"] = "El pedido no está listo para cobro";
                return RedirectToAction("Index");
            }

            // Validar stock antes de facturar
            foreach (var detalle in pedido.PedidoDetalles)
            {
                var producto = detalle.CodigoProductoNavigation;
                if (producto == null || producto.Cantidad < detalle.Cantidad)
                {
                    TempData["Error"] = $"Stock insuficiente para: {producto?.Nombre ?? "Producto"}";
                    return RedirectToAction("Index");
                }
            }

            // Calcular totales
            decimal subtotal = pedido.PedidoDetalles.Sum(d => d.Cantidad * d.PrecioUnitario);
            decimal iva = subtotal * IVA;
            decimal propina = pedido.TipoPedido == "Dine-in" ? subtotal * PROPINA : 0;

            decimal costoEmpaque = 0;
            if (pedido.TipoPedido == "Takeout")
            {
                costoEmpaque = pedido.PedidoDetalles.Sum(d =>
                    (d.CodigoProductoNavigation?.CostoEmpaque ?? 0) * d.Cantidad);
            }

            decimal costoDelivery = pedido.TipoPedido == "Delivery" ? COSTO_DELIVERY : 0;
            decimal total = subtotal + iva + propina + costoEmpaque + costoDelivery;

            ViewBag.Subtotal = subtotal;
            ViewBag.IVA = iva;
            ViewBag.Propina = propina;
            ViewBag.CostoEmpaque = costoEmpaque;
            ViewBag.CostoDelivery = costoDelivery;
            ViewBag.Total = total;

            return View(pedido);
        }

        [HttpPost]
        public async Task<IActionResult> ProcesarPago(int codigoPedido, decimal montoPagado)
        {
            try
            {
                var pedido = _context.Pedidos
                    .Include(p => p.IdUsuarioNavigation)
                    .Include(p => p.PedidoDetalles)
                        .ThenInclude(d => d.CodigoProductoNavigation)
                    .FirstOrDefault(p => p.CodigoPedido == codigoPedido);

                if (pedido == null)
                {
                    TempData["Error"] = "Pedido no encontrado";
                    return RedirectToAction("Index");
                }

                // Calcular totales
                decimal subtotal = pedido.PedidoDetalles.Sum(d => d.Cantidad * d.PrecioUnitario);
                decimal iva = subtotal * IVA;
                decimal propina = pedido.TipoPedido == "Dine-in" ? subtotal * PROPINA : 0;

                decimal costoEmpaque = 0;
                if (pedido.TipoPedido == "Takeout")
                {
                    costoEmpaque = pedido.PedidoDetalles.Sum(d =>
                        (d.CodigoProductoNavigation?.CostoEmpaque ?? 0) * d.Cantidad);
                }

                decimal costoDelivery = pedido.TipoPedido == "Delivery" ? COSTO_DELIVERY : 0;
                decimal totalCalculado = subtotal + iva + propina + costoEmpaque + costoDelivery;

                // Aplicar dinero disponible del usuario
                var usuario = pedido.IdUsuarioNavigation;
                decimal dineroUsado = 0;

                if (usuario != null && usuario.DineroDisponible > 0)
                {
                    dineroUsado = Math.Min(usuario.DineroDisponible ?? 0, totalCalculado);
                    usuario.DineroDisponible -= dineroUsado;
                    totalCalculado -= dineroUsado;
                }

                // Validar monto pagado
                if (montoPagado < totalCalculado)
                {
                    TempData["Error"] = "El monto pagado es insuficiente";
                    return RedirectToAction("Facturar", new { id = codigoPedido });
                }

                // Calcular vuelto y guardarlo como dinero disponible
                decimal vuelto = montoPagado - totalCalculado;
                if (vuelto > 0 && usuario != null)
                {
                    usuario.DineroDisponible = (usuario.DineroDisponible ?? 0) + vuelto;
                }

                // Descontar stock
                foreach (var detalle in pedido.PedidoDetalles)
                {
                    var producto = detalle.CodigoProductoNavigation;
                    if (producto != null)
                    {
                        producto.Cantidad -= detalle.Cantidad;
                    }
                }

                // Crear factura
                var factura = new Factura
                {
                    CodigoPedido = codigoPedido,
                    IdUsuario = pedido.IdUsuario,
                    Subtotal = subtotal,
                    Iva = iva,
                    Propina = propina,
                    CostoEmpaque = costoEmpaque,
                    CostoDelivery = costoDelivery,
                    Total = subtotal + iva + propina + costoEmpaque + costoDelivery,
                    Fecha = DateTime.Now
                };

                _context.Facturas.Add(factura);

                // Liberar mesa si es dine-in
                if (pedido.TipoPedido == "Dine-in" && pedido.NumeroMesa != null)
                {
                    var mesa = _context.Mesas.Find(pedido.NumeroMesa);
                    if (mesa != null)
                    {
                        mesa.Estado = "Libre";
                    }
                }

                pedido.Estado = "Pagado";
                pedido.Total = factura.Total ?? 0;

                await _context.SaveChangesAsync();

                // Generar y enviar PDF
                var pdfBytes = GenerarFacturaPDF(factura, pedido);
                await EnviarFacturaPorCorreo(pedido.IdUsuarioNavigation?.Correo, pdfBytes, factura.NumeroFactura);

                TempData["Success"] = $"Factura #{factura.NumeroFactura} generada correctamente";
                TempData["FacturaId"] = factura.NumeroFactura;

                return RedirectToAction("FacturaExitosa");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al procesar pago: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public IActionResult FacturaExitosa()
        {
            if (TempData["FacturaId"] == null)
                return RedirectToAction("Index");

            ViewBag.FacturaId = TempData["FacturaId"];
            return View();
        }

        public IActionResult HistorialFacturas()
        {
            var facturas = _context.Facturas
                .Include(f => f.CodigoPedidoNavigation)
                    .ThenInclude(p => p.NumeroMesaNavigation)
                .OrderByDescending(f => f.Fecha)
                .Take(100)
                .ToList();

            return View(facturas);
        }

        // Método temporal para generar PDF (usar librería como iTextSharp o DinkToPdf)
        private byte[] GenerarFacturaPDF(Factura factura, Pedido pedido)
        {
            // Implementación simplificada - en producción usar una librería PDF
            var html = $@"
                <h1>FACTURA #{factura.NumeroFactura}</h1>
                <p>Fecha: {factura.Fecha:dd/MM/yyyy HH:mm}</p>
                <p>Cliente: {pedido.IdUsuarioNavigation?.NombreCompleto}</p>
                <hr/>
                <h3>Detalle</h3>
                <table border='1'>
                    <tr><th>Producto</th><th>Cant</th><th>Precio</th><th>Subtotal</th></tr>";

            foreach (var d in pedido.PedidoDetalles)
            {
                html += $"<tr><td>{d.CodigoProductoNavigation?.Nombre}</td><td>{d.Cantidad}</td><td>₡{d.PrecioUnitario:N2}</td><td>₡{d.Cantidad * d.PrecioUnitario:N2}</td></tr>";
            }

            html += $@"
                </table>
                <p>Subtotal: ₡{factura.Subtotal:N2}</p>
                <p>IVA (13%): ₡{factura.Iva:N2}</p>
                <p>Propina: ₡{factura.Propina:N2}</p>
                <p>Empaque: ₡{factura.CostoEmpaque:N2}</p>
                <p>Delivery: ₡{factura.CostoDelivery:N2}</p>
                <h2>TOTAL: ₡{factura.Total:N2}</h2>";

            return System.Text.Encoding.UTF8.GetBytes(html);
        }

        private async Task EnviarFacturaPorCorreo(string correo, byte[] pdfBytes, int numeroFactura)
        {
            if (string.IsNullOrEmpty(correo)) return;

            try
            {
                await _emailService.SendAsync(
                    correo,
                    $"Factura #{numeroFactura} - Atelier 27",
                    $"<h2>Gracias por su compra</h2><p>Adjunto encontrará su factura #{numeroFactura}</p>"
                );
            }
            catch
            {
                // Log error pero no detener el proceso
            }
        }
    }
}