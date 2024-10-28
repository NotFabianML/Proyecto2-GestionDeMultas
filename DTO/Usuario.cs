using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class Usuario
    {
        [Key]
        public Guid IdUsuario { get; set; } = Guid.NewGuid();

        [Required]
        public string Cedula { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Apellido1 { get; set; }

        public string Apellido2 { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Telefono { get; set; }

        [Required]
        public string ContrasennaHash { get; set; }

        public string FotoPerfil { get; set; }

        public bool Estado { get; set; } = true;

        public bool DosFactorActivo { get; set; } = false;

        public string DosFactorSecret { get; set; }

        // Relación con Rol
        public ICollection<UsuarioXRol> UsuarioRoles { get; set; }
    }
}
