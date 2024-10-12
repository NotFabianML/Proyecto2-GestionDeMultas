using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class Tickets
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }
        [Required]
        public DateTime Fecha { get; set; }
        [Required] 
        public string Comment { get; set;}
        [Required] 
        public double Latitude { get; set;}
        [Required]
        public double Longitude { get; set; }

    }
}
