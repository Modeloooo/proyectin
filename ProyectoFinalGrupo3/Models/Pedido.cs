using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinalGrupo3.Models
{
    public partial class Pedido
    {
        [Key]
        public int CodigoPedido { get; set; } // autogenerado

        [Required]
        [MaxLength(50)]
        public string IdUsuario { get; set; } // FK a Usuarios

        [Required]
        [MaxLength(20)]
        public string TipoPedido { get; set; } // Ej: "Mesa", "Delivery"

        public int? NumeroMesa { get; set; } // FK a Mesas, opcional

        [Required]
        [MaxLength(50)]
        public string Estado { get; set; } = "Pendiente";

        [Column(TypeName = "datetime")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [MaxLength(250)]
        public string? Observaciones { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; } = 0;

        public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
        public virtual Mesa? NumeroMesaNavigation { get; set; }
        public virtual Usuarios IdUsuarioNavigation { get; set; }
        public virtual ICollection<PedidoDetalle> PedidoDetalles { get; set; } = new List<PedidoDetalle>();
    }
}
