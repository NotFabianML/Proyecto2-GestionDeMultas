using DataAccess.EF.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.EF.Models
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

        [Required]
        public string Ubicacion { get; set; }

        [MaxLength(255)]
        public string FotoUrl { get; set; }

        public EstadoMulta Estado { get; set; } = EstadoMulta.Pendiente;

        public ICollection<MultaXInfraccion> MultaInfracciones { get; set; }
    }
}
