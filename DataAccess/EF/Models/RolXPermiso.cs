using System;

namespace DataAccess.EF.Models
{
    public class RolXPermiso
    {
        public Guid RolId { get; set; }
        public Rol Rol { get; set; }

        public Guid PermisoId { get; set; }
        public Permiso Permiso { get; set; }
    }
}
