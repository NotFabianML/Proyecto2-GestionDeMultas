using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.EF.Models
{
    public class MultaXInfraccion
    {
        [Key]
        public Guid MultaId { get; set; }
        public Multa Multa { get; set; }

        [Key]
        public Guid InfraccionId { get; set; }
        public Infraccion Infraccion { get; set; }
    }
}
