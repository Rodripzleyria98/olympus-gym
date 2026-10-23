using Microsoft.EntityFrameworkCore;
using Olympus.Api.Models;

namespace Olympus.Api.Data;

public class OlympusDbContext(DbContextOptions<OlympusDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<PlanMembresia> PlanesMembresia => Set<PlanMembresia>();
    public DbSet<MembresiaUsuario> MembresiasUsuario => Set<MembresiaUsuario>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<PublicacionForo> PublicacionesForo => Set<PublicacionForo>();
    public DbSet<PlanEntrenamiento> PlanesEntrenamiento => Set<PlanEntrenamiento>();
    public DbSet<DiaEntrenamiento> DiasEntrenamiento => Set<DiaEntrenamiento>();
    public DbSet<EjercicioItem> Ejercicios => Set<EjercicioItem>();
    public DbSet<CajaTurno> CajasTurno => Set<CajaTurno>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<VentaProducto> VentasProducto => Set<VentaProducto>();
    public DbSet<PaseDiario> PasesDiarios => Set<PaseDiario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(usuario => usuario.Id);
            entity.Property(usuario => usuario.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(usuario => usuario.Apellido).HasMaxLength(100).IsRequired();
            entity.Property(usuario => usuario.Dni).HasMaxLength(20).IsRequired();
            entity.HasIndex(usuario => usuario.Dni).IsUnique();
            entity.Property(usuario => usuario.Email).HasMaxLength(254).IsRequired();
            entity.HasIndex(usuario => usuario.Email).IsUnique();
            entity.Property(usuario => usuario.PasswordHash).IsRequired();
            entity.Property(usuario => usuario.Rol).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<PlanMembresia>(entity =>
        {
            entity.HasKey(plan => plan.Id);
            entity.Property(plan => plan.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(plan => plan.Descripcion).HasMaxLength(500).IsRequired();
            entity.Property(plan => plan.Precio).HasPrecision(10, 2);
        });

        modelBuilder.Entity<MembresiaUsuario>(entity =>
        {
            entity.HasKey(membresia => membresia.Id);
            entity.Property(membresia => membresia.Estado).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(membresia => membresia.Usuario)
                .WithMany(usuario => usuario.Membresias)
                .HasForeignKey(membresia => membresia.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(membresia => membresia.PlanMembresia)
                .WithMany(plan => plan.Membresias)
                .HasForeignKey(membresia => membresia.PlanMembresiaId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(membresia => new { membresia.UsuarioId, membresia.FechaFin });
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(pago => pago.Id);
            entity.Property(pago => pago.Monto).HasPrecision(10, 2);
            entity.Property(pago => pago.MetodoPago).HasMaxLength(50).IsRequired();
            entity.Property(pago => pago.ComprobanteUrl).HasMaxLength(2048);
            entity.Property(pago => pago.PreferenciaId).HasMaxLength(100);
            entity.Property(pago => pago.TransaccionExternaId).HasMaxLength(50);
            entity.Property(pago => pago.PeriodoMeses).HasDefaultValue(1);
            entity.Property(pago => pago.EstadoPago).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(pago => pago.CajaTurnoId);
            entity.HasIndex(pago => pago.PreferenciaId).IsUnique();
            entity.HasIndex(pago => pago.TransaccionExternaId).IsUnique();
            entity.HasOne(pago => pago.Usuario)
                .WithMany(usuario => usuario.Pagos)
                .HasForeignKey(pago => pago.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(pago => pago.MembresiaUsuario)
                .WithMany(membresia => membresia.Pagos)
                .HasForeignKey(pago => pago.MembresiaUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(pago => pago.CajaTurno)
                .WithMany(caja => caja.Pagos)
                .HasForeignKey(pago => pago.CajaTurnoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PublicacionForo>(entity =>
        {
            entity.HasKey(publicacion => publicacion.Id);
            entity.Property(publicacion => publicacion.Titulo).HasMaxLength(200).IsRequired();
            entity.Property(publicacion => publicacion.Contenido).HasMaxLength(10000).IsRequired();
            entity.Property(publicacion => publicacion.ImagenUrl).HasMaxLength(2048);
            entity.HasOne(publicacion => publicacion.Autor)
                .WithMany(usuario => usuario.PublicacionesForo)
                .HasForeignKey(publicacion => publicacion.AutorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlanEntrenamiento>(entity =>
        {
            entity.HasKey(plan => plan.Id);
            entity.Property(plan => plan.Titulo).HasMaxLength(200).IsRequired();
            entity.HasOne(plan => plan.Usuario)
                .WithMany(usuario => usuario.PlanesEntrenamiento)
                .HasForeignKey(plan => plan.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(plan => plan.Dias)
                .WithOne(dia => dia.PlanEntrenamiento)
                .HasForeignKey(dia => dia.PlanEntrenamientoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(plan => new { plan.UsuarioId, plan.IsActivo });
        });

        modelBuilder.Entity<DiaEntrenamiento>(entity =>
        {
            entity.HasKey(dia => dia.Id);
            entity.Property(dia => dia.NombreDia).HasMaxLength(30).IsRequired();
            entity.Property(dia => dia.Enfoque).HasMaxLength(150).IsRequired();
            entity.HasIndex(dia => new { dia.PlanEntrenamientoId, dia.Orden }).IsUnique();
            entity.HasMany(dia => dia.Ejercicios)
                .WithOne(ejercicio => ejercicio.DiaEntrenamiento)
                .HasForeignKey(ejercicio => ejercicio.DiaEntrenamientoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EjercicioItem>(entity =>
        {
            entity.HasKey(ejercicio => ejercicio.Id);
            entity.Property(ejercicio => ejercicio.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(ejercicio => ejercicio.SeriesYRepeticiones).HasMaxLength(50).IsRequired();
            entity.Property(ejercicio => ejercicio.Notas).HasMaxLength(500);
            entity.HasIndex(ejercicio => new { ejercicio.DiaEntrenamientoId, ejercicio.Orden }).IsUnique();
        });

        modelBuilder.Entity<CajaTurno>(entity =>
        {
            entity.HasKey(caja => caja.Id);
            entity.Property(caja => caja.Turno).HasMaxLength(20).IsRequired();
            entity.Property(caja => caja.ResponsableTurno).HasMaxLength(250).IsRequired();
            entity.Property(caja => caja.ResponsableTurnoNormalizado).HasMaxLength(250).IsRequired();
            entity.Property(caja => caja.Estado).HasMaxLength(20).IsRequired();
            entity.Property(caja => caja.MontoInicial).HasPrecision(12, 2);
            entity.Property(caja => caja.MontoFinalEfectivoEsperado).HasPrecision(12, 2);
            entity.Property(caja => caja.MontoFinalEfectivoReal).HasPrecision(12, 2);
            entity.Property(caja => caja.Diferencia).HasPrecision(12, 2);
            entity.Property(caja => caja.TotalCobrosEfectivo).HasPrecision(12, 2);
            entity.Property(caja => caja.TotalCobrosDigitales).HasPrecision(12, 2);
            entity.Property(caja => caja.Observaciones).HasMaxLength(2000);
            entity.HasIndex(caja => new { caja.Fecha, caja.Turno })
                .IsUnique()
                .HasFilter("\"Estado\" = 'Abierta'");
            entity.HasOne(caja => caja.UsuarioApertura).WithMany()
                .HasForeignKey(caja => caja.UsuarioAperturaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(caja => caja.UsuarioCierre).WithMany()
                .HasForeignKey(caja => caja.UsuarioCierreId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(producto => producto.Id);
            entity.Property(producto => producto.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(producto => producto.Precio).HasPrecision(12, 2);
            entity.Property(producto => producto.Codigo).HasMaxLength(100);
            entity.HasIndex(producto => producto.Codigo).IsUnique();
        });

        modelBuilder.Entity<VentaProducto>(entity =>
        {
            entity.HasKey(venta => venta.Id);
            entity.Property(venta => venta.MontoTotal).HasPrecision(12, 2);
            entity.Property(venta => venta.MetodoPago).HasMaxLength(30).IsRequired();
            entity.HasOne(venta => venta.Usuario).WithMany()
                .HasForeignKey(venta => venta.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(venta => venta.CajaTurno).WithMany(caja => caja.Ventas)
                .HasForeignKey(venta => venta.CajaTurnoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(venta => venta.Items).WithOne(item => item.VentaProducto)
                .HasForeignKey(item => item.VentaProductoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VentaProductoItem>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.PrecioUnitario).HasPrecision(12, 2);
            entity.HasOne(item => item.Producto).WithMany()
                .HasForeignKey(item => item.ProductoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaseDiario>(entity =>
        {
            entity.HasKey(pase => pase.Id);
            entity.Property(pase => pase.NombreCompleto).HasMaxLength(200).IsRequired();
            entity.Property(pase => pase.Dni).HasMaxLength(20).IsRequired();
            entity.Property(pase => pase.Monto).HasPrecision(12, 2);
            entity.Property(pase => pase.MetodoPago).HasMaxLength(30).IsRequired();
            entity.HasIndex(pase => new { pase.Dni, pase.Fecha });
            entity.HasOne(pase => pase.RegistradoPorUsuario).WithMany()
                .HasForeignKey(pase => pase.RegistradoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(pase => pase.CajaTurno).WithMany(caja => caja.PasesDiarios)
                .HasForeignKey(pase => pase.CajaTurnoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Usuario>().HasData(RutinaSeedData.Usuarios);
        modelBuilder.Entity<PlanEntrenamiento>().HasData(RutinaSeedData.Planes);
        modelBuilder.Entity<DiaEntrenamiento>().HasData(RutinaSeedData.Dias);
        modelBuilder.Entity<EjercicioItem>().HasData(RutinaSeedData.Ejercicios);
        modelBuilder.Entity<PlanMembresia>().HasData(MembresiaSeedData.Planes);
    }
}