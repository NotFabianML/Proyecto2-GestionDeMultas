using System;

namespace DTO
{
    public class MultaDTO
    {
        public Guid IdMulta { get; set; }
        public Guid VehiculoId { get; set; }
        public Guid UsuarioIdOficial { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public string? Comentario { get; set; }
        public string? FotoPlaca { get; set; }

        public int Estado { get; set; } /// Representación numérica del enum EstadoMulta (1=Pendiente, 2=En Disputa, 3=Pagada)

        public decimal? MontoTotal { get; set; }
        public List<InfraccionDTO> Infracciones { get; set; } = new List<InfraccionDTO>();
    }
}
