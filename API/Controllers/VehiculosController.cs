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
    public class VehiculosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehiculosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Vehiculos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehiculo>>> GetVehiculos()
        {
            return await _context.Vehiculos
                .ToListAsync();
        }

        // GET: api/Vehiculos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vehiculo>> GetVehiculo(Guid id)
        {
            var vehiculo = await _context.Vehiculos
                .FromSqlRaw("EXEC sp_obtenerVehiculoPorId @id = {0}", id)
                .FirstOrDefaultAsync();

            if (vehiculo == null)
            {
                return NotFound();
            }

            return vehiculo;
        }

        // GET: api/Vehiculos/usuario/5
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Vehiculo>>> GetVehiculosPorUsuario(Guid usuarioId)
        {
            var vehiculos = await _context.Vehiculos
                .FromSqlRaw("EXEC sp_obtenerVehiculosPorUsuario @Usuario_idUsuario = {0}", usuarioId)
                .ToListAsync();

            if (vehiculos == null || vehiculos.Count == 0)
            {
                return NotFound();
            }

            return vehiculos;
        }

        // PUT: api/Vehiculos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVehiculo(Guid id, Vehiculo vehiculo)
        {
            if (id != vehiculo.IdVehiculo)
            {
                return BadRequest("El ID proporcionado no coincide con el vehículo.");
            }

            var existingVehiculo = await _context.Vehiculos.FindAsync(id);
            if (existingVehiculo == null)
            {
                return NotFound();
            }

            existingVehiculo.NumeroPlaca = vehiculo.NumeroPlaca;
            existingVehiculo.FotoVehiculo = vehiculo.FotoVehiculo;
            existingVehiculo.Marca = vehiculo.Marca;
            existingVehiculo.Anno = vehiculo.Anno;

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
        public async Task<ActionResult<Vehiculo>> PostVehiculo(Vehiculo vehiculo)
        {
            // Verificar si la placa ya existe utilizando el stored procedure
            var placaExistente = await _context.Vehiculos
                .FromSqlRaw("EXEC sp_obtenerVehiculoPorPlaca @numero_placa = {0}", vehiculo.NumeroPlaca)
                .FirstOrDefaultAsync();

            if (placaExistente != null)
            {
                return Conflict("El número de placa ya está registrado.");
            }

            vehiculo.IdVehiculo = Guid.NewGuid();
            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVehiculo", new { id = vehiculo.IdVehiculo }, vehiculo);
        }

        // DELETE: api/Vehiculos/5 (Eliminación lógica)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehiculo(Guid id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            // En lugar de eliminar, cambiar estado o hacer una eliminación lógica si es necesario
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
