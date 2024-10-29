using System;

namespace DTO
{
    public class DisputaDTO
    {
        public Guid IdDisputa { get; set; }
        public Guid MultaId { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid? JuezId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Motivo { get; set; }
        public int Estado { get; set; } // Representación del enum EstadoDisputa como int
        public string Resolucion { get; set; }
        public DateTime? FechaResolucion { get; set; }
    }
}
