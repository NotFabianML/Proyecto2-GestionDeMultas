using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.EF;
using DataAccess.EF.Models;
using DataAccess.EF.Models.Enums;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisputasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DisputasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Disputas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Disputa>>> GetDisputas()
        {
            try
            {
                var disputas = await _context.Disputas
                    .FromSqlRaw("EXEC sp_obtenerDisputas")
                    .ToListAsync();

                return Ok(disputas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener disputas: {ex.Message}");
            }
        }

        // GET: api/Disputas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Disputa>> GetDisputa(Guid id)
        {
            try
            {
                var disputa = await _context.Disputas
                    .FromSqlRaw("EXEC sp_obtenerDisputaPorId @idDisputa = {0}", id)
                    .FirstOrDefaultAsync();

                if (disputa == null)
                {
                    return NotFound("Disputa no encontrada.");
                }

                return Ok(disputa);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener disputa: {ex.Message}");
            }
        }

        // PUT: api/Disputas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDisputa(Guid id, Disputa disputa)
        {
            if (id != disputa.IdDisputa)
            {
                return BadRequest("El ID de la disputa no coincide.");
            }

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_actualizarDisputa @idDisputa = {0}, @multaId = {1}, @usuarioId = {2}, @usuarioIdJuez = {3}, @motivo = {4}, @estado = {5}, @resolucion = {6}, @fechaResolucion = {7}",
                    id, disputa.MultaId, disputa.UsuarioId, disputa.UsuarioIdJuez, disputa.Motivo, (int)disputa.Estado, disputa.Resolucion, disputa.FechaResolucion);

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DisputaExists(id))
                {
                    return NotFound("Disputa no encontrada para actualización.");
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar disputa: {ex.Message}");
            }
        }

        // POST: api/Disputas
        [HttpPost]
        public async Task<ActionResult<Disputa>> PostDisputa(Disputa disputa)
        {
            try
            {
                if (!await _context.Multas.AnyAsync(m => m.IdMulta == disputa.MultaId))
                {
                    return BadRequest("La multa especificada no existe.");
                }

                disputa.IdDisputa = Guid.NewGuid();

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_insertarDisputa @idDisputa = {0}, @multaId = {1}, @usuarioId = {2}, @usuarioIdJuez = {3}, @fechaCreacion = {4}, @motivo = {5}, @estado = {6}, @resolucion = {7}, @fechaResolucion = {8}",
                    disputa.IdDisputa, disputa.MultaId, disputa.UsuarioId, disputa.UsuarioIdJuez, DateTime.UtcNow, disputa.Motivo, (int)disputa.Estado, disputa.Resolucion, disputa.FechaResolucion);

                return CreatedAtAction(nameof(GetDisputa), new { id = disputa.IdDisputa }, disputa);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al crear disputa: {ex.Message}");
            }
        }

        // DELETE: api/Disputas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDisputa(Guid id)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_eliminarDisputa @idDisputa = {0}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar disputa: {ex.Message}");
            }
        }

        private bool DisputaExists(Guid id)
        {
            return _context.Disputas.Any(e => e.IdDisputa == id);
        }
    }
}
