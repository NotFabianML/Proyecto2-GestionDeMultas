using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.EF;
using DataAccess.EF.Models;

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
        public async Task<ActionResult<IEnumerable<Infraccion>>> GetInfracciones()
        {
            return await _context.Infracciones
                .FromSqlRaw("EXEC sp_obtenerInfracciones")
                .ToListAsync();
        }

        // GET: api/Infracciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Infraccion>> GetInfraccion(Guid id)
        {
            var infraccion = await _context.Infracciones
                .FromSqlRaw("EXEC sp_obtenerInfraccionPorId @idInfraccion = {0}", id)
                .FirstOrDefaultAsync();

            if (infraccion == null)
            {
                return NotFound();
            }

            return infraccion;
        }

        // PUT: api/Infracciones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInfraccion(Guid id, Infraccion infraccion)
        {
            if (id != infraccion.IdInfraccion)
            {
                return BadRequest("El ID proporcionado no coincide con la infracción.");
            }

            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_actualizarInfraccion @idInfraccion = {0}, @articulo = {1}, @categoria = {2}, @monto = {3}, @descripcion = {4}",
                    id, infraccion.Articulo, infraccion.Categoria, infraccion.Monto, infraccion.Descripcion);
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
        public async Task<ActionResult<Infraccion>> PostInfraccion(Infraccion infraccion)
        {
            // Verificar si el artículo ya existe
            if (_context.Infracciones.Any(i => i.Articulo == infraccion.Articulo))
            {
                return Conflict("El artículo ya está registrado.");
            }

            infraccion.IdInfraccion = Guid.NewGuid();

            await _context.Database.ExecuteSqlRawAsync("EXEC sp_insertarInfraccion @idInfraccion = {0}, @articulo = {1}, @categoria = {2}, @monto = {3}, @descripcion = {4}",
                infraccion.IdInfraccion, infraccion.Articulo, infraccion.Categoria, infraccion.Monto, infraccion.Descripcion);

            return CreatedAtAction("GetInfraccion", new { id = infraccion.IdInfraccion }, infraccion);
        }

        // DELETE: api/Infracciones/5 (Eliminación lógica)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInfraccion(Guid id)
        {
            var infraccion = await _context.Infracciones.FindAsync(id);
            if (infraccion == null)
            {
                return NotFound();
            }

            await _context.Database.ExecuteSqlRawAsync("EXEC sp_eliminarInfraccion @idInfraccion = {0}", id);

            return NoContent();
        }

        private bool InfraccionExists(Guid id)
        {
            return _context.Infracciones.Any(e => e.IdInfraccion == id);
        }
    }
}
