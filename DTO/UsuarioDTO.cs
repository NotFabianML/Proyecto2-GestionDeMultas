using System;

namespace DTO
{
    public class UsuarioDTO
    {
        public Guid IdUsuario { get; set; }
        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Apellido1 { get; set; }
        public string? Apellido2 { get; set; }
        public string Email { get; set; }
        public string? ContrasennaHash { get; set; }
        public string FechaNacimiento { get; set; }
        public string Telefono { get; set; }
        public string? FotoPerfil { get; set; }
        public bool Estado { get; set; }

        public List<string>? Roles { get; set; } // Nueva propiedad para almacenar roles
    }
}
