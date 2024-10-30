﻿using DataAccess.EF.Models.Enums;
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

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Required]
        public string Motivo { get; set; }

        public EstadoDisputa Estado { get; set; } = EstadoDisputa.EnDisputa;

        public string? Resolucion { get; set; }

        public DateTime? FechaResolucion { get; set; }
    }
}
