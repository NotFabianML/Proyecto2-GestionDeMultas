using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.EF.Models
{
    public class RolXPermiso
    {
        [Key]
        public Guid RolId { get; set; }
        public Rol Rol { get; set; }

        [Key]
        public Guid PermisoId { get; set; }
        public Permiso Permiso { get; set; }
    }
}
