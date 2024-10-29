using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.EF.Models
{
    public class Permiso
    {
        [Key]
        public Guid IdPermiso { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string NombrePermiso { get; set; }

        [Required]
        public string Descripcion { get; set; }

        public bool Estado { get; set; } = true;

        public ICollection<RolXPermiso> RolPermisos { get; set; }
    }
}
