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
using Microsoft.AspNetCore.Authorization;

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
        public async Task<ActionResult<IEnumerable<MultaDTO>>> GetMultas()
        {
            var multas = await _context.Multas
                .Select(m => new MultaDTO
                {
                    IdMulta = m.IdMulta,
                    VehiculoId = m.VehiculoId,
                    UsuarioIdOficial = m.UsuarioIdOficial,
                    FechaHora = m.FechaHora,
                    Latitud = m.Latitud.ToString(),
                    Longitud = m.Longitud.ToString(),
                    Comentario = m.Comentario,
                    FotoPlaca = m.FotoPlaca,
                    Estado = (int)m.Estado
                })
                .ToListAsync();

            return Ok(multas);
        }

        // GET: api/Multas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MultaDTO>> GetMulta(Guid id)
        {
            var multa = await _context.Multas
                .Where(m => m.IdMulta == id)
                .Select(m => new MultaDTO
                {
                    IdMulta = m.IdMulta,
                    VehiculoId = m.VehiculoId,
                    UsuarioIdOficial = m.UsuarioIdOficial,
                    FechaHora = m.FechaHora,
                    Latitud = m.Latitud.ToString(),
                    Longitud = m.Longitud.ToString(),
                    Comentario = m.Comentario,
                    FotoPlaca = m.FotoPlaca,
                    Estado = (int)m.Estado
                })
                .FirstOrDefaultAsync();

            if (multa == null)
            {
                return NotFound("Multa no encontrada.");
            }

            return Ok(multa);
        }

        // PUT: api/Multas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMulta(Guid id, MultaDTO multaDTO)
        {
            if (id != multaDTO.IdMulta)
            {
                return BadRequest("El ID de la multa no coincide.");
            }

            var multa = await _context.Multas.FindAsync(id);
            if (multa == null)
            {
                return NotFound("Multa no encontrada para actualización.");
            }

            multa.VehiculoId = multaDTO.VehiculoId;
            multa.UsuarioIdOficial = multaDTO.UsuarioIdOficial;
            multa.FechaHora = multaDTO.FechaHora;
            multa.Latitud = decimal.Parse(multaDTO.Latitud);
            multa.Longitud = decimal.Parse(multaDTO.Longitud);
            multa.Comentario = multaDTO.Comentario;
            multa.FotoPlaca = multaDTO.FotoPlaca;
            multa.Estado = (EstadoMulta)multaDTO.Estado;

            _context.Entry(multa).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
        public async Task<ActionResult<MultaDTO>> PostMulta(MultaDTO multaDTO)
        {
            if (!await VehiculoAndOficialExist(multaDTO.VehiculoId, multaDTO.UsuarioIdOficial))
            {
                return BadRequest("Vehículo o Oficial no válidos.");
            }

            var multa = new Multa
            {
                IdMulta = Guid.NewGuid(),
                VehiculoId = multaDTO.VehiculoId,
                UsuarioIdOficial = multaDTO.UsuarioIdOficial,
                FechaHora = multaDTO.FechaHora,
                Latitud = decimal.Parse(multaDTO.Latitud),
                Longitud = decimal.Parse(multaDTO.Longitud),
                Comentario = multaDTO.Comentario,
                FotoPlaca = multaDTO.FotoPlaca,
                Estado = (EstadoMulta)multaDTO.Estado
            };

            _context.Multas.Add(multa);
            await _context.SaveChangesAsync();

            multaDTO.IdMulta = multa.IdMulta;
            return CreatedAtAction(nameof(GetMulta), new { id = multa.IdMulta }, multaDTO);
        }

        // Cambiar estado a "En Disputa"
        [HttpPost("{id}/en-disputa")]
        public async Task<IActionResult> CambiarEstadoEnDisputa(Guid id)
        {
            var multa = await _context.Multas.FindAsync(id);
            if (multa == null)
            {
                return NotFound("Multa no encontrada.");
            }

            multa.Estado = EstadoMulta.EnDisputa;
            _context.Entry(multa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Estado de la multa cambiado a En Disputa.");
        }

        // Cambiar estado a "Pagada"
        [HttpPost("{id}/pagada")]
        public async Task<IActionResult> CambiarEstadoPagada(Guid id)
        {
            var multa = await _context.Multas.FindAsync(id);
            if (multa == null)
            {
                return NotFound("Multa no encontrada.");
            }

            multa.Estado = EstadoMulta.Pagada;
            _context.Entry(multa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Estado de la multa cambiado a Pagada.");
        }

        // GET: api/Multas/infraccion/{idInfraccion} - Obtener multas por infracción
        [HttpGet("infraccion/{idInfraccion}")]
        public async Task<ActionResult<IEnumerable<MultaDTO>>> GetMultasPorInfraccion(Guid idInfraccion)
        {
            var multas = await _context.Multas
                .FromSqlRaw("EXEC sp_obtenerMultasPorInfraccion @idInfraccion = {0}", idInfraccion)
                .Select(m => new MultaDTO
                {
                    IdMulta = m.IdMulta,
                    VehiculoId = m.VehiculoId,
                    UsuarioIdOficial = m.UsuarioIdOficial,
                    FechaHora = m.FechaHora,
                    Latitud = m.Latitud.ToString(),
                    Longitud = m.Longitud.ToString(),
                    Comentario = m.Comentario,
                    FotoPlaca = m.FotoPlaca,
                    Estado = (int)m.Estado
                })
                .ToListAsync();

            if (multas == null || !multas.Any())
            {
                return NotFound("No se encontraron multas para la infracción especificada.");
            }

            return Ok(multas);
        }

        // Asignar infracción a una multa
        [HttpPost("{id}/infracciones/{infraccionId}")]
        public async Task<IActionResult> AsignarInfraccionAMulta(Guid id, Guid infraccionId)
        {
            if (!MultaExists(id) || !_context.Infracciones.Any(i => i.IdInfraccion == infraccionId))
            {
                return NotFound("Multa o infracción no encontrada.");
            }

            var infraccionAsignada = await _context.MultaInfracciones
                .AnyAsync(mi => mi.MultaId == id && mi.InfraccionId == infraccionId);

            if (infraccionAsignada)
            {
                return BadRequest("La infracción ya está asignada a esta multa.");
            }

            // Ejecutar el stored procedure para asignar la infracción a la multa
            await _context.Database.ExecuteSqlRawAsync("EXEC sp_asignarInfraccionAMulta @Multa_idMulta = {0}, @Infraccion_idInfraccion = {1}", id, infraccionId);

            return Ok("Infracción asignada a la multa.");
        }

        // Obtener multas por número de placa
        [HttpGet("placa/{numeroPlaca}")]
        [AllowAnonymous] // Permite acceso público
        public async Task<ActionResult<IEnumerable<MultaDTO>>> GetMultasPorPlaca(string numeroPlaca)
        {
            var multas = await _context.Multas
                .FromSqlRaw("EXEC sp_obtenerMultasPorPlaca @NumeroPlaca = {0}", numeroPlaca)
                .Select(m => new MultaDTO
                {
                    IdMulta = m.IdMulta,
                    FechaHora = m.FechaHora,
                    Latitud = m.Latitud.ToString(),
                    Longitud = m.Longitud.ToString(),
                    FotoPlaca = m.FotoPlaca,
                    Estado = (int)m.Estado,
                    VehiculoId = m.VehiculoId
                })
                .ToListAsync();

            if (multas == null || !multas.Any())
            {
                return NotFound("No se encontraron multas para el número de placa proporcionado.");
            }

            return Ok(multas);
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
