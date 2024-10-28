using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class Permiso
    {
        [Key]
        public Guid IdPermiso { get; set; } = Guid.NewGuid();

        [Required]
        public string NombrePermiso { get; set; }

        [Required]
        public string Descripcion { get; set; }

        public bool Estado { get; set; } = true;

        // Relación con Rol
        public ICollection<RolXPermiso> RolPermisos { get; set; }
    }
}
