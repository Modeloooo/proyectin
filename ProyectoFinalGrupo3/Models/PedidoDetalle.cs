using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinalGrupo3.Models
{
    public partial class PedidoDetalle
    {
        [Key]
        public int Id { get; set; } // autogenerado

        [Required]
        public int CodigoPedido { get; set; } // FK a Pedido

        [Required]
        public int CodigoProducto { get; set; } // FK a Producto

        [Required]
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Required]
        public decimal PrecioUnitario { get; set; }

        [MaxLength(50)]
        public string Estado { get; set; } = "Pendiente";

        public virtual Pedido CodigoPedidoNavigation { get; set; }
        public virtual Producto CodigoProductoNavigation { get; set; }
    }
}