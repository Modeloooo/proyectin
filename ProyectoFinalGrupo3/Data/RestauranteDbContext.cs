using Microsoft.EntityFrameworkCore;
using ProyectoFinalGrupo3.Models;

namespace ProyectoFinalGrupo3.Data
{
    public class RestauranteDbContext : DbContext
    {
        public RestauranteDbContext(DbContextOptions<RestauranteDbContext> options) : base(options) { }
        
        public DbSet<Usuarios> Usuarios { get; set; }
        public virtual DbSet<Categorias> Categorias { get; set; }
        public virtual DbSet<Producto> Productos { get; set; }
        public virtual DbSet<Mesa> Mesas { get; set; }
        public virtual DbSet<Pedido> Pedidos { get; set; }
        public virtual DbSet<PedidoDetalle> PedidoDetalles { get; set; }
        public virtual DbSet<Factura> Facturas { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuarios>(entity =>
            {
                entity.HasKey(e => e.Identificacion);
                entity.Property(e => e.Identificacion).HasMaxLength(50);
                entity.Property(e => e.NombreCompleto).HasMaxLength(150);
                entity.Property(e => e.Genero).HasMaxLength(20);
                entity.Property(e => e.Correo).HasMaxLength(30);
                entity.Property(e => e.TipoTarjeta).HasMaxLength(20);
                entity.Property(e => e.NumeroTarjeta).HasMaxLength(20);
                entity.Property(e => e.DineroDisponible).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Contrasena).HasMaxLength(200);
                entity.Property(e => e.Perfil).HasMaxLength(15);
                entity.Property(e => e.CodigoRecuperacion).HasMaxLength(50);
                entity.Property(e => e.CodigoExpira).HasColumnType("datetime");
                entity.Property(e => e.UltimoLogin).HasColumnType("datetime");
            });

            modelBuilder.Entity<Categorias>(entity =>
            {
                entity.HasKey(e => e.CodigoCategoria).HasName("PK__Categori__3CEE2F4C17B78E1F");
                entity.Property(e => e.CodigoCategoria).ValueGeneratedOnAdd();
                entity.Property(e => e.Descripcion).HasMaxLength(100);
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(e => e.CodigoProducto).HasName("PK__Producto__785B009E45B1DF44");

                entity.Property(e => e.CodigoProducto).ValueGeneratedOnAdd(); // autogenerado
                entity.Property(e => e.Nombre).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Precio).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.RequiereEmpaque).HasDefaultValue(0);
                entity.Property(e => e.CostoEmpaque).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(e => e.Cantidad).IsRequired();
                entity.Property(e => e.Estado).HasDefaultValue(1);
                entity.Property(e => e.UrlImagen).HasMaxLength(300);

                entity.HasOne(d => d.CodigoCategoriaNavigation)
                    .WithMany(p => p.Productos)
                    .HasForeignKey(d => d.CodigoCategoria)
                    .HasConstraintName("FK_Productos_Categorias")
                    .OnDelete(DeleteBehavior.Restrict);

            });
            modelBuilder.Entity<Mesa>(entity =>
            {
                entity.HasKey(e => e.NumeroMesa).HasName("PK__Mesas__A5588DD32071D496");

                entity.Property(e => e.NumeroMesa).ValueGeneratedOnAdd(); // ahora autogenerado
                entity.Property(e => e.Capacidad).IsRequired();
                entity.Property(e => e.Estado)
                    .HasMaxLength(20)
                    .HasDefaultValue("Libre");
            });

            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(e => e.CodigoPedido);

                entity.Property(e => e.TipoPedido)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.Estado)
                    .HasMaxLength(50)
                    .HasDefaultValue("Pendiente");

                entity.Property(e => e.Fecha)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Observaciones)
                    .HasMaxLength(250);

                entity.Property(e => e.Total)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                entity.HasOne(d => d.NumeroMesaNavigation)
                    .WithMany(p => p.Pedidos)
                    .HasForeignKey(d => d.NumeroMesa)
                    .HasConstraintName("FK_Pedidos_Mesas");

                entity.HasOne(d => d.IdUsuarioNavigation)
                    .WithMany(p => p.Pedidos)
                    .HasForeignKey(d => d.IdUsuario)
                    .HasConstraintName("FK_Pedidos_Usuarios");
            });

            modelBuilder.Entity<PedidoDetalle>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Cantidad).IsRequired();
                entity.Property(e => e.PrecioUnitario)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Estado)
                    .HasMaxLength(50)
                    .HasDefaultValue("Pendiente");

                entity.HasOne(d => d.CodigoPedidoNavigation)
                    .WithMany(p => p.PedidoDetalles)
                    .HasForeignKey(d => d.CodigoPedido)
                    .HasConstraintName("FK_PedidoDetalle_Pedido");

                entity.HasOne(d => d.CodigoProductoNavigation)
                    .WithMany(p => p.PedidoDetalles) // inversa en Producto
                    .HasForeignKey(d => d.CodigoProducto)
                    .HasConstraintName("FK_PedidoDetalle_Producto");
            });



            modelBuilder.Entity<Factura>(entity =>
            {
                entity.HasKey(e => e.NumeroFactura);

                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Iva).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Propina).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CostoEmpaque).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CostoDelivery).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Fecha).HasColumnType("datetime");

                entity.HasOne(d => d.CodigoPedidoNavigation)
                    .WithMany(p => p.Facturas)
                    .HasForeignKey(d => d.CodigoPedido)
                    .HasConstraintName("FK_Facturas_Pedido");
            });
        }
    }
}

    