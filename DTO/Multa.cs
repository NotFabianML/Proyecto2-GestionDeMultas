using System;
using System.ComponentModel.DataAnnotations;
using DTO.Enums;

namespace DTO
{
    public class Multa
    {
        [Key]
        public Guid IdMulta { get; set; } = Guid.NewGuid();

        public Guid VehiculoId { get; set; }
        public Vehiculo Vehiculo { get; set; }

        public Guid UsuarioIdOficial { get; set; }
        public Usuario Oficial { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        public string Ubicacion { get; set; }

        public string FotoUrl { get; set; }

        public EstadoMulta Estado { get; set; } = EstadoMulta.Pendiente;

        // Colección para la relación muchos a muchos con Infraccion
        public ICollection<MultaXInfraccion> MultaInfracciones { get; set; }
    }
}
