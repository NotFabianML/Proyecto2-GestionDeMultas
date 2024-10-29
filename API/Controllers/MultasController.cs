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
    public class MultasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MultasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Multas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Multa>>> GetMultas()
        {
            return await _context.Multas
                .FromSqlRaw("EXEC sp_obtenerMultas")
                .ToListAsync();
        }

        // GET: api/Multas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Multa>> GetMulta(Guid id)
        {
            var multa = await _context.Multas
                .FromSqlRaw("EXEC sp_obtenerMultaPorId @idMulta = {0}", id)
                .FirstOrDefaultAsync();

            if (multa == null)
            {
                return NotFound("Multa no encontrada.");
            }

            return multa;
        }

        // PUT: api/Multas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMulta(Guid id, Multa multa)
        {
            if (id != multa.IdMulta)
            {
                return BadRequest("El ID de la multa no coincide.");
            }

            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_actualizarMulta @idMulta = {0}, @vehiculoId = {1}, @usuarioIdOficial = {2}, @fechaHora = {3}, @ubicacion = {4}, @fotoUrl = {5}, @estado = {6}",
                    id, multa.VehiculoId, multa.UsuarioIdOficial, multa.FechaHora, multa.Ubicacion, multa.FotoUrl, (int)multa.Estado);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MultaExists(id))
                {
                    return NotFound("Multa no encontrada para actualización.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Multas
        [HttpPost]
        public async Task<ActionResult<Multa>> PostMulta(Multa multa)
        {
            if (!await VehiculoAndOficialExist(multa.VehiculoId, multa.UsuarioIdOficial))
            {
                return BadRequest("Vehículo o Oficial no válidos.");
            }

            multa.IdMulta = Guid.NewGuid();

            await _context.Database.ExecuteSqlRawAsync("EXEC sp_insertarMulta @idMulta = {0}, @vehiculoId = {1}, @usuarioIdOficial = {2}, @fechaHora = {3}, @ubicacion = {4}, @fotoUrl = {5}, @estado = {6}",
                multa.IdMulta, multa.VehiculoId, multa.UsuarioIdOficial, multa.FechaHora, multa.Ubicacion, multa.FotoUrl, (int)multa.Estado);

            return CreatedAtAction(nameof(GetMulta), new { id = multa.IdMulta }, multa);
        }

        // DELETE: api/Multas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMulta(Guid id)
        {
            var multa = await _context.Multas.FindAsync(id);
            if (multa == null)
            {
                return NotFound("Multa no encontrada para eliminación.");
            }

            await _context.Database.ExecuteSqlRawAsync("EXEC sp_actualizarEstadoMulta @idMulta = {0}, @estado = {1}", id, (int)EstadoMulta.Pagada);

            return NoContent();
        }

        private bool MultaExists(Guid id)
        {
            return _context.Multas.Any(e => e.IdMulta == id);
        }

        private async Task<bool> VehiculoAndOficialExist(Guid vehiculoId, Guid oficialId)
        {
            return await _context.Vehiculos.AnyAsync(v => v.IdVehiculo == vehiculoId) &&
                   await _context.Usuarios.AnyAsync(u => u.IdUsuario == oficialId);
        }
    }
}
