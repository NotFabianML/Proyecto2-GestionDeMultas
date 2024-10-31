using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.EF.Models
{
    public class Vehiculo
    {
        [Key]
        public Guid IdVehiculo { get; set; } = Guid.NewGuid();

        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        [Required]
        [MaxLength(6)]
        public string NumeroPlaca { get; set; }

        [MaxLength(255)]
        public string? FotoVehiculo { get; set; }

        [MaxLength(50)]
        public string? Marca { get; set; }

        public int? Anno { get; set; }
    }
}
