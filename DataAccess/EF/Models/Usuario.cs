using DTO;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.EF.Models
{
    public class Usuario
    {
        [Key]
        public Guid IdUsuario { get; set; } = Guid.NewGuid();
        public string? UserId { get; set; } // Enlaza con AspNetUsers

        [ForeignKey("UserId")]
        public IdentityUser IdentityUser { get; set; }  // Navegación hacia IdentityUser

        [Required]
        [MaxLength(10)]
        public string Cedula { get; set; }

        [Required]
        [MaxLength(45)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(45)]
        public string Apellido1 { get; set; }

        [MaxLength(45)]
        public string? Apellido2 { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }
        
        [Required]
        public DateOnly FechaNacimiento { get; set; }

        [MaxLength(8)]
        public string Telefono { get; set; }

        [Required]
        [MaxLength(255)]
        public string ContrasennaHash { get; set; }

        [MaxLength(255)]
        public string? FotoPerfil { get; set; }

        public bool Estado { get; set; } = true;

        public bool DobleFactorActivo { get; set; } = false;

        [MaxLength(100)]
        public string? DobleFactorSecret { get; set; }

        public ICollection<UsuarioXRol> UsuarioRoles { get; set; }
        public ICollection<Vehiculo> Vehiculos { get; set; }
    }
}
