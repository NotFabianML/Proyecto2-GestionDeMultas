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

        [Required]
        [MaxLength(6)]
        public string NumeroPlaca { get; set; } // Nueva propiedad para almacenar la placa directamente

        [Required]
        [MaxLength(9)]
        public string CedulaInfractor { get; set; } // Nueva propiedad para almacenar la cédula del infractor

        public Guid UsuarioIdOficial { get; set; }
        public Usuario Oficial { get; set; }

        public DateTime FechaHora { get; set; }

        [Required]
        [Column(TypeName = "decimal(9, 4)")]
        public decimal Latitud { get; set; }

        [Required]
        [Column(TypeName = "decimal(9, 4)")]
        public decimal Longitud { get; set; }

        [MaxLength(255)]
        public string? Comentario { get; set; }

        [MaxLength(255)]
        public string? FotoPlaca { get; set; }

        public EstadoMulta Estado { get; set; } = EstadoMulta.Pendiente;

        public ICollection<MultaXInfraccion> MultaInfracciones { get; set; }
    }
}
