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
                .Include(m => m.MultaInfracciones)
                .ThenInclude(mi => mi.Infraccion)
                .ToListAsync();

            var multasDTO = await MapearMultasConInfracciones(multas);
            return Ok(multasDTO);
        }

        // GET: api/Multas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MultaDTO>> GetMulta(Guid id)
        {
            var multa = await _context.Multas
                .Include(m => m.MultaInfracciones)
                .ThenInclude(mi => mi.Infraccion)
                .FirstOrDefaultAsync(m => m.IdMulta == id);

            if (multa == null)
            {
                return NotFound("Multa no encontrada.");
            }

            var multasDTO = await MapearMultasConInfracciones(new List<Multa> { multa });
            return Ok(multasDTO.FirstOrDefault());
        }

        // Obtener multa por estado - GET: api/Multas/estado/1
        [HttpGet("estado/{estado}")]
        public async Task<ActionResult<IEnumerable<MultaDTO>>> GetMultaPorEstado(int estado)
        {
            var multas = await _context.Multas
                .Where(m => (int)m.Estado == estado)
                .Include(m => m.MultaInfracciones)
                .ThenInclude(mi => mi.Infraccion)
                .ToListAsync();

            if (!multas.Any())
            {
                return NotFound();
            }

            var multasDTO = await MapearMultasConInfracciones(multas);
            return Ok(multasDTO);
        }

        // Obtener todas las multas por cédula del usuario final - CONSULTA PUBLICA
        [HttpGet("usuario/cedula/{cedula}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MultaDTO>>> GetMultasPorCedulaUsuarioFinal(string cedula)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Cedula == cedula);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var multas = await _context.Multas
                .Where(m => m.CedulaInfractor == cedula)
                .Include(m => m.MultaInfracciones)
                .ThenInclude(mi => mi.Infraccion)
                .ToListAsync();

            var multasDTO = await MapearMultasConInfracciones(multas);
            return Ok(multasDTO);
        }

        // Obtener todas las multas por ID del usuario final - CONSULTA PUBLICA
        [HttpGet("usuario/id/{usuarioId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MultaDTO>>> GetMultasPorIdUsuarioFinal(Guid usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var multas = await _context.Multas
                .Where(m => m.CedulaInfractor == usuario.Cedula)
                .Include(m => m.MultaInfracciones)
                .ThenInclude(mi => mi.Infraccion)
                .ToListAsync();

            var multasDTO = await MapearMultasConInfracciones(multas);
            return Ok(multasDTO);
        }

        // Obtener multas asignadas por el oficial - GET: api/Multas/oficial/{oficialId}
        [HttpGet("oficial/{oficialId}")]
        public async Task<ActionResult<IEnumerable<MultaDTO>>> GetMultasPorIdOficial(Guid oficialId)
        {
            var multas = await _context.Multas
                .Where(m => m.UsuarioIdOficial == oficialId)
                .Include(m => m.MultaInfracciones)
                .ThenInclude(mi => mi.Infraccion)
                .ToListAsync();

            if (!multas.Any())
            {
                return NotFound("No se encontraron multas asignadas por el oficial proporcionado.");
            }

            var multasDTO = await MapearMultasConInfracciones(multas);
            return Ok(multasDTO);
        }


        // Obtener multas por número de placa - CONSULTA PUBLICA
        [HttpGet("placa/{numeroPlaca}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MultaDTO>>> GetMultasPorPlaca(string numeroPlaca)
        {
            var multas = await _context.Multas
                .Where(m => m.NumeroPlaca == numeroPlaca)
                .Include(m => m.MultaInfracciones)
                .ThenInclude(mi => mi.Infraccion)
                .ToListAsync();

            if (!multas.Any())
            {
                return NotFound("Vehículo no encontrado.");
            }

            var multasDTO = await MapearMultasConInfracciones(multas);
            return Ok(multasDTO);
        }

        // Obtener multas por infracción
        [HttpGet("{infraccionid}/multas-por-infraccion")]
        public async Task<ActionResult<IEnumerable<MultaDTO>>> GetMultasPorInfraccion(Guid infraccionid)
        {
            var multas = await _context.Multas
                .Include(m => m.MultaInfracciones)
                .ThenInclude(mi => mi.Infraccion)
                .Where(m => m.MultaInfracciones.Any(mi => mi.InfraccionId == infraccionid))
                .ToListAsync();

            if (!multas.Any())
            {
                return NotFound("No se encontraron multas para la infracción proporcionada.");
            }

            var multasDTO = await MapearMultasConInfracciones(multas);
            return Ok(multasDTO);
        }

        // PUT: api/Multas/5
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

            // Actualiza los campos de acuerdo al DTO y nuevo modelo
            multa.NumeroPlaca = multaDTO.NumeroPlaca;  // Usa NumeroPlaca en lugar de VehiculoId
            multa.CedulaInfractor = multaDTO.CedulaInfractor;  // Agrega CedulaInfractor
            multa.UsuarioIdOficial = multaDTO.UsuarioIdOficial;
            multa.FechaHora = multaDTO.FechaHora;
            multa.Latitud = multaDTO.Latitud;
            multa.Longitud = multaDTO.Longitud;
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
            var multa = new Multa
            {
                IdMulta = Guid.NewGuid(),
                NumeroPlaca = multaDTO.NumeroPlaca,
                CedulaInfractor = multaDTO.CedulaInfractor,
                UsuarioIdOficial = multaDTO.UsuarioIdOficial,
                FechaHora = multaDTO.FechaHora,
                Latitud = multaDTO.Latitud,
                Longitud = multaDTO.Longitud,
                Comentario = multaDTO.Comentario,
                FotoPlaca = multaDTO.FotoPlaca,
                Estado = (EstadoMulta)multaDTO.Estado
            };

            _context.Multas.Add(multa);
            await _context.SaveChangesAsync();

            multaDTO.IdMulta = multa.IdMulta;
            return CreatedAtAction(nameof(GetMulta), new { id = multa.IdMulta }, multaDTO);
        }

        // DELETE: api/Multas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMulta(Guid id)
        {
            var multa = await _context.Multas
                .Include(m => m.MultaInfracciones) // Incluye las relaciones
                .FirstOrDefaultAsync(m => m.IdMulta == id);

            if (multa == null)
            {
                return NotFound("Multa no encontrada.");
            }

            // Eliminar las relaciones en MultaInfracciones
            _context.MultaInfracciones.RemoveRange(multa.MultaInfracciones);

            // Eliminar la multa en sí
            _context.Multas.Remove(multa);

            await _context.SaveChangesAsync();

            return Ok("Multa eliminada exitosamente.");
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

        private bool VehiculoExists(string placa)
        {
            return _context.Vehiculos.Any(e => e.NumeroPlaca == placa);
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

        // POST: api/Multas/Inicializar
        [HttpPost("Inicializar")]
        public async Task<ActionResult> InicializarMultas()
        {
            var multasIniciales = new List<MultaDTO>
            {
                // Multas para vehículos de Carlos Gomez
                new MultaDTO { IdMulta = Guid.NewGuid(), NumeroPlaca = "123456", CedulaInfractor = "218860349", UsuarioIdOficial = await ObtenerIdUsuario("luiss@nextek.com"), FechaHora = DateTime.Now.AddDays(-10), Latitud = (decimal)9.9281, Longitud = (decimal)-84.0907, Comentario = "Exceso de velocidad", Estado = 1 },
                new MultaDTO { IdMulta = Guid.NewGuid(), NumeroPlaca = "234567", CedulaInfractor = "218860349", UsuarioIdOficial = await ObtenerIdUsuario("sofiac@nextek.com"), FechaHora = DateTime.Now.AddDays(-8), Latitud = (decimal)9.9347, Longitud = (decimal)-84.0875, Comentario = "Uso indebido del carril", Estado = 1 },

                // Multas para vehículos de Maria Perez
                new MultaDTO { IdMulta = Guid.NewGuid(), NumeroPlaca = "345678", CedulaInfractor = "318860349", UsuarioIdOficial = await ObtenerIdUsuario("andresv@nextek.com"), FechaHora = DateTime.Now.AddDays(-15), Latitud = (decimal)9.9352, Longitud = (decimal)-84.0833, Comentario = "Conducir sin luces", Estado = 1 },
                new MultaDTO { IdMulta = Guid.NewGuid(), NumeroPlaca = "456789", CedulaInfractor = "318860349", UsuarioIdOficial = await ObtenerIdUsuario("luiss@nextek.com"), FechaHora = DateTime.Now.AddDays(-5), Latitud = (decimal)9.9273, Longitud = (decimal)-84.0928, Comentario = "Exceso de velocidad", Estado = 1 },

                // Multas para vehículos de Juan Rojas
                new MultaDTO { IdMulta = Guid.NewGuid(), NumeroPlaca = "567890", CedulaInfractor = "418860349", UsuarioIdOficial = await ObtenerIdUsuario("sofiac@nextek.com"), FechaHora = DateTime.Now.AddDays(-7), Latitud = (decimal)9.9258, Longitud = (decimal)-84.0923, Comentario = "Estacionamiento indebido", Estado = 1 },
                new MultaDTO { IdMulta = Guid.NewGuid(), NumeroPlaca = "678901", CedulaInfractor = "418860349", UsuarioIdOficial = await ObtenerIdUsuario("andresv@nextek.com"), FechaHora = DateTime.Now.AddDays(-3), Latitud = (decimal)9.9299, Longitud = (decimal)-84.0890, Comentario = "No respetar señales", Estado = 1 }
            };

            foreach (var multaDTO in multasIniciales)
            {
                var multa = new Multa
                {
                    IdMulta = multaDTO.IdMulta,
                    NumeroPlaca = multaDTO.NumeroPlaca,
                    CedulaInfractor = multaDTO.CedulaInfractor,
                    UsuarioIdOficial = multaDTO.UsuarioIdOficial,
                    FechaHora = multaDTO.FechaHora,
                    Latitud = multaDTO.Latitud,
                    Longitud = multaDTO.Longitud,
                    Comentario = multaDTO.Comentario,
                    FotoPlaca = multaDTO.FotoPlaca,
                    Estado = (EstadoMulta)multaDTO.Estado
                };

                _context.Multas.Add(multa);
            }

            await _context.SaveChangesAsync();

            return Ok("Multas iniciales agregadas exitosamente.");
        }


        // Método auxiliar para obtener el Id de vehículo por número de placa
        private async Task<Guid> ObtenerIdVehiculo(string numeroPlaca)
        {
            var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.NumeroPlaca == numeroPlaca);
            return vehiculo?.IdVehiculo ?? Guid.Empty;
        }

        // Método auxiliar para obtener el Id de vehículo por número de placa
        private async Task<string> ObtenerPlaca(Guid vehiculoId)
        {
            var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.IdVehiculo == vehiculoId);
            return vehiculo?.NumeroPlaca ?? string.Empty;
        }

        // Método auxiliar para obtener el Id de usuario por email
        private async Task<Guid> ObtenerIdUsuario(string email)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            return usuario?.IdUsuario ?? Guid.Empty;
        }

        // Método auxiliar para obtener el Id de usuario por email
        private async Task<string> ObtenerCedulaUsuario(Guid usuarioId)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);
            return usuario?.Cedula ?? string.Empty;
        }

        private async Task<List<MultaDTO>> MapearMultasConInfracciones(List<Multa> multas)
        {
            var usuarios = await _context.Usuarios
                .Where(u => multas.Select(m => m.UsuarioIdOficial).Contains(u.IdUsuario))
                .ToDictionaryAsync(u => u.IdUsuario, u => u.Cedula);

            var multasDTO = multas.Select(m => new MultaDTO
            {
                IdMulta = m.IdMulta,
                NumeroPlaca = m.NumeroPlaca,
                CedulaInfractor = m.CedulaInfractor,
                UsuarioIdOficial = m.UsuarioIdOficial,
                FechaHora = m.FechaHora,
                Latitud = m.Latitud,
                Longitud = m.Longitud,
                Comentario = m.Comentario,
                FotoPlaca = m.FotoPlaca,
                Estado = (int)m.Estado,
                MontoTotal = m.MultaInfracciones.Sum(mi => mi.Infraccion.Monto),
                Infracciones = m.MultaInfracciones.Select(mi => new InfraccionDTO
                {
                    IdInfraccion = mi.Infraccion.IdInfraccion,
                    Descripcion = mi.Infraccion.Descripcion,
                    Monto = mi.Infraccion.Monto,
                    Titulo = mi.Infraccion.Titulo,
                    Articulo = mi.Infraccion.Articulo,
                    Estado = mi.Infraccion.Estado
                }).ToList()
            }).ToList();

            return multasDTO;
        }

    }
}
