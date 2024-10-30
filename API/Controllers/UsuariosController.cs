using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using DataAccess.EF;
using DataAccess.EF.Models;
using DTO;
using BusinessLogic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UsuariosController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Usuarios
        [HttpGet]
        //[Authorize]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Where(u => u.Estado)
                .Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    Cedula = u.Cedula,
                    Nombre = u.Nombre,
                    Apellido1 = u.Apellido1,
                    Apellido2 = u.Apellido2,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    Estado = u.Estado
                })
                .ToListAsync();

            return usuarios;
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UsuarioDTO>> GetUsuario(Guid id)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.Estado && u.IdUsuario == id)
                .Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    Cedula = u.Cedula,
                    Nombre = u.Nombre,
                    Apellido1 = u.Apellido1,
                    Apellido2 = u.Apellido2,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    Estado = u.Estado
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUsuario(Guid id, UsuarioDTO usuarioDTO)
        {
            if (id != usuarioDTO.IdUsuario)
            {
                return BadRequest("El ID proporcionado no coincide con el usuario.");
            }

            var existingUsuario = await _context.Usuarios.FindAsync(id);
            if (existingUsuario == null)
            {
                return NotFound();
            }

            existingUsuario.Nombre = usuarioDTO.Nombre;
            existingUsuario.Apellido1 = usuarioDTO.Apellido1;
            existingUsuario.Apellido2 = usuarioDTO.Apellido2;
            existingUsuario.Email = usuarioDTO.Email;
            existingUsuario.Telefono = usuarioDTO.Telefono;

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
        public async Task<ActionResult<UsuarioDTO>> PostUsuario(UsuarioDTO usuarioDTO)
        {
            if (_context.Usuarios.Any(u => u.Email == usuarioDTO.Email))
            {
                return Conflict("El email ya está registrado.");
            }

            var usuario = new Usuario
            {
                IdUsuario = Guid.NewGuid(),
                Cedula = usuarioDTO.Cedula,
                Nombre = usuarioDTO.Nombre,
                Apellido1 = usuarioDTO.Apellido1,
                Apellido2 = usuarioDTO.Apellido2,
                Email = usuarioDTO.Email,
                Telefono = usuarioDTO.Telefono,
                Estado = true,
                ContrasennaHash = Encrypt.GetSHA256(usuarioDTO.ContrasennaHash)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            usuarioDTO.IdUsuario = usuario.IdUsuario;

            return CreatedAtAction("GetUsuario", new { id = usuario.IdUsuario }, usuarioDTO);
        }

        // DELETE: api/Usuarios/5 (Eliminación lógica)
        [HttpDelete("{id}")]
        //[Authorize]
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

        // POST: api/Usuarios/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == loginDTO.Email);

            if (usuario == null || !usuario.Estado)
            {
                return Unauthorized("Usuario no encontrado o inactivo.");
            }

            // Verificar la contraseña hasheada usando SHA-256
            var hashedPassword = Encrypt.GetSHA256(loginDTO.Password);
            if (usuario.ContrasennaHash != hashedPassword)
            {
                return Unauthorized("Contraseña incorrecta.");
            }

            // Generar token JWT
            var token = GenerateJwtToken(usuario);
            return Ok(new { Token = token });
        }

        // Asignar rol a usuario usando el SP
        [HttpPost("{id}/roles/{rolId}")]
        [Authorize]
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

        // Obtener roles asignados a un usuario usando el SP
        [HttpGet("{id}/roles")]
        [Authorize]
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
        [Authorize]
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

        // Método privado para generar el token JWT
        private string GenerateJwtToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool UsuarioExists(Guid id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}
