using System;

namespace DTO
{
    public class UsuarioDTO
    {
        public Guid IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        public string Email { get; set; }
        public bool Estado { get; set; }
        public string ContrasennaHash { get; set; }
    }
}
