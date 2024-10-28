using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class Rol
    {
        [Key]
        public Guid IdRol { get; set; } = Guid.NewGuid();

        [Required]
        public string NombreRol { get; set; }

        public string Descripcion { get; set; }

        // Relación con Usuario y Permiso
        public ICollection<UsuarioXRol> UsuarioRoles { get; set; }
        public ICollection<RolXPermiso> RolPermisos { get; set; }
    }
}
