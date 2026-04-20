using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalGrupo3.Data;
using ProyectoFinalGrupo3.Models;
using System.Security.Claims;
using System.Text.Json;

namespace ProyectoFinalGrupo3.Controllers
{
    public class CarritoRequest
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
    }
    public class CarritoActionRequest
    {
        public int Id { get; set; }
        public string Action { get; set; }
    }
    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
    public class OrdersController : Controller
    {
        private readonly RestauranteDbContext _context;

        public OrdersController(RestauranteDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {

            var pedidos = _context.Pedidos
                .OrderByDescending(p => p.Fecha)
                .ToList();

            return View(pedidos);
        }

        [HttpPost]
        public IActionResult Crear(Pedido pedido, List<int> productoIds, List<int> cantidades)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pedido.TipoPedido))
                {
                    TempData["Error"] = "El tipo de pedido es obligatorio";
                    return RedirectToAction("Index");
                }

                if (productoIds == null || cantidades == null || productoIds.Count == 0 || cantidades.Count == 0)
                {
                    TempData["Error"] = "Debe agregar al menos un producto al pedido";
                    return RedirectToAction("Index");
                }

                if (productoIds.Count != cantidades.Count)
                {
                    TempData["Error"] = "La lista de productos y cantidades no coincide";
                    return RedirectToAction("Index");
                }

                if (pedido.TipoPedido == "Dine-in")
                {
                    if (pedido.NumeroMesa == null || pedido.NumeroMesa <= 0)
                    {
                        TempData["Error"] = "Debe seleccionar una mesa para pedidos Dine-in";
                        return RedirectToAction("Index");
                    }

                    var mesa = _context.Mesas.Find(pedido.NumeroMesa);
                    if (mesa == null)
                    {
                        TempData["Error"] = "La mesa seleccionada no existe";
                        return RedirectToAction("Index");
                    }

                    if (mesa.Estado != null && mesa.Estado.Trim().ToLower() == "ocupada")
                    {
                        TempData["Error"] = "La mesa seleccionada ya se encuentra ocupada";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    pedido.NumeroMesa = null;
                }

                pedido.Estado = "Pendiente";
                pedido.Fecha = DateTime.Now;

                _context.Pedidos.Add(pedido);
                _context.SaveChanges();

                for (int i = 0; i < productoIds.Count; i++)
                {
                    if (cantidades[i] <= 0)
                    {
                        TempData["Error"] = "Las cantidades deben ser mayores a 0";
                        return RedirectToAction("Index");
                    }

                    var producto = _context.Productos.Find(productoIds[i]);

                    if (producto == null)
                    {
                        TempData["Error"] = $"No se encontró el producto con código {productoIds[i]}";
                        return RedirectToAction("Index");
                    }

                    if (producto.Cantidad < cantidades[i])
                    {
                        TempData["Error"] = $"No hay suficiente stock para el producto {producto.Nombre}";
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
                }

                if (pedido.TipoPedido == "Dine-in" && pedido.NumeroMesa != null)
                {
                    var mesa = _context.Mesas.Find(pedido.NumeroMesa);
                    if (mesa != null)
                    {
                        mesa.Estado = "Ocupada";
                    }
                }

                _context.SaveChanges();

                TempData["Success"] = "Pedido creado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al crear el pedido: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Editar(Pedido pedido)
        {
            try
            {
                var pedidoDb = _context.Pedidos.Find(pedido.CodigoPedido);

                if (pedidoDb == null)
                {
                    TempData["Error"] = "Pedido no encontrado";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(pedido.TipoPedido))
                {
                    TempData["Error"] = "El tipo de pedido es obligatorio";
                    return RedirectToAction("Index");
                }

                if (pedido.TipoPedido == "Dine-in")
                {
                    if (pedido.NumeroMesa == null || pedido.NumeroMesa <= 0)
                    {
                        TempData["Error"] = "Debe seleccionar una mesa para pedidos Dine-in";
                        return RedirectToAction("Index");
                    }

                    var mesaNueva = _context.Mesas.Find(pedido.NumeroMesa);
                    if (mesaNueva == null)
                    {
                        TempData["Error"] = "La mesa seleccionada no existe";
                        return RedirectToAction("Index");
                    }

                    if (pedidoDb.NumeroMesa != pedido.NumeroMesa &&
                        mesaNueva.Estado != null &&
                        mesaNueva.Estado.Trim().ToLower() == "ocupada")
                    {
                        TempData["Error"] = "La mesa seleccionada ya está ocupada";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    if (pedidoDb.NumeroMesa != null)
                    {
                        var mesaAnterior = _context.Mesas.Find(pedidoDb.NumeroMesa);
                        if (mesaAnterior != null)
                        {
                            mesaAnterior.Estado = "Libre";
                        }
                    }

                    pedido.NumeroMesa = null;
                }

                if (pedidoDb.NumeroMesa != pedido.NumeroMesa)
                {
                    if (pedidoDb.NumeroMesa != null)
                    {
                        var mesaAnterior = _context.Mesas.Find(pedidoDb.NumeroMesa);
                        if (mesaAnterior != null)
                        {
                            mesaAnterior.Estado = "Libre";
                        }
                    }

                    if (pedido.TipoPedido == "Dine-in" && pedido.NumeroMesa != null)
                    {
                        var mesaNueva = _context.Mesas.Find(pedido.NumeroMesa);
                        if (mesaNueva != null)
                        {
                            mesaNueva.Estado = "Ocupada";
                        }
                    }
                }

                pedidoDb.IdUsuario = pedido.IdUsuario;
                pedidoDb.TipoPedido = pedido.TipoPedido;
                pedidoDb.NumeroMesa = pedido.NumeroMesa;
                pedidoDb.Estado = pedido.Estado;

                _context.SaveChanges();

                TempData["Success"] = "Pedido actualizado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el pedido: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var pedido = _context.Pedidos.Find(id);

                if (pedido == null)
                {
                    TempData["Error"] = "Pedido no encontrado";
                    return RedirectToAction("Index");
                }

                bool tieneFactura = _context.Facturas.Any(f => f.CodigoPedido == id);
                if (tieneFactura)
                {
                    TempData["Error"] = "No se puede eliminar el pedido porque ya tiene factura asociada";
                    return RedirectToAction("Index");
                }

                var detalles = _context.PedidoDetalles.Where(d => d.CodigoPedido == id).ToList();
                _context.PedidoDetalles.RemoveRange(detalles);

                if (pedido.NumeroMesa != null)
                {
                    var mesa = _context.Mesas.Find(pedido.NumeroMesa);
                    if (mesa != null)
                    {
                        mesa.Estado = "Libre";
                    }
                }

                _context.Pedidos.Remove(pedido);
                _context.SaveChanges();

                TempData["Success"] = "Pedido eliminado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el pedido: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AgregarDetalle(int codigoPedido, int codigoProducto, int cantidad)
        {
            try
            {
                if (cantidad <= 0)
                {
                    TempData["Error"] = "La cantidad debe ser mayor a 0";
                    return RedirectToAction("Index");
                }

                var pedido = _context.Pedidos.Find(codigoPedido);
                if (pedido == null)
                {
                    TempData["Error"] = "Pedido no encontrado";
                    return RedirectToAction("Index");
                }

                var producto = _context.Productos.Find(codigoProducto);
                if (producto == null)
                {
                    TempData["Error"] = "Producto no encontrado";
                    return RedirectToAction("Index");
                }

                if (producto.Cantidad  < cantidad)
                {
                    TempData["Error"] = "No hay suficiente stock para ese producto";
                    return RedirectToAction("Index");
                }

                var detalleExistente = _context.PedidoDetalles
                    .FirstOrDefault(d => d.CodigoPedido == codigoPedido && d.CodigoProducto == codigoProducto);

                if (detalleExistente != null)
                {
                    detalleExistente.Cantidad += cantidad;
                }
                else
                {
                    var detalle = new PedidoDetalle
                    {
                        CodigoPedido = codigoPedido,
                        CodigoProducto = codigoProducto,
                        Cantidad = cantidad,
                        PrecioUnitario = producto.Precio,
                        Estado = "Pendiente"
                    };

                    _context.PedidoDetalles.Add(detalle);
                }

                _context.SaveChanges();

                TempData["Success"] = "Detalle agregado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al agregar detalle: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EditarDetalle(PedidoDetalle detalle)
        {
            try
            {
                var detalleDb = _context.PedidoDetalles.Find(detalle.Id);

                if (detalleDb == null)
                {
                    TempData["Error"] = "Detalle no encontrado";
                    return RedirectToAction("Index");
                }

                if (detalle.Cantidad <= 0)
                {
                    TempData["Error"] = "La cantidad debe ser mayor a 0";
                    return RedirectToAction("Index");
                }

                var producto = _context.Productos.Find(detalleDb.CodigoProducto);
                if (producto == null)
                {
                    TempData["Error"] = "Producto no encontrado";
                    return RedirectToAction("Index");
                }

                if (producto.Cantidad< detalle.Cantidad)
                {
                    TempData["Error"] = "No hay suficiente stock para actualizar la cantidad";
                    return RedirectToAction("Index");
                }

                detalleDb.Cantidad = detalle.Cantidad;
                detalleDb.Estado = detalle.Estado;
                detalleDb.PrecioUnitario = detalle.PrecioUnitario;

                _context.SaveChanges();

                TempData["Success"] = "Detalle actualizado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el detalle: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EliminarDetalle(int id)
        {
            try
            {
                var detalle = _context.PedidoDetalles.Find(id);

                if (detalle == null)
                {
                    TempData["Error"] = "Detalle no encontrado";
                    return RedirectToAction("Index");
                }

                _context.PedidoDetalles.Remove(detalle);
                _context.SaveChanges();

                TempData["Success"] = "Detalle eliminado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el detalle: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        public IActionResult Detalles(int id)
        {
            var pedido = _context.Pedidos.FirstOrDefault(p => p.CodigoPedido == id);

            if (pedido == null)
            {
                TempData["Error"] = "Pedido no encontrado";
                return RedirectToAction("Index");
            }

            var detalles = _context.PedidoDetalles
                .Where(d => d.CodigoPedido == id)
                .ToList();

            ViewBag.Pedido = pedido;
            ViewBag.Productos = _context.Productos.ToList();

            return View(detalles);
        }









        public IActionResult CreateUserOrder()
        {
            var categorias = _context.Categorias.ToList();
            ViewBag.Categorias = categorias;

            var carrito = HttpContext.Session.GetObject<List<CarritoItem>>("Carrito") ?? new List<CarritoItem>();
            ViewBag.CartCount = carrito.Sum(c => c.Cantidad);

            var productos = _context.Productos.Include(p => p.CodigoCategoriaNavigation).ToList();
            return View(productos);
        }

        [HttpPost]
        public IActionResult AgregarAlCarritoAjax([FromBody] CarritoRequest req)
        {
            try
            {
                var producto = _context.Productos.FirstOrDefault(p => p.CodigoProducto == req.Id);
                if (producto == null) return Json(new { success = false, message = "Producto no encontrado" });

                if (req.Cantidad <= 0) return Json(new { success = false, message = "Cantidad inválida" });

                List<CarritoItem> carrito = HttpContext.Session.GetObject<List<CarritoItem>>("Carrito") ?? new List<CarritoItem>();

                var item = carrito.FirstOrDefault(c => c.CodigoProducto == req.Id);
                if (item != null)
                {
                    item.Cantidad += req.Cantidad;
                }
                else
                {
                    carrito.Add(new CarritoItem
                    {
                        CodigoProducto = producto.CodigoProducto,
                        Nombre = producto.Nombre,
                        Precio = producto.Precio,
                        Cantidad = req.Cantidad,
                        UrlImagen = producto.UrlImagen
                    });
                }

                HttpContext.Session.SetObject("Carrito", carrito);

                return Json(new { success = true, cartCount = carrito.Sum(c => c.Cantidad) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public IActionResult AgregarAlCarrito(int id)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.CodigoProducto == id);
            if (producto == null) return NotFound();

            List<CarritoItem> carrito = HttpContext.Session.GetObject<List<CarritoItem>>("Carrito") ?? new List<CarritoItem>();

            var item = carrito.FirstOrDefault(c => c.CodigoProducto == id);
            if (item != null)
            {
                item.Cantidad++;
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    CodigoProducto = producto.CodigoProducto,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = 1,
                    UrlImagen = producto.UrlImagen
                });
            }

            HttpContext.Session.SetObject("Carrito", carrito);

            return RedirectToAction("CreateUserOrder");
        }

        public IActionResult CartUserOrder()
        {
            var carrito = HttpContext.Session.GetObject<List<CarritoItem>>("Carrito") ?? new List<CarritoItem>();

            decimal subtotal = carrito.Sum(c => c.Precio * c.Cantidad);
            decimal iva = subtotal * 0.13m; // ejemplo: 12% IVA
            decimal envio = 0; // gratis o según lógica
            decimal total = subtotal + iva + envio;

            ViewBag.Subtotal = subtotal;
            ViewBag.IVA = iva;
            ViewBag.Envio = envio;
            ViewBag.Total = total;

            return View(carrito);
        }
        [HttpPost]
        public IActionResult AumentarCantidad(int id)
        {
            var carrito = HttpContext.Session.GetObject<List<CarritoItem>>("Carrito") ?? new List<CarritoItem>();
            var item = carrito.FirstOrDefault(c => c.CodigoProducto == id);
            if (item != null) item.Cantidad++;
            HttpContext.Session.SetObject("Carrito", carrito);
            return RedirectToAction("CartUserOrder");
        }

        [HttpPost]
        public IActionResult DisminuirCantidad(int id)
        {
            var carrito = HttpContext.Session.GetObject<List<CarritoItem>>("Carrito") ?? new List<CarritoItem>();
            var item = carrito.FirstOrDefault(c => c.CodigoProducto == id);
            if (item != null && item.Cantidad > 1) item.Cantidad--;
            HttpContext.Session.SetObject("Carrito", carrito);
            return RedirectToAction("CartUserOrder");
        }

        [HttpPost]
        public IActionResult EliminarDelCarrito(int id)
        {
            var carrito = HttpContext.Session.GetObject<List<CarritoItem>>("Carrito") ?? new List<CarritoItem>();
            carrito.RemoveAll(c => c.CodigoProducto == id);
            HttpContext.Session.SetObject("Carrito", carrito);
            return RedirectToAction("CartUserOrder");
        }

        [HttpPost]
        public IActionResult VaciarCarrito()
        {
            HttpContext.Session.SetObject("Carrito", new List<CarritoItem>());
            return RedirectToAction("CartUserOrder");
        }

        [HttpPost]
        public IActionResult UpdateCartItemAjax([FromBody] CarritoActionRequest req)
        {
            try
            {
                var carrito = HttpContext.Session.GetObject<List<CarritoItem>>("Carrito") ?? new List<CarritoItem>();
                var item = carrito.FirstOrDefault(c => c.CodigoProducto == req.Id);
                
                if (req.Action == "increase" && item != null)
                {
                    item.Cantidad++;
                }
                else if (req.Action == "decrease" && item != null && item.Cantidad > 1)
                {
                    item.Cantidad--;
                }
                else if (req.Action == "remove")
                {
                    carrito.RemoveAll(c => c.CodigoProducto == req.Id);
                    item = null;
                }
                else if (req.Action == "clear")
                {
                    carrito.Clear();
                    item = null;
                }

                HttpContext.Session.SetObject("Carrito", carrito);

                decimal subtotal = carrito.Sum(c => c.Precio * c.Cantidad);
                decimal iva = subtotal * 0.13m; // ejemplo: 13% IVA
                decimal envio = 0; // Configurar envío si es necesario
                decimal total = subtotal + iva + envio;

                return Json(new 
                { 
                    success = true, 
                    itemQuantity = item != null ? item.Cantidad : 0,
                    itemTotal = item != null ? (item.Cantidad * item.Precio) : 0,
                    subtotal = subtotal,
                    iva = iva,
                    total = total,
                    cartCount = carrito.Sum(c => c.Cantidad),
                    isEmpty = carrito.Count == 0
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult DataUserOrder()
        {
            var carrito = HttpContext.Session.GetObject<List<CarritoItem>>("Carrito") ?? new List<CarritoItem>();

            decimal subtotal = carrito.Sum(c => c.Precio * c.Cantidad);
            decimal iva = subtotal * 0.13m; // Fix: 13% IVA
            decimal envio = 0; // se ajusta según método
            decimal total = subtotal + iva + envio;

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Identificacion == usuarioId);

            var model = new DataUCViewModel
            {
                Usuario = usuario,
                Carrito = carrito,
                Subtotal = subtotal,
                IVA = iva,
                Envio = envio,
                Total = total
            };

            ViewBag.Productos = _context.Productos.ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult SeleccionarMetodo(string metodo)
        {
            var carrito = HttpContext.Session.GetObject<List<CarritoItem>>("Carrito") ?? new List<CarritoItem>();
            decimal subtotal = carrito.Sum(c => c.Precio * c.Cantidad);
            decimal iva = subtotal * 0.12m;

            decimal envio = 0;
            if (metodo == "Delivery")
            {
                // Aquí puedes calcular según zona, por ahora fijo
                envio = 2000;
            }

            decimal total = subtotal + iva + envio;

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Identificacion == usuarioId);

            var model = new DataUCViewModel
            {
                Usuario = usuario,
                Carrito = carrito,
                Subtotal = subtotal,
                IVA = iva,
                Envio = envio,
                Total = total,
                MetodoEntrega = metodo
            };

            ViewBag.Productos = _context.Productos.ToList();

            return View("DataUserOrder", model);
        }

        [HttpPost]
        public IActionResult ConfirmarPedido(string metodoEntrega)
        {
            var carrito = HttpContext.Session.GetObject<List<CarritoItem>>("Carrito") ?? new List<CarritoItem>();

            if (!carrito.Any())
            {
                TempData["Error"] = "El carrito está vacío";
                return RedirectToAction("CartUserOrder");
            }

            if (string.IsNullOrEmpty(metodoEntrega))
            {
                TempData["Error"] = "Debe seleccionar un método de entrega";
                return RedirectToAction("DataUserOrder");
            }

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Identificacion == usuarioId);

            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado";
                return RedirectToAction("CartUserOrder");
            }

            try
            {
                decimal subtotal = carrito.Sum(c => c.Precio * c.Cantidad);
                decimal iva = subtotal * 0.13m;
                decimal envio = metodoEntrega == "Delivery" ? 2000m : 0m;
                decimal total = subtotal + iva + envio;

                // Validar stock antes de crear pedido
                foreach (var item in carrito)
                {
                    var producto = _context.Productos.Find(item.CodigoProducto);
                    if (producto == null)
                    {
                        TempData["Error"] = $"Producto '{item.Nombre}' no encontrado";
                        return RedirectToAction("CartUserOrder");
                    }

                    if (producto.Cantidad < item.Cantidad)
                    {
                        TempData["Error"] = $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Cantidad}";
                        return RedirectToAction("CartUserOrder");
                    }
                }

                // Crear Pedido
                var pedido = new Pedido
                {
                    IdUsuario = usuario.Identificacion,
                    Fecha = DateTime.Now,
                    TipoPedido = metodoEntrega,
                    Estado = "Pendiente",
                    Total = total
                };

                _context.Pedidos.Add(pedido);
                _context.SaveChanges();

                // Crear Detalles
                foreach (var item in carrito)
                {
                    var detalle = new PedidoDetalle
                    {
                        CodigoPedido = pedido.CodigoPedido,
                        CodigoProducto = item.CodigoProducto,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Precio,
                        Estado = "Pendiente"
                    };
                    _context.PedidoDetalles.Add(detalle);
                }

                _context.SaveChanges();

                // Vaciar carrito
                HttpContext.Session.SetObject("Carrito", new List<CarritoItem>());

                TempData["Success"] = $"¡Pedido #{pedido.CodigoPedido} creado exitosamente!";
                TempData["PedidoCreado"] = pedido.CodigoPedido;

                return RedirectToAction("PedidoConfirmado");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al procesar el pedido: " + ex.Message;
                return RedirectToAction("CartUserOrder");
            }
        }
        public IActionResult PaymentUserOrder()
        {
            return View();
        }




        public IActionResult HistoryUserOrder()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pedidos = _context.Pedidos
                .Where(p => p.IdUsuario == usuarioId)
                .Include(p => p.PedidoDetalles)
                .ThenInclude(d => d.CodigoProductoNavigation) // para mostrar nombre del producto
                .OrderByDescending(p => p.Fecha)
                .ToList();

            return View(pedidos);
        }


        public IActionResult HistoryAccountantOrder()
        {
            return View();
        }
        public IActionResult CreateWaiterOrder()
        {
            return View();
        }
        public IActionResult DetailsOrdersKitchen()
        {
            return View();
        }
        public IActionResult DetailsOrdersCashier()
        {
            return View();
        }
        public IActionResult PedidoConfirmado()
        {
            if (TempData["PedidoCreado"] == null)
                return RedirectToAction("CreateUserOrder");

            ViewBag.PedidoId = TempData["PedidoCreado"];
            return View();
        }
    }
}
