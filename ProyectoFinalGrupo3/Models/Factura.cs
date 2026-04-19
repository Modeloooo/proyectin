using System;
using System.Collections.Generic;

namespace ProyectoFinalGrupo3.Models;

public partial class Factura
{
    public int NumeroFactura { get; set; }

    public int? CodigoPedido { get; set; }

    public string? IdUsuario { get; set; }

    public decimal? Subtotal { get; set; }

    public decimal? Iva { get; set; }

    public decimal? Propina { get; set; }

    public decimal? CostoEmpaque { get; set; }

    public decimal? CostoDelivery { get; set; }

    public decimal? Total { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual Pedido? CodigoPedidoNavigation { get; set; }

   // public virtual ICollection<FacturaDetalle> FacturaDetalles { get; set; } = new List<FacturaDetalle>();

    //public virtual ICollection<Reversione> Reversiones { get; set; } = new List<Reversione>();
}
