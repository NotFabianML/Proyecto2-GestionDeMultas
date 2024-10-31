namespace DTO
{
    public class VehiculoDTO
    {
        public Guid IdVehiculo { get; set; }
        public Guid UsuarioId { get; set; }
        public string NumeroPlaca { get; set; }
        public string? FotoVehiculo { get; set; }
        public string? Marca { get; set; }
        public int? Anno { get; set; }
    }
}
