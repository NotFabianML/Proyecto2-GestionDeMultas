using DataAccess.EF.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Column(TypeName = "decimal(9, 2)")]
        public decimal Latitud { get; set; }

        [Required]
        [Column(TypeName = "decimal(9, 2)")]
        public decimal Longitud { get; set; }

        [MaxLength(255)]
        public string? FotoUrl { get; set; }

        public EstadoMulta Estado { get; set; } = EstadoMulta.Pendiente;

        public ICollection<MultaXInfraccion> MultaInfracciones { get; set; }
    }
}
