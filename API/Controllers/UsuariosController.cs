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
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios
                .Where(u => u.Estado)
                .ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(Guid id)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.Estado && u.IdUsuario == id)
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(Guid id, Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return BadRequest("El ID proporcionado no coincide con el usuario.");
            }

            var existingUsuario = await _context.Usuarios.FindAsync(id);
            if (existingUsuario == null)
            {
                return NotFound();
            }

            existingUsuario.Nombre = usuario.Nombre;
            existingUsuario.Apellido1 = usuario.Apellido1;
            existingUsuario.Apellido2 = usuario.Apellido2;
            existingUsuario.Email = usuario.Email;

            _context.Entry(existingUsuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
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

        // POST: api/Usuarios
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            if (_context.Usuarios.Any(u => u.Email == usuario.Email))
            {
                return Conflict("El email ya está registrado.");
            }

            usuario.IdUsuario = Guid.NewGuid();
            usuario.Estado = true;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.IdUsuario }, usuario);
        }

        // DELETE: api/Usuarios/5 (Eliminación lógica)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Estado = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Asignar rol a usuario usando el SP
        [HttpPost("{id}/roles/{rolId}")]
        public async Task<IActionResult> AsignarRol(Guid id, Guid rolId)
        {
            if (!UsuarioExists(id) || !_context.Roles.Any(r => r.IdRol == rolId))
            {
                return NotFound("Usuario o rol no encontrado.");
            }

            var result = await _context.Database.ExecuteSqlRawAsync("EXEC sp_insertarUsuarioXRol @Usuario_idUsuario = {0}, @Rol_idRol = {1}", id, rolId);
            if (result == 0)
            {
                return BadRequest("Error al asignar el rol al usuario.");
            }

            return NoContent();
        }

        // Obtener roles asignados a un usuario
        [HttpGet("{id}/roles")]
        public async Task<ActionResult<IEnumerable<Rol>>> GetRolesPorUsuario(Guid id)
        {
            if (!UsuarioExists(id))
            {
                return NotFound("Usuario no encontrado.");
            }

            var roles = await _context.Roles
                .FromSqlRaw("EXEC sp_obtenerRolesPorUsuario @Usuario_idUsuario = {0}", id)
                .ToListAsync();

            return roles;
        }

        // Eliminar rol de usuario usando el SP
        [HttpDelete("{id}/roles/{rolId}")]
        public async Task<IActionResult> DeleteRolDeUsuario(Guid id, Guid rolId)
        {
            if (!UsuarioExists(id) || !_context.Roles.Any(r => r.IdRol == rolId))
            {
                return NotFound("Usuario o rol no encontrado.");
            }

            var result = await _context.Database.ExecuteSqlRawAsync("EXEC sp_eliminarUsuarioXRol @Usuario_idUsuario = {0}, @Rol_idRol = {1}", id, rolId);
            if (result == 0)
            {
                return BadRequest("Error al eliminar el rol del usuario.");
            }

            return NoContent();
        }

        // Activar dosFactor
        [HttpPost("{id}/activar-2fa")]
        public async Task<IActionResult> ActivarDosFactor(Guid id, [FromBody] string dosFactorSecret)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            usuario.DosFactorActivo = true;
            usuario.DosFactorSecret = dosFactorSecret;
            await _context.SaveChangesAsync();

            return Ok("2FA activado.");
        }

        // Desactivar dosFactor
        [HttpPost("{id}/desactivar-2fa")]
        public async Task<IActionResult> DesactivarDosFactor(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            usuario.DosFactorActivo = false;
            usuario.DosFactorSecret = null;
            await _context.SaveChangesAsync();

            return Ok("2FA desactivado.");
        }

        private bool UsuarioExists(Guid id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}
