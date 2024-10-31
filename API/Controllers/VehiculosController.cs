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
    public class VehiculosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehiculosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Vehiculos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehiculoDTO>>> GetVehiculos()
        {
            var vehiculos = await _context.Vehiculos
                .Select(v => new VehiculoDTO
                {
                    IdVehiculo = v.IdVehiculo,
                    UsuarioId = v.UsuarioId,
                    NumeroPlaca = v.NumeroPlaca,
                    FotoVehiculo = v.FotoVehiculo,
                    Marca = v.Marca,
                    Anno = v.Anno ?? 0
                })
                .ToListAsync();

            return vehiculos;
        }

        // GET: api/Vehiculos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehiculoDTO>> GetVehiculo(Guid id)
        {
            var vehiculo = await _context.Vehiculos
                .Where(v => v.IdVehiculo == id)
                .Select(v => new VehiculoDTO
                {
                    IdVehiculo = v.IdVehiculo,
                    UsuarioId = v.UsuarioId,
                    NumeroPlaca = v.NumeroPlaca,
                    FotoVehiculo = v.FotoVehiculo,
                    Marca = v.Marca,
                    Anno = v.Anno ?? 0
                })
                .FirstOrDefaultAsync();

            if (vehiculo == null)
            {
                return NotFound("Vehículo no encontrado.");
            }

            return vehiculo;
        }

        // GET: api/Vehiculos/placa/{numeroPlaca}
        [HttpGet("placa/{numeroPlaca}")]
        public async Task<ActionResult<VehiculoDTO>> GetVehiculoPorPlaca(string numeroPlaca)
        {
            var vehiculo = await _context.Vehiculos
                .Where(v => v.NumeroPlaca == numeroPlaca)
                .Select(v => new VehiculoDTO
                {
                    IdVehiculo = v.IdVehiculo,
                    UsuarioId = v.UsuarioId,
                    NumeroPlaca = v.NumeroPlaca,
                    FotoVehiculo = v.FotoVehiculo,
                    Marca = v.Marca,
                    Anno = v.Anno ?? 0
                })
                .FirstOrDefaultAsync();

            if (vehiculo == null)
            {
                return NotFound("Vehículo no encontrado.");
            }

            return vehiculo;
        }

        // GET: api/Vehiculos/usuario/5
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<VehiculoDTO>>> GetVehiculosPorUsuario(Guid usuarioId)
        {
            var vehiculos = await _context.Vehiculos
                .Where(v => v.UsuarioId == usuarioId)
                .Select(v => new VehiculoDTO
                {
                    IdVehiculo = v.IdVehiculo,
                    UsuarioId = v.UsuarioId,
                    NumeroPlaca = v.NumeroPlaca,
                    FotoVehiculo = v.FotoVehiculo,
                    Marca = v.Marca,
                    Anno = v.Anno ?? 0
                })
                .ToListAsync();

            if (vehiculos == null || vehiculos.Count == 0)
            {
                return NotFound("No se encontraron vehículos para el usuario.");
            }

            return vehiculos;
        }

        // PUT: api/Vehiculos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVehiculo(Guid id, VehiculoDTO vehiculoDTO)
        {
            if (id != vehiculoDTO.IdVehiculo)
            {
                return BadRequest("El ID proporcionado no coincide con el vehículo.");
            }

            var existingVehiculo = await _context.Vehiculos.FindAsync(id);
            if (existingVehiculo == null)
            {
                return NotFound("Vehículo no encontrado.");
            }

            // Actualizar campos
            existingVehiculo.NumeroPlaca = vehiculoDTO.NumeroPlaca;
            existingVehiculo.FotoVehiculo = vehiculoDTO.FotoVehiculo;
            existingVehiculo.Marca = vehiculoDTO.Marca;
            existingVehiculo.Anno = vehiculoDTO.Anno;

            _context.Entry(existingVehiculo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehiculoExists(id))
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

        // POST: api/Vehiculos
        [HttpPost]
        public async Task<ActionResult<VehiculoDTO>> PostVehiculo(VehiculoDTO vehiculoDTO)
        {
            // Verificar si la placa ya existe
            var placaExistente = await _context.Vehiculos
                .AnyAsync(v => v.NumeroPlaca == vehiculoDTO.NumeroPlaca);

            if (placaExistente)
            {
                return Conflict("El número de placa ya está registrado.");
            }

            var vehiculo = new Vehiculo
            {
                IdVehiculo = Guid.NewGuid(),
                UsuarioId = vehiculoDTO.UsuarioId,
                NumeroPlaca = vehiculoDTO.NumeroPlaca,
                FotoVehiculo = vehiculoDTO.FotoVehiculo,
                Marca = vehiculoDTO.Marca,
                Anno = vehiculoDTO.Anno
            };

            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();

            vehiculoDTO.IdVehiculo = vehiculo.IdVehiculo;

            return CreatedAtAction("GetVehiculo", new { id = vehiculo.IdVehiculo }, vehiculoDTO);
        }

        // DELETE: api/Vehiculos/5 (Eliminación física)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehiculo(Guid id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound("Vehículo no encontrado.");
            }

            _context.Vehiculos.Remove(vehiculo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VehiculoExists(Guid id)
        {
            return _context.Vehiculos.Any(e => e.IdVehiculo == id);
        }
    }
}
