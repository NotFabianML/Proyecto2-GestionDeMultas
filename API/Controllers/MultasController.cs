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

        // Obtener multa por estado - GET: api/Multas/estado/1
        [HttpGet("{estado}")]
        //[Authorize]
        public async Task<ActionResult<MultaDTO>> GetMultaPorEstado(int estado)
        {
            var multa = await _context.Multas
                .Where(m => (int)m.Estado == estado)
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
                return NotFound();
            }

            return multa;
        }

        // Obtener multas por número de placa - CONSULTA PUBLICA
        [HttpGet("{numeroPlaca}")]
        [AllowAnonymous] // Permite acceso público
        public async Task<ActionResult<IEnumerable<MultaDTO>>> GetMultasPorPlaca(string numeroPlaca)
        {
            var multas = await _context.Multas
                .FromSqlRaw("EXEC sp_GetMultasPorPlaca @NumeroPlaca = {0}", numeroPlaca)
                .Select(m => new MultaDTO
                {
                    IdMulta = m.IdMulta,
                    FechaHora = m.FechaHora,
                    Latitud = m.Latitud.ToString(),
                    Longitud = m.Longitud.ToString(),
                    Comentario = m.Comentario,
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

        // Obtener multas por infraccion
        [HttpGet("{infraccionid}")]
        public async Task<ActionResult<IEnumerable<MultaDTO>>> GetMultasPorInfraccion(Guid infraccionid)
        {
            var multas = await _context.Multas
                .FromSqlRaw("EXEC sp_GetMultasPorInfraccion @idInfraccion = {0}", infraccionid)
                .Select(m => new MultaDTO
                {
                    IdMulta = m.IdMulta,
                    FechaHora = m.FechaHora,
                    Latitud = m.Latitud.ToString(),
                    Longitud = m.Longitud.ToString(),
                    Comentario = m.Comentario,
                    FotoPlaca = m.FotoPlaca,
                    Estado = (int)m.Estado,
                    VehiculoId = m.VehiculoId
                })
                .ToListAsync();

            if (multas == null || !multas.Any())
            {
                return NotFound("No se encontraron multas para la infraccion proporcionado.");
            }

            return Ok(multas);
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
        [HttpPost("{id}/cambiar-estado/{estado}")]
        public async Task<IActionResult> CambiarEstado(Guid id, int estado)
        {
            var multa = await _context.Multas.FindAsync(id);
            if (multa == null)
            {
                return NotFound("Multa no encontrada.");
            }

            switch (estado)
            {
                case 0:
                    multa.Estado = EstadoMulta.Pagada;
                    break;
                case 1:
                    multa.Estado = EstadoMulta.EnDisputa;
                    break;
                case 2:
                    multa.Estado = EstadoMulta.Pagada;
                    break;
                default:
                    return BadRequest("Estado no válido.");
            }

            _context.Entry(multa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Estado de la multa cambiado.");
        }

        private bool MultaExists(Guid id)
        {
            return _context.Multas.Any(e => e.IdMulta == id);
        }

        private bool InfraccionExists(Guid id)
        {
            return _context.Infracciones.Any(e => e.IdInfraccion == id);
        }

        private async Task<bool> VehiculoAndOficialExist(Guid vehiculoId, Guid oficialId)
        {
            return await _context.Vehiculos.AnyAsync(v => v.IdVehiculo == vehiculoId) &&
                   await _context.Usuarios.AnyAsync(u => u.IdUsuario == oficialId);
        }
    }
}
