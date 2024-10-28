using System;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class Vehiculo
    {
        [Key]
        public Guid IdVehiculo { get; set; } = Guid.NewGuid();

        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        [Required]
        public string NumeroPlaca { get; set; }

        public string FotoVehiculo { get; set; }

        public string Marca { get; set; }

        public int Anno { get; set; }
    }
}
