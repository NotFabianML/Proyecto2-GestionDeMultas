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
                .Include(d => d.Multa)
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
                    FechaResolucion = d.FechaResolucion,
                    NumeroPlaca = d.Multa.NumeroPlaca // Incluye el número de placa
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
                    FechaResolucion = d.FechaResolucion,
                    NumeroPlaca = d.Multa.NumeroPlaca // Incluye el número de placa
                })
                .FirstOrDefaultAsync();

            if (disputa == null)
            {
                return NotFound("Disputa no encontrada.");
            }

            return Ok(disputa);
        }

        // Obtener disputas por usuario - GET: api/Disputas/usuario/5
        [HttpGet("{usuarioId}/disputas-por-usuario")]
        public async Task<ActionResult<IEnumerable<DisputaDTO>>> GetDisputasDeUsuario(Guid usuarioId)
        {
            var disputas = await _context.Disputas
                .Where(d => d.UsuarioId == usuarioId)
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
                    FechaResolucion = d.FechaResolucion,
                    NumeroPlaca = d.Multa.NumeroPlaca // Incluye el número de placa
                })
                .ToListAsync();

            if (!disputas.Any())
            {
                return NotFound("No se encontraron disputas para el usuario especificado.");
            }

            return Ok(disputas);
        }

        // GET: api/Disputas/juez/{juezId}/multas
        [HttpGet("juez/{juezId}/multas")]
        public async Task<ActionResult<IEnumerable<DisputaDTO>>> GetMultasAsignadasAJuez(Guid juezId)
        {
            var disputas = await _context.Disputas
                .Include(d => d.Multa)
                .Where(d => d.UsuarioIdJuez == juezId)
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
                    FechaResolucion = d.FechaResolucion,
                    NumeroPlaca = d.Multa.NumeroPlaca // Incluye el número de placa
                })
                .ToListAsync();

            if (!disputas.Any())
            {
                return NotFound("No se encontraron multas asignadas para este juez.");
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

            var existingDisputa = await _context.Disputas
                .Include(d => d.Multa)
                .FirstOrDefaultAsync(d => d.IdDisputa == id);

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

            // Incluye el número de placa en la respuesta actualizada
            disputaDTO.NumeroPlaca = existingDisputa.Multa.NumeroPlaca;
            return Ok(disputaDTO);
        }

        // POST: api/Disputas
        [HttpPost]
        public async Task<ActionResult<DisputaDTO>> PostDisputa(DisputaDTO disputaDTO)
        {
            // Verificar si la multa existe
            if (!await _context.Multas.AnyAsync(m => m.IdMulta == disputaDTO.MultaId))
            {
                return BadRequest("La multa especificada no existe.");
            }

            // Obtener el juez con menos disputas o de forma aleatoria en caso de empate
            var juezAsignado = await ObtenerJuezConMenosDisputas();
            if (juezAsignado == null)
            {
                return BadRequest("No hay jueces disponibles para asignar la disputa.");
            }

            // Crear la disputa y asignar el juez
            var disputa = new Disputa
            {
                IdDisputa = Guid.NewGuid(),
                MultaId = disputaDTO.MultaId,
                UsuarioId = disputaDTO.UsuarioId,
                UsuarioIdJuez = juezAsignado.IdUsuario, // Asignar el juez con menos disputas
                FechaCreacion = DateTime.UtcNow,
                MotivoReclamo = disputaDTO.MotivoReclamo,
                Estado = EstadoDisputa.EnDisputa, // Estado inicial
                ResolucionJuez = disputaDTO.ResolucionJuez,
                DeclaracionOficial = disputaDTO.DeclaracionOficial,
                FechaResolucion = disputaDTO.FechaResolucion
            };

            _context.Disputas.Add(disputa);
            await _context.SaveChangesAsync();

            disputaDTO.IdDisputa = disputa.IdDisputa;

            // Retornar el correo del juez asignado para notificación
            return CreatedAtAction(nameof(GetDisputa), new { id = disputa.IdDisputa }, new
            {
                DisputaId = disputaDTO.IdDisputa,
                JuezAsignadoCorreo = juezAsignado.Email // Retornar el correo del juez asignado
            });
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

        // Cambiar estado a Disputa - POST: api/Disputas/5/cambiar-estado/0
        //[HttpPost("{id}/cambiar-estado/{estado}")]
        //public async Task<IActionResult> CambiarEstado(Guid id, int estado)
        //{
        //    var disputa = await _context.Disputas.FindAsync(id);
        //    if (disputa == null)
        //    {
        //        return NotFound("Disputa no encontrada.");
        //    }

        //    switch (estado)
        //    {
        //        case 0:
        //            disputa.Estado = EstadoDisputa.EnDisputa;
        //            break;
        //        case 1:
        //            disputa.Estado = EstadoDisputa.Aceptada;
        //            break;
        //        case 2:
        //            disputa.Estado = EstadoDisputa.Rechazada;
        //            break;
        //        default:
        //            return BadRequest("Estado de disputa no válido.");
        //    }

        //    _context.Entry(disputa).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();

        //    return Ok("Estado de la disputa cambio");
        //}

        // Cambiar estado a Disputa - POST: api/Disputas/5/cambiar-estado/0
        [HttpPost("{id}/cambiar-estado/{estado}")]
        public async Task<IActionResult> CambiarEstado(Guid id, int estado)
        {
            var disputa = await _context.Disputas.FindAsync(id);
            if (disputa == null)
            {
                return NotFound("Disputa no encontrada.");
            }

            disputa.Estado = estado switch
            {
                0 => EstadoDisputa.EnDisputa,
                1 => EstadoDisputa.Aceptada,
                2 => EstadoDisputa.Rechazada,
                _ => throw new ArgumentException("Estado de disputa no válido.")
            };

            _context.Entry(disputa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Estado de la disputa cambiado exitosamente.");
        }

        // Método auxiliar para obtener el juez con menos disputas asignadas
        private async Task<Usuario?> ObtenerJuezConMenosDisputas()
        {
            // Obtener la lista de jueces de tránsito
            var jueces = await _context.Usuarios
                .Where(u => _context.Roles
                    .Any(r => r.NombreRol == "Juez de Tránsito" && r.UsuarioRoles.Any(ru => ru.UsuarioId == u.IdUsuario)))
                .ToListAsync();

            if (!jueces.Any())
            {
                return null; // En caso de que no haya jueces registrados
            }

            // Ordenar los jueces por la cantidad de disputas asignadas, de menor a mayor
            var juezConMenosDisputas = jueces
                .OrderBy(j => _context.Disputas.Count(d => d.UsuarioIdJuez == j.IdUsuario))
                .ToList();

            // Obtener la cantidad de disputas del juez con menos disputas asignadas
            var minDisputas = _context.Disputas.Count(d => d.UsuarioIdJuez == juezConMenosDisputas.First().IdUsuario);

            // Filtrar jueces que tienen la misma cantidad mínima de disputas
            var juecesConMinDisputas = juezConMenosDisputas
                .Where(j => _context.Disputas.Count(d => d.UsuarioIdJuez == j.IdUsuario) == minDisputas)
                .ToList();

            // Seleccionar aleatoriamente un juez si hay empate en la cantidad de disputas
            return juecesConMinDisputas.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
        }

        private bool DisputaExists(Guid id)
        {
            return _context.Disputas.Any(e => e.IdDisputa == id);
        }

        // POST: api/Disputas/Inicializar
        [HttpPost("Inicializar")]
        public async Task<ActionResult> InicializarDisputas()
        {
            // Obtener todas las multas iniciales por placa
            var multas = await _context.Multas
                .Where(m => new[] { "123456", "234567", "345678", "456789", "567890", "678901" }
                .Contains(m.NumeroPlaca))
                .ToListAsync();

            // Obtener los jueces y usuarios finales
            var jueces = await _context.Usuarios
                .Where(u => new[] { "lauras@nextek.com", "diegom@nextek.com", "anah@nextek.com" }
                .Contains(u.Email))
                .ToListAsync();

            var usuariosFinales = await _context.Usuarios
                .Where(u => new[] { "carlosg@gmail.com", "mariap@gmail.com", "juanr@gmail.com" }
                .Contains(u.Email))
                .ToDictionaryAsync(u => u.Email, u => u);

            var juecesIndex = 0;

            var disputasIniciales = new List<Disputa>();

            // Crear disputas para las multas
            foreach (var multa in multas)
            {
                // Obtener usuario final para esta multa
                var usuarioEmail = multa.CedulaInfractor == "218860349" ? "carlosg@gmail.com" :
                                   multa.CedulaInfractor == "318860349" ? "mariap@gmail.com" :
                                   "juanr@gmail.com";

                var usuarioFinal = usuariosFinales[usuarioEmail];

                // Asignar juez de manera equitativa
                var juezAsignado = jueces[juecesIndex];
                juecesIndex = (juecesIndex + 1) % jueces.Count;

                // Crear nueva disputa
                var disputa = new Disputa
                {
                    IdDisputa = Guid.NewGuid(),
                    MultaId = multa.IdMulta,
                    UsuarioId = usuarioFinal.IdUsuario,
                    UsuarioIdJuez = juezAsignado.IdUsuario,
                    FechaCreacion = DateTime.UtcNow,
                    MotivoReclamo = "Revisión de multa asignada.",
                    Estado = EstadoDisputa.EnDisputa,
                    ResolucionJuez = "Pendiente de resolución",
                    DeclaracionOficial = "En espera de análisis",
                    FechaResolucion = null
                };

                disputasIniciales.Add(disputa);
            }

            _context.Disputas.AddRange(disputasIniciales);
            await _context.SaveChangesAsync();

            return Ok("Disputas iniciales agregadas exitosamente.");
        }


    }
}
