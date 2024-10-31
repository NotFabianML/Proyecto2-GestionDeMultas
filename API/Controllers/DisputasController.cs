using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.EF;
using DataAccess.EF.Models;
using DataAccess.EF.Models.Enums;
using DTO;

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
        public async Task<ActionResult<IEnumerable<DisputaDTO>>> GetDisputas()
        {
            var disputas = await _context.Disputas
                .Select(d => new DisputaDTO
                {
                    IdDisputa = d.IdDisputa,
                    MultaId = d.MultaId,
                    UsuarioId = d.UsuarioId,
                    JuezId = d.UsuarioIdJuez,
                    FechaCreacion = d.FechaCreacion,
                    MotivoReclamo = d.MotivoReclamo,
                    Estado = (int)d.Estado,
                    ResolucionJuez = d.ResolucionJuez,
                    DeclaracionOficial = d.DeclaracionOficial,
                    FechaResolucion = d.FechaResolucion
                })
                .ToListAsync();

            return Ok(disputas);
        }

        // GET: api/Disputas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DisputaDTO>> GetDisputa(Guid id)
        {
            var disputa = await _context.Disputas
                .Where(d => d.IdDisputa == id)
                .Select(d => new DisputaDTO
                {
                    IdDisputa = d.IdDisputa,
                    MultaId = d.MultaId,
                    UsuarioId = d.UsuarioId,
                    JuezId = d.UsuarioIdJuez,
                    FechaCreacion = d.FechaCreacion,
                    MotivoReclamo = d.MotivoReclamo,
                    Estado = (int)d.Estado,
                    ResolucionJuez = d.ResolucionJuez,
                    DeclaracionOficial = d.DeclaracionOficial,
                    FechaResolucion = d.FechaResolucion
                })
                .FirstOrDefaultAsync();

            if (disputa == null)
            {
                return NotFound("Disputa no encontrada.");
            }

            return Ok(disputa);
        }

        // GET: api/Disputas/usuario/5 - Obtener disputas por usuario
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<DisputaDTO>>> GetDisputasPorUsuario(Guid usuarioId)
        {
            var disputas = await _context.Disputas
                .FromSqlRaw("EXEC sp_obtenerDisputasPorUsuario @Usuario_idUsuario = {0}", usuarioId)
                .Select(d => new DisputaDTO
                {
                    IdDisputa = d.IdDisputa,
                    MultaId = d.MultaId,
                    UsuarioId = d.UsuarioId,
                    JuezId = d.UsuarioIdJuez,
                    FechaCreacion = d.FechaCreacion,
                    MotivoReclamo = d.MotivoReclamo,
                    Estado = (int)d.Estado,
                    ResolucionJuez = d.ResolucionJuez,
                    DeclaracionOficial = d.DeclaracionOficial,
                    FechaResolucion = d.FechaResolucion
                })
                .ToListAsync();

            if (!disputas.Any())
            {
                return NotFound("No se encontraron disputas para el usuario especificado.");
            }

            return Ok(disputas);
        }

        // PUT: api/Disputas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDisputa(Guid id, DisputaDTO disputaDTO)
        {
            if (id != disputaDTO.IdDisputa)
            {
                return BadRequest("El ID de la disputa no coincide.");
            }

            var existingDisputa = await _context.Disputas.FindAsync(id);
            if (existingDisputa == null)
            {
                return NotFound("Disputa no encontrada para actualización.");
            }

            existingDisputa.MotivoReclamo = disputaDTO.MotivoReclamo;
            existingDisputa.UsuarioIdJuez = disputaDTO.JuezId;
            existingDisputa.Estado = (EstadoDisputa)disputaDTO.Estado;
            existingDisputa.ResolucionJuez = disputaDTO.ResolucionJuez;
            existingDisputa.DeclaracionOficial = disputaDTO.DeclaracionOficial;
            existingDisputa.FechaResolucion = disputaDTO.FechaResolucion;

            try
            {
                _context.Entry(existingDisputa).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DisputaExists(id))
                {
                    return NotFound("Disputa no encontrada para actualización.");
                }
                throw;
            }

            return NoContent();
        }

        // POST: api/Disputas
        [HttpPost]
        public async Task<ActionResult<DisputaDTO>> PostDisputa(DisputaDTO disputaDTO)
        {
            if (!await _context.Multas.AnyAsync(m => m.IdMulta == disputaDTO.MultaId))
            {
                return BadRequest("La multa especificada no existe.");
            }

            var disputa = new Disputa
            {
                IdDisputa = Guid.NewGuid(),
                MultaId = disputaDTO.MultaId,
                UsuarioId = disputaDTO.UsuarioId,
                UsuarioIdJuez = disputaDTO.JuezId,
                FechaCreacion = DateTime.UtcNow,
                MotivoReclamo = disputaDTO.MotivoReclamo,
                Estado = (EstadoDisputa)disputaDTO.Estado,
                ResolucionJuez = disputaDTO.ResolucionJuez,
                DeclaracionOficial = disputaDTO.DeclaracionOficial,
                FechaResolucion = disputaDTO.FechaResolucion
            };

            _context.Disputas.Add(disputa);
            await _context.SaveChangesAsync();

            disputaDTO.IdDisputa = disputa.IdDisputa;
            return CreatedAtAction(nameof(GetDisputa), new { id = disputa.IdDisputa }, disputaDTO);
        }

        // DELETE: api/Disputas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDisputa(Guid id)
        {
            var disputa = await _context.Disputas.FindAsync(id);
            if (disputa == null)
            {
                return NotFound("Disputa no encontrada para eliminación.");
            }

            _context.Disputas.Remove(disputa);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Cambiar estado a "En Disputa"
        [HttpPost("{id}/en-disputa")]
        public async Task<IActionResult> CambiarEstadoEnDisputa(Guid id)
        {
            var disputa = await _context.Multas.FindAsync(id);
            if (disputa == null)
            {
                return NotFound("Multa no encontrada.");
            }

            disputa.Estado = EstadoMulta.EnDisputa;
            _context.Entry(disputa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Estado de la multa cambiado a En Disputa.");
        }

        // Cambiar estado a "Pagada"
        [HttpPost("{id}/pagada")]
        public async Task<IActionResult> CambiarEstadoAceptada(Guid id)
        {
            var disputa = await _context.Multas.FindAsync(id);
            if (disputa == null)
            {
                return NotFound("Multa no encontrada.");
            }

            disputa.Estado = EstadoMulta.Pagada;
            _context.Entry(disputa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Estado de la multa cambiado a En Disputa.");
        }

        // Cambiar estado a "Rechazada"
        [HttpPost("{id}/rechazada")]
        public async Task<IActionResult> CambiarEstadoRechazada(Guid id)
        {
            var disputa = await _context.Multas.FindAsync(id);
            if (disputa == null)
            {
                return NotFound("Multa no encontrada.");
            }

            disputa.Estado = EstadoMulta.EnDisputa;
            _context.Entry(disputa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Estado de la multa cambiado a En Disputa.");
        }

        private bool DisputaExists(Guid id)
        {
            return _context.Disputas.Any(e => e.IdDisputa == id);
        }
    }
}
