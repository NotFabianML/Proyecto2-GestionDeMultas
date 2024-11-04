using DTO;

public class ConsultaPublicaDTO
{
    public Guid IdMulta { get; set; }
    public string NumeroPlaca { get; set; }
    public string CedulaOficial { get; set; }
    public DateTime FechaHora { get; set; }
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public string? Comentario { get; set; }
    public string? FotoPlaca { get; set; }
    public int Estado { get; set; }
    public decimal? MontoTotal { get; set; }
    public List<InfraccionDTO> Infracciones { get; set; } = new List<InfraccionDTO>();
}
