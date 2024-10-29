using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.EF.Models
{
    public class Rol
    {
        [Key]
        public Guid IdRol { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(45)]
        public string NombreRol { get; set; }

        public string Descripcion { get; set; }

        public ICollection<UsuarioXRol> UsuarioRoles { get; set; }
        public ICollection<RolXPermiso> RolPermisos { get; set; }
    }
}
