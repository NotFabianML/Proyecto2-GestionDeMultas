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
                    Descripcion = i.Descripcion
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
                    Descripcion = i.Descripcion
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

        // Activar Infracción
        [HttpPost("{id}/activar")]
        public async Task<IActionResult> ActivarInfraccion(Guid id)
        {
            var infraccion = await _context.Infracciones.FindAsync(id);
            if (infraccion == null)
            {
                return NotFound("Infracción no encontrada.");
            }

            infraccion.Estado = true; // Suponiendo que hay un campo de estado booleano para activación
            await _context.SaveChangesAsync();

            return Ok("Infracción activada.");
        }

        // Desactivar Infracción
        [HttpPost("{id}/desactivar")]
        public async Task<IActionResult> DesactivarInfraccion(Guid id)
        {
            var infraccion = await _context.Infracciones.FindAsync(id);
            if (infraccion == null)
            {
                return NotFound("Infracción no encontrada.");
            }

            infraccion.Estado = false; // Suponiendo que hay un campo de estado booleano para desactivación
            await _context.SaveChangesAsync();

            return Ok("Infracción desactivada.");
        }

        private bool InfraccionExists(Guid id)
        {
            return _context.Infracciones.Any(e => e.IdInfraccion == id);
        }
    }
}
