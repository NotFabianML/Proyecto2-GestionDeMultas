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
    public class PermisosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermisosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Permisos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PermisoDTO>>> GetPermisos()
        {
            var permisos = await _context.Permisos
                .Where(p => p.Estado)
                .Select(p => new PermisoDTO
                {
                    IdPermiso = p.IdPermiso,
                    NombrePermiso = p.NombrePermiso,
                    Descripcion = p.Descripcion,
                    Estado = p.Estado
                })
                .ToListAsync();

            return permisos;
        }

        // GET: api/Permisos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PermisoDTO>> GetPermiso(Guid id)
        {
            var permiso = await _context.Permisos
                .Where(p => p.IdPermiso == id && p.Estado)
                .Select(p => new PermisoDTO
                {
                    IdPermiso = p.IdPermiso,
                    NombrePermiso = p.NombrePermiso,
                    Descripcion = p.Descripcion,
                    Estado = p.Estado
                })
                .FirstOrDefaultAsync();

            if (permiso == null)
            {
                return NotFound("Permiso no encontrado.");
            }

            return permiso;
        }

        // PUT: api/Permisos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermiso(Guid id, PermisoDTO permisoDTO)
        {
            if (id != permisoDTO.IdPermiso)
            {
                return BadRequest("El ID proporcionado no coincide con el permiso.");
            }

            var existingPermiso = await _context.Permisos.FindAsync(id);
            if (existingPermiso == null)
            {
                return NotFound("Permiso no encontrado.");
            }

            existingPermiso.NombrePermiso = permisoDTO.NombrePermiso;
            existingPermiso.Descripcion = permisoDTO.Descripcion;
            existingPermiso.Estado = permisoDTO.Estado;

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
        public async Task<ActionResult<PermisoDTO>> PostPermiso(PermisoDTO permisoDTO)
        {
            if (_context.Permisos.Any(p => p.NombrePermiso == permisoDTO.NombrePermiso))
            {
                return Conflict("El nombre del permiso ya está registrado.");
            }

            var permiso = new Permiso
            {
                IdPermiso = Guid.NewGuid(),
                NombrePermiso = permisoDTO.NombrePermiso,
                Descripcion = permisoDTO.Descripcion,
                Estado = true
            };

            _context.Permisos.Add(permiso);
            await _context.SaveChangesAsync();

            permisoDTO.IdPermiso = permiso.IdPermiso;

            return CreatedAtAction("GetPermiso", new { id = permiso.IdPermiso }, permisoDTO);
        }

        // DELETE: api/Permisos/5 (Eliminación lógica)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermiso(Guid id)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null)
            {
                return NotFound("Permiso no encontrado.");
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

        // Asignar permiso a un rol
        [HttpPost("{id}/roles/{rolId}")]
        public async Task<IActionResult> AsignarPermisoARol(Guid id, Guid rolId)
        {
            if (!PermisoExists(id) || !_context.Roles.Any(r => r.IdRol == rolId))
            {
                return NotFound("Permiso o rol no encontrado.");
            }

            var permisoAsignado = await _context.RolPermisos
                .AnyAsync(rp => rp.RolId == rolId && rp.PermisoId == id);

            if (permisoAsignado)
            {
                return BadRequest("El rol ya tiene asignado este permiso.");
            }

            // Ejecutar el stored procedure para asignar el permiso al rol
            await _context.Database.ExecuteSqlRawAsync("EXEC sp_asignarPermisoARol @Permiso_idPermiso = {0}, @Rol_idRol = {1}", id, rolId);

            return Ok("Permiso asignado al rol.");
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
