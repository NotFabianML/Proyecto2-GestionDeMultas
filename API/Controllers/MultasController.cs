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
                    Latitud = m.Latitud,
                    Longitud = m.Longitud,
                    Comentario = m.Comentario,
                    FotoPlaca = m.FotoPlaca,
                    Estado = (int)m.Estado,
                    MontoTotal = m.MultaInfracciones.Sum(mi => mi.Infraccion.Monto)
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
                    Latitud = m.Latitud,
                    Longitud = m.Longitud,
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
        [HttpGet("estado/{estado}")]
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
                    Latitud = m.Latitud,
                    Longitud = m.Longitud,
                    Comentario = m.Comentario,
                    FotoPlaca = m.FotoPlaca,
                    Estado = (int)m.Estado
                })
                .ToListAsync();

            if (multa == null)
            {
                return NotFound();
            }

            return Ok(multa);
        }

        // Obtener todas las multas por cédula del usuario final - CONSULTA PUBLICA
        [HttpGet("usuario/cedula/{cedula}")]
        [AllowAnonymous] // Permite acceso público
        public async Task<ActionResult<IEnumerable<ConsultaPublicaDTO>>> GetMultasPorCedulaUsuarioFinal(string cedula)
        {
            // Verificar si el usuario con la cédula existe
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Cedula == cedula);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            // Obtener los vehículos del usuario
            var vehiculosIds = await _context.Vehiculos
                .Where(v => v.UsuarioId == usuario.IdUsuario)
                .Select(v => v.IdVehiculo)
                .ToListAsync();

            // Obtener las multas relacionadas con esos vehículos
            var multas = await _context.Multas
                .Where(m => vehiculosIds.Contains(m.VehiculoId))
                .ToListAsync();

            // Mapeo de multas a ConsultaPublicaDTO
            var consultasPublicas = await MapearMultasAConsultaPublica(multas);

            return Ok(consultasPublicas);
        }

        // Obtener todas las multas por ID del usuario final - CONSULTA PUBLICA
        [HttpGet("usuario/id/{usuarioId}")]
        [AllowAnonymous] // Permite acceso público
        public async Task<ActionResult<IEnumerable<ConsultaPublicaDTO>>> GetMultasPorIdUsuarioFinal(Guid usuarioId)
        {
            // Verificar si el usuario con el ID existe
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            // Obtener los vehículos del usuario
            var vehiculosIds = await _context.Vehiculos
                .Where(v => v.UsuarioId == usuario.IdUsuario)
                .Select(v => v.IdVehiculo)
                .ToListAsync();

            // Obtener las multas relacionadas con esos vehículos
            var multas = await _context.Multas
                .Where(m => vehiculosIds.Contains(m.VehiculoId))
                .ToListAsync();

            // Mapeo de multas a ConsultaPublicaDTO
            var consultasPublicas = await MapearMultasAConsultaPublica(multas);

            return Ok(consultasPublicas);
        }

        // Obtener multas por número de placa - CONSULTA PUBLICA
        [HttpGet("placa/{numeroPlaca}")]
        [AllowAnonymous] // Permite acceso público
        public async Task<ActionResult<IEnumerable<ConsultaPublicaDTO>>> GetMultasPorPlaca(string numeroPlaca)
        {
            // Verifica si el vehículo existe en la base de datos
            if (!VehiculoExists(numeroPlaca))
            {
                return NotFound("Vehículo no encontrado.");
            }

            // Ejecuta el procedimiento almacenado para obtener las multas del vehículo
            var multas = await _context.Multas
                .FromSqlRaw("EXEC sp_GetMultasPorPlaca @NumeroPlaca = {0}", numeroPlaca)
                .ToListAsync();

            // Mapea las multas a ConsultaPublicaDTO usando la función auxiliar
            var consultasPublicas = await MapearMultasAConsultaPublica(multas);

            return Ok(consultasPublicas);
        }

        // Obtener multas por título de infracción - CONSULTA PUBLICA
        [HttpGet("infraccion/{tituloInfraccion}")]
        public async Task<ActionResult<IEnumerable<ConsultaPublicaDTO>>> GetMultasPorTituloInfraccion(string tituloInfraccion)
        {
            // Ejecuta el procedimiento almacenado para obtener las multas relacionadas con el título de infracción
            var multas = await _context.Multas
                .FromSqlRaw("EXEC sp_GetMultasPorTituloInfraccion @TituloInfraccion = {0}", tituloInfraccion)
                .ToListAsync();

            // Mapea las multas a ConsultaPublicaDTO usando la función auxiliar
            var consultasPublicas = await MapearMultasAConsultaPublica(multas);

            return Ok(consultasPublicas);
        }



        // Obtener multas por infraccion
        [HttpGet("{infraccionid}/multas-por-infraccion")]
        public async Task<ActionResult<IEnumerable<MultaDTO>>> GetMultasPorInfraccion(Guid infraccionid)
        {
            var multas = await _context.Multas
                .FromSqlRaw("EXEC sp_GetMultasPorInfraccion @idInfraccion = {0}", infraccionid)
                .Select(m => new MultaDTO
                {
                    IdMulta = m.IdMulta,
                    FechaHora = m.FechaHora,
                    Latitud = m.Latitud,
                    Longitud = m.Longitud,
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
                new MultaDTO { IdMulta = Guid.NewGuid(), VehiculoId = await ObtenerIdVehiculo("123456"), UsuarioIdOficial = await ObtenerIdUsuario("luiss@nextek.com"), FechaHora = DateTime.Now.AddDays(-10), Latitud = (decimal)9.9281, Longitud = (decimal)-84.0907, Comentario = "Exceso de velocidad", Estado = 1 },
                new MultaDTO { IdMulta = Guid.NewGuid(), VehiculoId = await ObtenerIdVehiculo("234567"), UsuarioIdOficial = await ObtenerIdUsuario("sofiac@nextek.com"), FechaHora = DateTime.Now.AddDays(-8), Latitud = (decimal)9.9347, Longitud = (decimal)-84.0875, Comentario = "Uso indebido del carril", Estado = 1 },

                new MultaDTO { IdMulta = Guid.NewGuid(), VehiculoId = await ObtenerIdVehiculo("345678"), UsuarioIdOficial = await ObtenerIdUsuario("andresv@nextek.com"), FechaHora = DateTime.Now.AddDays(-15), Latitud = (decimal)9.9352, Longitud = (decimal)-84.0833, Comentario = "Conducir sin luces", Estado = 1 },
                new MultaDTO { IdMulta = Guid.NewGuid(), VehiculoId = await ObtenerIdVehiculo("456789"), UsuarioIdOficial = await ObtenerIdUsuario("luiss@nextek.com"), FechaHora = DateTime.Now.AddDays(-5), Latitud = (decimal)9.9273, Longitud = (decimal)-84.0928, Comentario = "Exceso de velocidad", Estado = 1 },

                new MultaDTO { IdMulta = Guid.NewGuid(), VehiculoId = await ObtenerIdVehiculo("567890"), UsuarioIdOficial = await ObtenerIdUsuario("sofiac@nextek.com"), FechaHora = DateTime.Now.AddDays(-7), Latitud = (decimal)9.9258, Longitud = (decimal)-84.0923, Comentario = "Estacionamiento indebido", Estado = 1 },
                new MultaDTO { IdMulta = Guid.NewGuid(), VehiculoId = await ObtenerIdVehiculo("678901"), UsuarioIdOficial = await ObtenerIdUsuario("andresv@nextek.com"), FechaHora = DateTime.Now.AddDays(-3), Latitud = (decimal)9.9299, Longitud = (decimal)-84.0890, Comentario = "No respetar señales", Estado = 1 }
            };

            foreach (var multaDTO in multasIniciales)
            {
                var multa = new Multa
                {
                    IdMulta = multaDTO.IdMulta,
                    VehiculoId = multaDTO.VehiculoId,
                    UsuarioIdOficial = multaDTO.UsuarioIdOficial,
                    FechaHora = multaDTO.FechaHora,
                    Latitud = multaDTO.Latitud,
                    Longitud = multaDTO.Longitud,
                    Comentario = multaDTO.Comentario,
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

        private async Task<List<ConsultaPublicaDTO>> MapearMultasAConsultaPublica(List<Multa> multas)
        {
            // Obtener previamente todas las placas y cédulas relacionadas para evitar concurrencia en DbContext
            var vehiculoIds = multas.Select(m => m.VehiculoId).Distinct();
            var usuarioIds = multas.Select(m => m.UsuarioIdOficial).Distinct();

            var vehiculos = await _context.Vehiculos
                .Where(v => vehiculoIds.Contains(v.IdVehiculo))
                .ToDictionaryAsync(v => v.IdVehiculo, v => v.NumeroPlaca);

            var usuarios = await _context.Usuarios
                .Where(u => usuarioIds.Contains(u.IdUsuario))
                .ToDictionaryAsync(u => u.IdUsuario, u => u.Cedula);

            // Crear la lista de ConsultaPublicaDTO usando los datos obtenidos previamente
            var consultasPublicas = multas.Select(m => new ConsultaPublicaDTO
            {
                IdMulta = m.IdMulta,
                NumeroPlaca = vehiculos[m.VehiculoId], // Usa el diccionario para obtener la placa
                CedulaOficial = usuarios[m.UsuarioIdOficial], // Usa el diccionario para obtener la cédula
                FechaHora = m.FechaHora,
                Latitud = m.Latitud,
                Longitud = m.Longitud,
                Comentario = m.Comentario,
                FotoPlaca = m.FotoPlaca,
                Estado = (int)m.Estado,

                // Calcula el monto total sumando los montos de las infracciones relacionadas
                MontoTotal = _context.Infracciones
                    .Where(i => _context.MultaInfracciones
                        .Where(mi => mi.MultaId == m.IdMulta)
                        .Select(mi => mi.InfraccionId)
                        .Contains(i.IdInfraccion))
                    .Sum(i => i.Monto),

                // Incluye las infracciones relacionadas con esta multa
                Infracciones = _context.Infracciones
                    .Where(i => _context.MultaInfracciones
                        .Where(mi => mi.MultaId == m.IdMulta)
                        .Select(mi => mi.InfraccionId)
                        .Contains(i.IdInfraccion))
                    .Select(i => new InfraccionDTO
                    {
                        IdInfraccion = i.IdInfraccion,
                        Descripcion = i.Descripcion,
                        Monto = i.Monto,
                        Titulo = i.Titulo,
                        Articulo = i.Articulo,
                        Estado = i.Estado
                    }).ToList()
            }).ToList();

            return consultasPublicas;
        }


    }
}
