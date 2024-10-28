using System;

namespace DTO
{
    public class MultaXInfraccion
    {
        public Guid MultaId { get; set; }
        public Multa Multa { get; set; }

        public Guid InfraccionId { get; set; }
        public Infraccion Infraccion { get; set; }
    }
}
