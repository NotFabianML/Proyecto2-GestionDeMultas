using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DataAccess.EF.Models;

namespace DataAccess.EF
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        // Definir DbSets para cada entidad principal
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Infraccion> Infracciones { get; set; }
        public DbSet<Multa> Multas { get; set; }
        public DbSet<Disputa> Disputas { get; set; }

        // Definir DbSets para las tablas intermedias
        public DbSet<UsuarioXRol> UsuarioRoles { get; set; }
        public DbSet<RolXPermiso> RolPermisos { get; set; }
        public DbSet<MultaXInfraccion> MultaInfracciones { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de relación Usuario - IdentityUser
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.IdentityUser)
                .WithOne()
                .HasForeignKey<Usuario>(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de relación Usuario - UsuarioXRol - Rol
            modelBuilder.Entity<UsuarioXRol>()
                .HasKey(ur => new { ur.UsuarioId, ur.RolId });

            modelBuilder.Entity<UsuarioXRol>()
                .HasOne(ur => ur.Usuario)
                .WithMany(u => u.UsuarioRoles)
                .HasForeignKey(ur => ur.UsuarioId);

            modelBuilder.Entity<UsuarioXRol>()
                .HasOne(ur => ur.Rol)
                .WithMany(r => r.UsuarioRoles)
                .HasForeignKey(ur => ur.RolId);

            // Configuración de relación Rol - RolXPermiso - Permiso
            modelBuilder.Entity<RolXPermiso>()
                .HasKey(rp => new { rp.RolId, rp.PermisoId });

            modelBuilder.Entity<RolXPermiso>()
                .HasOne(rp => rp.Rol)
                .WithMany(r => r.RolPermisos)
                .HasForeignKey(rp => rp.RolId);

            modelBuilder.Entity<RolXPermiso>()
                .HasOne(rp => rp.Permiso)
                .WithMany(p => p.RolPermisos)
                .HasForeignKey(rp => rp.PermisoId);

            // Configuración de relación Multa - MultaXInfraccion - Infraccion
            modelBuilder.Entity<MultaXInfraccion>()
                .HasKey(mi => new { mi.MultaId, mi.InfraccionId });

            modelBuilder.Entity<MultaXInfraccion>()
                .HasOne(mi => mi.Multa)
                .WithMany(m => m.MultaInfracciones)
                .HasForeignKey(mi => mi.MultaId);

            modelBuilder.Entity<MultaXInfraccion>()
                .HasOne(mi => mi.Infraccion)
                .WithMany(i => i.MultaInfracciones)
                .HasForeignKey(mi => mi.InfraccionId);

            // Configuración para la relación Multa y Vehiculo con DeleteBehavior.NoAction
            modelBuilder.Entity<Multa>()
                .HasOne(m => m.Vehiculo)
                .WithMany()
                .HasForeignKey(m => m.VehiculoId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuración para la relación Multa y Usuario (Oficial) con DeleteBehavior.NoAction
            modelBuilder.Entity<Multa>()
                .HasOne(m => m.Oficial)
                .WithMany()
                .HasForeignKey(m => m.UsuarioIdOficial)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuración de relación Disputa - Usuario (Usuario común)
            modelBuilder.Entity<Disputa>()
                .HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de relación Disputa - Usuario (Juez)
            modelBuilder.Entity<Disputa>()
                .HasOne(d => d.Juez)
                .WithMany()
                .HasForeignKey(d => d.UsuarioIdJuez)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de enums en Multa y Disputa
            modelBuilder.Entity<Multa>()
                .Property(m => m.Estado)
                .HasConversion<int>();

            modelBuilder.Entity<Disputa>()
                .Property(d => d.Estado)
                .HasConversion<int>();
        }
    }
}
