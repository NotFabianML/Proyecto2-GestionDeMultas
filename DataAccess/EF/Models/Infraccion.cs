using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.EF.Models
{
    public class Infraccion
    {
        [Key]
        public Guid IdInfraccion { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(20)]
        public string Articulo { get; set; }

        [Required]
        [MaxLength(45)]
        public string Categoria { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Monto { get; set; }

        public string Descripcion { get; set; }

        public ICollection<MultaXInfraccion> MultaInfracciones { get; set; }
    }
}
