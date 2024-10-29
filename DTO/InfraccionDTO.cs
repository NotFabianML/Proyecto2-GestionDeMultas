namespace DTO
{
    public class InfraccionDTO
    {
        public Guid IdInfraccion { get; set; }
        public string Articulo { get; set; }
        public string Categoria { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; }
    }
}
