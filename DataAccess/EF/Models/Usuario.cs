using DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.EF.Models
{
    public class Usuario
    {
        [Key]
        public Guid IdUsuario { get; set; } = Guid.NewGuid();

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

        [MaxLength(8)]
        public string? Telefono { get; set; }

        [Required]
        [MaxLength(255)]
        public string ContrasennaHash { get; set; }

        [MaxLength(255)]
        public string? FotoPerfil { get; set; }

        public bool Estado { get; set; } = true;

        public bool DosFactorActivo { get; set; } = false;

        [MaxLength(100)]
        public string? DosFactorSecret { get; set; }

        public ICollection<UsuarioXRol> UsuarioRoles { get; set; }
        public ICollection<Vehiculo> Vehiculos { get; set; }
    }
}
