using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.EF.Models
{
    public class UsuarioXRol
    {
        [Key]
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        [Key]
        public Guid RolId { get; set; }
        public Rol Rol { get; set; }
    }
}
