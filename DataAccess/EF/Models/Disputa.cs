using DataAccess.EF.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.EF.Models
{
    public class Disputa
    {
        [Key]
        public Guid IdDisputa { get; set; } = Guid.NewGuid();

        public Guid MultaId { get; set; }
        public Multa Multa { get; set; }

        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public Guid? UsuarioIdJuez { get; set; }
        public Usuario Juez { get; set; }

        public DateTime FechaCreacion { get; set; }

        [Required]
        [MaxLength(255)]
        public string MotivoReclamo { get; set; } // Motivo del usuario para disputar la multa

        public EstadoDisputa Estado { get; set; } = EstadoDisputa.EnDisputa;

        [MaxLength(255)]
        public string? ResolucionJuez { get; set; } // Resolución del juez

        [MaxLength(255)]
        public string? DeclaracionOficial { get; set; } // Declaración del oficial

        public DateTime? FechaResolucion { get; set; }
    }
}
