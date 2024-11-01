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
                .Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    Cedula = u.Cedula,
                    Nombre = u.Nombre,
                    Apellido1 = u.Apellido1,
                    Apellido2 = u.Apellido2,
                    Email = u.Email,
                    FechaNacimiento = u.FechaNacimiento.ToString(),
                    Telefono = u.Telefono,
                    FotoPerfil = u.FotoPerfil,
                    Estado = u.Estado
                })
                .ToListAsync();

            return usuarios;
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
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
                    FechaNacimiento = u.FechaNacimiento.ToString(),
                    Email = u.Email,
                    Telefono = u.Telefono,
                    FotoPerfil = u.FotoPerfil,
                    Estado = u.Estado
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // Obtener usuario por cédula - GET: api/Usuarios/cedula/{cedula}
        [HttpGet("cedula/{cedula}")]
        public async Task<ActionResult<UsuarioDTO>> GetUsuarioPorCedula(string cedula)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.Estado && u.Cedula == cedula)
                .Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    Cedula = u.Cedula,
                    Nombre = u.Nombre,
                    Apellido1 = u.Apellido1,
                    Apellido2 = u.Apellido2,
                    FechaNacimiento = u.FechaNacimiento.ToString(),
                    Email = u.Email,
                    Telefono = u.Telefono,
                    FotoPerfil = u.FotoPerfil,
                    Estado = u.Estado
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // Obtener usuarios con x rol asignado
        [HttpGet("usuarios-por-rol/{rolid}")]
        public async Task<ActionResult<IEnumerable<Rol>>> GetUsuariosPorRol(Guid rolid)
        {
            if (!RolExists(rolid))
            {
                return NotFound("Usuario no encontrado.");
            }

            var roles = await _context.Roles
                .FromSqlRaw("EXEC sp_GetUsuariosPorRol @Rol_idRol = {0}", rolid)
                .ToListAsync();

            return roles;
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
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
            existingUsuario.FechaNacimiento = DateOnly.Parse(usuarioDTO.FechaNacimiento);
            existingUsuario.Telefono = usuarioDTO.Telefono;
            existingUsuario.FotoPerfil = usuarioDTO.FotoPerfil;

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
                FechaNacimiento = DateOnly.Parse(usuarioDTO.FechaNacimiento),
                Telefono = usuarioDTO.Telefono,
                FotoPerfil = usuarioDTO.FotoPerfil,
                Estado = true,
                ContrasennaHash = Encrypt.GetSHA256(usuarioDTO.ContrasennaHash)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            usuarioDTO.IdUsuario = usuario.IdUsuario;

            return CreatedAtAction("GetUsuario", new { id = usuario.IdUsuario }, usuarioDTO);
        }

        // Método para cambiar el estado de un usuario (1.Activo - 0.Inactivo)
        [HttpPut("{id}/cambiar-estado/{estado}")]
        public async Task<IActionResult> CambiarEstado(Guid id, int estado)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Estado = Convert.ToBoolean(estado);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Activar el Doble Factor de Autenticación - PUT: api/Usuarios/5/activar-2fa
        [HttpPut("{id}/activar-2fa")]
        public async Task<IActionResult> ActivarDobleFactor(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.DobleFactorActivo = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Desactivar el Doble Factor de Autenticación - PUT: api/Usuarios/5/desactivar-2fa
        [HttpPut("{id}/desactivar-2fa")]
        public async Task<IActionResult> DesactivarDobleFactor(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.DobleFactorActivo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }



        // POST: api/Usuarios/login
        //[HttpPost("login")]
        //public async Task<ActionResult> Login([FromBody] LogInDTO loginDTO)
        //{
        //    var usuario = await _context.Usuarios
        //        .FirstOrDefaultAsync(u => u.Email == loginDTO.Email);

        //    if (usuario == null || !usuario.Estado)
        //    {
        //        return Unauthorized("Usuario no encontrado o inactivo.");
        //    }

        //    // Verificar la contraseña hasheada usando SHA-256
        //    var hashedPassword = Encrypt.GetSHA256(loginDTO.Password);
        //    if (usuario.ContrasennaHash != hashedPassword)
        //    {
        //        return Unauthorized("Contraseña incorrecta.");
        //    }

        //    // Generar token JWT
        //    var token = GenerateJwtToken(usuario);
        //    return Ok(new { Token = token });
        //}



        // Método privado para generar el token JWT
        //private string GenerateJwtToken(Usuario usuario)
        //{
        //    var claims = new[]
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
        //        new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //    };

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["Jwt:Issuer"],
        //        audience: _configuration["Jwt:Audience"],
        //        claims: claims,
        //        expires: DateTime.Now.AddHours(1),
        //        signingCredentials: creds);

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        private bool UsuarioExists(Guid id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }

        private bool RolExists(Guid id)
        {
            return _context.Roles.Any(e => e.IdRol == id);
        }

        // POST: api/Usuarios/Inicializar
        [HttpPost("Inicializar")]
        public async Task<ActionResult> InicializarUsuarios()
        {
            var usuariosIniciales = new List<UsuarioDTO>
            {
                new UsuarioDTO { Cedula = "123456789", Nombre = "Jimmy", Apellido1 = "Bogantes", Apellido2 = "Rodriguez", Email = "admin@nextek.com", Telefono = "87587272", ContrasennaHash = "pass123" },
                new UsuarioDTO { Cedula = "218860349", Nombre = "Carlos", Apellido1 = "Gomez", Apellido2 = "Lopez", Email = "carlosg@gmail.com", Telefono = "83123456", ContrasennaHash = "pass123" },
                new UsuarioDTO { Cedula = "318860349", Nombre = "Maria", Apellido1 = "Perez", Apellido2 = "Jimenez", Email = "mariap@gmail.com", Telefono = "83234567", ContrasennaHash = "pass123" },
                new UsuarioDTO { Cedula = "418860349", Nombre = "Juan", Apellido1 = "Rojas", Apellido2 = "Mora", Email = "juanr@gmail.com", Telefono = "83345678", ContrasennaHash = "pass123" },
                new UsuarioDTO { Cedula = "518860349", Nombre = "Luis", Apellido1 = "Chacon", Apellido2 = "Soto", Email = "luiss@nextek.com", Telefono = "83456789", ContrasennaHash = "pass123" },
                new UsuarioDTO { Cedula = "618860349", Nombre = "Sofia", Apellido1 = "Castro", Apellido2 = "Vargas", Email = "sofiac@nextek.com", Telefono = "83567890", ContrasennaHash = "pass123" },
                new UsuarioDTO { Cedula = "718860349", Nombre = "Andres", Apellido1 = "Vega", Apellido2 = "Quesada", Email = "andresv@nextek.com", Telefono = "83678901", ContrasennaHash = "pass123" },
                new UsuarioDTO { Cedula = "118860349", Nombre = "Laura", Apellido1 = "Solis", Apellido2 = "Cruz", Email = "lauras@nextek.com", Telefono = "83789012", ContrasennaHash = "pass123" },
                new UsuarioDTO { Cedula = "228860349", Nombre = "Diego", Apellido1 = "Morales", Apellido2 = "Ulate", Email = "diegom@nextek.com", Telefono = "83890123", ContrasennaHash = "pass123" },
                new UsuarioDTO { Cedula = "338860349", Nombre = "Ana", Apellido1 = "Herrera", Apellido2 = "Diaz", Email = "anah@nextek.com", Telefono = "83901234", ContrasennaHash = "pass123" }
            };

            foreach (var usuarioDTO in usuariosIniciales)
            {
                // Verificar si el email ya existe
                if (_context.Usuarios.Any(u => u.Email == usuarioDTO.Email))
                {
                    continue; // Saltar si el email ya está registrado
                }

                // Crear el usuario con los datos y hash de contraseña
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
                    ContrasennaHash = Encrypt.GetSHA256("pass123") // Hash de "pass123"
                };

                _context.Usuarios.Add(usuario);
            }

            await _context.SaveChangesAsync();

            return Ok("Usuarios iniciales agregados exitosamente.");
        }

    }
}
