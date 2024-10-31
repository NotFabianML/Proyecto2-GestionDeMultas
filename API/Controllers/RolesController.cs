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
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RolDTO>>> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new RolDTO
                {
                    IdRol = r.IdRol,
                    NombreRol = r.NombreRol,
                    Descripcion = r.Descripcion
                })
                .ToListAsync();

            return roles;
        }

        // GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RolDTO>> GetRol(Guid id)
        {
            var rol = await _context.Roles
                .Where(r => r.IdRol == id)
                .Select(r => new RolDTO
                {
                    IdRol = r.IdRol,
                    NombreRol = r.NombreRol,
                    Descripcion = r.Descripcion
                })
                .FirstOrDefaultAsync();

            if (rol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            return rol;
        }

        // PUT: api/Roles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRol(Guid id, RolDTO rolDTO)
        {
            if (id != rolDTO.IdRol)
            {
                return BadRequest("El ID proporcionado no coincide con el rol.");
            }

            var existingRol = await _context.Roles.FindAsync(id);
            if (existingRol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            existingRol.NombreRol = rolDTO.NombreRol;
            existingRol.Descripcion = rolDTO.Descripcion;

            _context.Entry(existingRol).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RolExists(id))
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

        // POST: api/Roles
        [HttpPost]
        public async Task<ActionResult<RolDTO>> PostRol(RolDTO rolDTO)
        {
            if (_context.Roles.Any(r => r.NombreRol == rolDTO.NombreRol))
            {
                return Conflict("El nombre del rol ya está registrado.");
            }

            var rol = new Rol
            {
                IdRol = Guid.NewGuid(),
                NombreRol = rolDTO.NombreRol,
                Descripcion = rolDTO.Descripcion
            };

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            rolDTO.IdRol = rol.IdRol;

            return CreatedAtAction("GetRol", new { id = rol.IdRol }, rolDTO);
        }

        // DELETE: api/Roles/5 (Eliminación física)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRol(Guid id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Método para asignar permiso a un rol
        [HttpPost("{id}/permisos/{permisoId}")]
        public async Task<IActionResult> AsignarPermiso(Guid id, Guid permisoId)
        {
            if (!RolExists(id) || !_context.Permisos.Any(p => p.IdPermiso == permisoId))
            {
                return NotFound("Rol o permiso no encontrado.");
            }

            if (_context.RolPermisos.Any(rp => rp.RolId == id && rp.PermisoId == permisoId))
            {
                return BadRequest("El rol ya tiene asignado este permiso.");
            }

            _context.RolPermisos.Add(new RolXPermiso { RolId = id, PermisoId = permisoId });
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Obtener permisos asignados a un rol
        [HttpGet("{id}/permisos")]
        public async Task<ActionResult<IEnumerable<Permiso>>> GetPermisosPorRol(Guid id)
        {
            if (!RolExists(id))
            {
                return NotFound("Rol no encontrado.");
            }

            var permisos = await _context.Permisos
                .FromSqlRaw("EXEC sp_obtenerPermisosPorRol @Rol_idRol = {0}", id)
                .ToListAsync();

            return permisos;
        }

        // Eliminar permiso de un rol
        [HttpDelete("{id}/permisos/{permisoId}")]
        public async Task<IActionResult> DeletePermisoDeRol(Guid id, Guid permisoId)
        {
            if (!RolExists(id) || !_context.Permisos.Any(p => p.IdPermiso == permisoId))
            {
                return NotFound("Rol o permiso no encontrado.");
            }

            var result = await _context.Database.ExecuteSqlRawAsync("EXEC sp_eliminarRolXPermiso @Rol_idRol = {0}, @Permiso_idPermiso = {1}", id, permisoId);
            if (result == 0)
            {
                return BadRequest("Error al eliminar el permiso del rol.");
            }

            return NoContent();
        }

        private bool RolExists(Guid id)
        {
            return _context.Roles.Any(e => e.IdRol == id);
        }
    }
}
