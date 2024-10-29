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
    public class PermisosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermisosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Permisos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permiso>>> GetPermisos()
        {
            return await _context.Permisos.ToListAsync();
        }

        // GET: api/Permisos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permiso>> GetPermiso(Guid id)
        {
            var permiso = await _context.Permisos.FindAsync(id);

            if (permiso == null)
            {
                return NotFound();
            }

            return permiso;
        }

        // PUT: api/Permisos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermiso(Guid id, Permiso permiso)
        {
            if (id != permiso.IdPermiso)
            {
                return BadRequest("El ID proporcionado no coincide con el permiso.");
            }

            var existingPermiso = await _context.Permisos.FindAsync(id);
            if (existingPermiso == null)
            {
                return NotFound();
            }

            existingPermiso.NombrePermiso = permiso.NombrePermiso;
            existingPermiso.Descripcion = permiso.Descripcion;
            existingPermiso.Estado = permiso.Estado;

            _context.Entry(existingPermiso).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PermisoExists(id))
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

        // POST: api/Permisos
        [HttpPost]
        public async Task<ActionResult<Permiso>> PostPermiso(Permiso permiso)
        {
            if (_context.Permisos.Any(p => p.NombrePermiso == permiso.NombrePermiso))
            {
                return Conflict("El nombre del permiso ya está registrado.");
            }

            permiso.IdPermiso = Guid.NewGuid();
            _context.Permisos.Add(permiso);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPermiso", new { id = permiso.IdPermiso }, permiso);
        }

        // DELETE: api/Permisos/5 (Eliminación lógica)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermiso(Guid id)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null)
            {
                return NotFound();
            }

            permiso.Estado = false; // Cambio a inactivo en lugar de eliminar
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Activar Permiso
        [HttpPost("{id}/activar")]
        public async Task<IActionResult> ActivarPermiso(Guid id)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null)
            {
                return NotFound("Permiso no encontrado.");
            }

            permiso.Estado = true;
            await _context.SaveChangesAsync();

            return Ok("Permiso activado.");
        }

        // Desactivar Permiso
        [HttpPost("{id}/desactivar")]
        public async Task<IActionResult> DesactivarPermiso(Guid id)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null)
            {
                return NotFound("Permiso no encontrado.");
            }

            permiso.Estado = false;
            await _context.SaveChangesAsync();

            return Ok("Permiso desactivado.");
        }

        // Obtener roles asignados a un permiso
        [HttpGet("{id}/roles")]
        public async Task<ActionResult<IEnumerable<Rol>>> GetRolesPorPermiso(Guid id)
        {
            if (!PermisoExists(id))
            {
                return NotFound("Permiso no encontrado.");
            }

            var roles = await _context.Roles
                .FromSqlRaw("EXEC sp_obtenerRolesPorPermiso @Permiso_idPermiso = {0}", id)
                .ToListAsync();

            return roles;
        }

        private bool PermisoExists(Guid id)
        {
            return _context.Permisos.Any(e => e.IdPermiso == id);
        }
    }
}
