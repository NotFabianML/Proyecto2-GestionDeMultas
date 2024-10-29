using System;

namespace DataAccess.EF.Models
{
    public class UsuarioXRol
    {
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public Guid RolId { get; set; }
        public Rol Rol { get; set; }
    }
}
