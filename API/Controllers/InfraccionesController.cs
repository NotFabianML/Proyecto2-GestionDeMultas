using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.EF;
using DataAccess.EF.Models;
using DTO;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfraccionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InfraccionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Infracciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InfraccionDTO>>> GetInfracciones()
        {
            var infracciones = await _context.Infracciones
                .Select(i => new InfraccionDTO
                {
                    IdInfraccion = i.IdInfraccion,
                    Articulo = i.Articulo,
                    Titulo = i.Titulo,
                    Monto = i.Monto,
                    Descripcion = i.Descripcion,
                    Estado = i.Estado
                })
                .ToListAsync();

            return Ok(infracciones);
        }

        // GET: api/Infracciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InfraccionDTO>> GetInfraccion(Guid id)
        {
            var infraccion = await _context.Infracciones
                .Where(i => i.IdInfraccion == id)
                .Select(i => new InfraccionDTO
                {
                    IdInfraccion = i.IdInfraccion,
                    Articulo = i.Articulo,
                    Titulo = i.Titulo,
                    Monto = i.Monto,
                    Descripcion = i.Descripcion,
                    Estado = i.Estado
                })
                .FirstOrDefaultAsync();

            if (infraccion == null)
            {
                return NotFound("Infracción no encontrada.");
            }

            return Ok(infraccion);
        }

        // PUT: api/Infracciones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInfraccion(Guid id, InfraccionDTO infraccionDTO)
        {
            if (id != infraccionDTO.IdInfraccion)
            {
                return BadRequest("El ID proporcionado no coincide con la infracción.");
            }

            var infraccion = await _context.Infracciones.FindAsync(id);
            if (infraccion == null)
            {
                return NotFound("Infracción no encontrada.");
            }

            infraccion.Articulo = infraccionDTO.Articulo;
            infraccion.Titulo = infraccionDTO.Titulo;
            infraccion.Monto = infraccionDTO.Monto;
            infraccion.Descripcion = infraccionDTO.Descripcion;

            _context.Entry(infraccion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InfraccionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Infracciones
        [HttpPost]
        public async Task<ActionResult<InfraccionDTO>> PostInfraccion(InfraccionDTO infraccionDTO)
        {
            // Verificar si el artículo ya existe
            if (_context.Infracciones.Any(i => i.Articulo == infraccionDTO.Articulo))
            {
                return Conflict("El artículo ya está registrado.");
            }

            var infraccion = new Infraccion
            {
                IdInfraccion = Guid.NewGuid(),
                Articulo = infraccionDTO.Articulo,
                Titulo = infraccionDTO.Titulo,
                Monto = infraccionDTO.Monto,
                Descripcion = infraccionDTO.Descripcion
            };

            _context.Infracciones.Add(infraccion);
            await _context.SaveChangesAsync();

            infraccionDTO.IdInfraccion = infraccion.IdInfraccion;

            return CreatedAtAction("GetInfraccion", new { id = infraccion.IdInfraccion }, infraccionDTO);
        }

        // Asignar infracción a una multa
        [HttpPost("{multaId}/asignar-infracciones/{id}")]
        public async Task<IActionResult> AsignarInfraccionAMulta(Guid multaId, Guid id)
        {
            if (!MultaExists(multaId) || !_context.Infracciones.Any(i => i.IdInfraccion == id))
            {
                return NotFound("Multa o infracción no encontrada.");
            }

            var result = await _context.Database.ExecuteSqlRawAsync("EXEC sp_AsignarInfraccion @Multa_idMulta = {0}, @Permiso_idPermiso = {1}", multaId, id);
            if (result == 0)
            {
                return BadRequest("Error al asignar la infraccion a la multa.");
            }

            return NoContent();
        }

        // Cambiar estado
        [HttpPost("{id}/cambiar-estado/{estado}")]
        public async Task<IActionResult> CambiarEstado(Guid id, int estado)
        {
            var infraccion = await _context.Infracciones.FindAsync(id);
            if (infraccion == null)
            {
                return NotFound("Infracción no encontrada.");
            }

            infraccion.Estado = Convert.ToBoolean(estado);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InfraccionExists(Guid id)
        {
            return _context.Infracciones.Any(e => e.IdInfraccion == id);
        }

        private bool MultaExists(Guid id)
        {
            return _context.Multas.Any(e => e.IdMulta == id);
        }

        // POST: api/Infracciones/Inicializar
        [HttpPost("Inicializar")]
        public async Task<ActionResult> InicializarInfracciones()
        {
            var infraccionesIniciales = new List<InfraccionDTO>
            {
                new InfraccionDTO { Articulo = "124", Titulo = "Exceso de Velocidad", Monto = 47000.00M, Descripcion = "Conducir superando el límite de velocidad establecido por la ley o señalización vigente en la vía." },
                new InfraccionDTO { Articulo = "125", Titulo = "Irrespetar Semáforo en Rojo", Monto = 189000.00M, Descripcion = "No respetar la señal de alto cuando el semáforo se encuentra en rojo." },
                new InfraccionDTO { Articulo = "126", Titulo = "Estacionamiento en Línea Amarilla", Monto = 61470.00M, Descripcion = "Aparcar el vehículo en zonas demarcadas con línea amarilla, indicando prohibición de estacionamiento." },
                new InfraccionDTO { Articulo = "127", Titulo = "Adelantamiento Indebido", Monto = 280000.00M, Descripcion = "Realizar maniobras de adelantamiento en lugares donde está prohibido o que representan riesgo para otros conductores." },
                new InfraccionDTO { Articulo = "128", Titulo = "Marchamo Vencido", Monto = 47000.00M, Descripcion = "Circular sin haber renovado el marchamo en la fecha establecida por la ley." },
                new InfraccionDTO { Articulo = "129", Titulo = "Revisión Técnica Vencida", Monto = 47000.00M, Descripcion = "Circular sin la revisión técnica vehicular obligatoria, vencida en la fecha indicada." },
                new InfraccionDTO { Articulo = "130", Titulo = "No Uso de Cinturón de Seguridad", Monto = 94000.00M, Descripcion = "Conducir o transitar en un vehículo sin utilizar el cinturón de seguridad, incumpliendo las normas de seguridad." },
                new InfraccionDTO { Articulo = "131", Titulo = "Conducción bajo Efectos del Alcohol", Monto = 280000.00M, Descripcion = "Conducir bajo los efectos del alcohol, poniendo en riesgo la seguridad vial." },
                new InfraccionDTO { Articulo = "132", Titulo = "Conducción Temeraria", Monto = 280000.00M, Descripcion = "Realizar maniobras que representan riesgo elevado para otros usuarios de la vía, como zigzaguear o exceder el límite de velocidad en zonas urbanas." },
                new InfraccionDTO { Articulo = "133", Titulo = "Conducir sin Licencia", Monto = 47000.00M, Descripcion = "Manejar un vehículo sin portar la licencia de conducir válida y vigente." }
            };

            foreach (var infraccionDTO in infraccionesIniciales)
            {
                // Verificar si el artículo ya existe
                if (_context.Infracciones.Any(i => i.Articulo == infraccionDTO.Articulo))
                {
                    continue; // Saltar esta infracción si ya está registrada
                }

                // Crear la entidad Infraccion
                var infraccion = new Infraccion
                {
                    IdInfraccion = Guid.NewGuid(),
                    Articulo = infraccionDTO.Articulo,
                    Titulo = infraccionDTO.Titulo,
                    Monto = infraccionDTO.Monto,
                    Descripcion = infraccionDTO.Descripcion,
                    Estado = true
                };

                _context.Infracciones.Add(infraccion);
            }

            await _context.SaveChangesAsync();

            return Ok("Infracciones iniciales agregadas exitosamente.");
        }

    }
}
