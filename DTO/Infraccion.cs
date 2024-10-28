using System;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class Infraccion
    {
        [Key]
        public Guid IdInfraccion { get; set; } = Guid.NewGuid();

        [Required]
        public string Articulo { get; set; }

        [Required]
        public string Categoria { get; set; }

        [Required]
        public decimal Monto { get; set; }

        public string Descripcion { get; set; }

        // Colección para la relación muchos a muchos con Multa
        public ICollection<MultaXInfraccion> MultaInfracciones { get; set; }
    }
}
