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
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UsuariosController(UserManager<IdentityUser> userManager, AppDbContext context, IConfiguration configuration)
        {
            _userManager = userManager;
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
                    Estado = u.Estado,
                    DobleFactorActivo = u.DobleFactorActivo,
                    Roles = u.UsuarioRoles
                        .Select(ur => ur.Rol.NombreRol) // Asume que la relación UsuarioRoles contiene el rol y que este tiene un campo NombreRol
                        .ToList()
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
                    Estado = u.Estado,
                    DobleFactorActivo = u.DobleFactorActivo
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

        // Obtener usuario por correo - GET: api/Usuarios/verificar-correo/{email}
        [HttpGet("verificar-correo/{email}")]
        public async Task<ActionResult> VerificarCorreoUnico(string email)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.Email == email)
                .Select(u => new { u.IdUsuario })
                .FirstOrDefaultAsync();

            if (usuario != null)
            {
                return Ok(new
                {
                    Existe = true,
                    IdUsuario = usuario.IdUsuario
                });
            }

            return Ok(new
            {
                Existe = false,
                IdUsuario = (Guid?)null
            });
        }


        // Obtener usuarios por rol - reemplazo de sp_GetUsuariosPorRol
        [HttpGet("usuarios-por-rol/{rolId}")]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetUsuariosPorRol(Guid rolId)
        {
            var usuarios = await _context.Usuarios
                .Where(u => u.UsuarioRoles.Any(ur => ur.RolId == rolId))
                .Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    Cedula = u.Cedula,
                    Nombre = u.Nombre,
                    Apellido1 = u.Apellido1,
                    Apellido2 = u.Apellido2,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    Estado = u.Estado,
                    FotoPerfil = u.FotoPerfil
                })
                .ToListAsync();

            if (!usuarios.Any())
            {
                return NotFound("No se encontraron usuarios con el rol especificado.");
            }

            return usuarios;
        }

        // Obtener usuarios por nombre de rol
        [HttpGet("usuarios-por-rol-nombre/{rolNombre}")]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetUsuariosPorNombreRol(string rolNombre)
        {
            // Buscar el rol por su nombre para obtener su ID
            var rol = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol.ToLower() == rolNombre.ToLower());
            if (rol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            var usuarios = await _context.Usuarios
                .Where(u => u.UsuarioRoles.Any(ur => ur.RolId == rol.IdRol))
                .Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    Cedula = u.Cedula,
                    Nombre = u.Nombre,
                    Apellido1 = u.Apellido1,
                    Apellido2 = u.Apellido2,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    Estado = u.Estado,
                    FotoPerfil = u.FotoPerfil
                })
                .ToListAsync();

            if (!usuarios.Any())
            {
                return NotFound("No se encontraron usuarios con el rol especificado.");
            }

            return usuarios;
        }


        //// PUT: api/Usuarios/5
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutUsuario(Guid id, UsuarioDTO usuarioDTO)
        //{
        //    if (id != usuarioDTO.IdUsuario)
        //    {
        //        return BadRequest("El ID proporcionado no coincide con el usuario.");
        //    }

        //    var existingUsuario = await _context.Usuarios.FindAsync(id);
        //    if (existingUsuario == null)
        //    {
        //        return NotFound("Usuario no encontrado.");
        //    }

        //    // Actualizar datos en la tabla personalizada y en Identity
        //    existingUsuario.Nombre = usuarioDTO.Nombre;
        //    existingUsuario.Apellido1 = usuarioDTO.Apellido1;
        //    existingUsuario.Apellido2 = usuarioDTO.Apellido2;
        //    existingUsuario.Email = usuarioDTO.Email;
        //    existingUsuario.FechaNacimiento = DateOnly.ParseExact(usuarioDTO.FechaNacimiento, "dd-MM-yyyy");
        //    existingUsuario.Telefono = usuarioDTO.Telefono;
        //    existingUsuario.FotoPerfil = usuarioDTO.FotoPerfil;

        //    _context.Entry(existingUsuario).State = EntityState.Modified;

        //    var identityUser = await _userManager.FindByIdAsync(existingUsuario.UserId);
        //    if (identityUser != null)
        //    {
        //        identityUser.Email = usuarioDTO.Email;
        //        identityUser.UserName = usuarioDTO.Email;
        //        await _userManager.UpdateAsync(identityUser);
        //    }

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!UsuarioExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

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
                return NotFound("Usuario no encontrado.");
            }

            // Actualizar los datos en la tabla personalizada y en Identity
            existingUsuario.Nombre = usuarioDTO.Nombre;
            existingUsuario.Apellido1 = usuarioDTO.Apellido1;
            existingUsuario.Apellido2 = usuarioDTO.Apellido2;
            existingUsuario.Email = usuarioDTO.Email;
            existingUsuario.FechaNacimiento = DateOnly.ParseExact(usuarioDTO.FechaNacimiento, "dd-MM-yyyy");
            existingUsuario.Telefono = usuarioDTO.Telefono;
            existingUsuario.FotoPerfil = usuarioDTO.FotoPerfil;
            existingUsuario.Estado = usuarioDTO.Estado;

            // Solo actualizar ContrasennaHash si se proporciona
            if (!string.IsNullOrEmpty(usuarioDTO.ContrasennaHash))
            {
                existingUsuario.ContrasennaHash = usuarioDTO.ContrasennaHash;
            }

            _context.Entry(existingUsuario).State = EntityState.Modified;

            var identityUser = await _userManager.FindByIdAsync(existingUsuario.UserId);
            if (identityUser != null)
            {
                identityUser.Email = usuarioDTO.Email;
                identityUser.UserName = usuarioDTO.Email;
                await _userManager.UpdateAsync(identityUser);
            }

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

            // Crear el usuario en Identity
            var identityUser = new IdentityUser { Email = usuarioDTO.Email, UserName = usuarioDTO.Email };
            var createResult = await _userManager.CreateAsync(identityUser, usuarioDTO.ContrasennaHash);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return BadRequest($"Error al crear el usuario en Identity: {errors}");
            }

            // Crear el usuario en la tabla personalizada
            var usuario = new Usuario
            {
                IdUsuario = Guid.NewGuid(),
                UserId = identityUser.Id, // Relacionar con AspNetUsers
                Cedula = usuarioDTO.Cedula,
                Nombre = usuarioDTO.Nombre,
                Apellido1 = usuarioDTO.Apellido1,
                Apellido2 = usuarioDTO.Apellido2,
                Email = usuarioDTO.Email,
                FechaNacimiento = DateOnly.ParseExact(usuarioDTO.FechaNacimiento, "dd-MM-yyyy"),
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

        // Endpoint para subir la foto de perfil
        [HttpPost("{id}/subir-fotoPerfil")]
        public async Task<IActionResult> UploadProfilePhoto(Guid id, IFormFile photo)
        {
            // Validación: verificar si el archivo fue recibido
            if (photo == null || photo.Length == 0)
            {
                return BadRequest("Archivo de imagen no válido.");
            }

            // Buscar el usuario en la base de datos
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            // Generar el nombre del archivo y la ruta
            var fileName = $"{id}_profile_{Path.GetFileName(photo.FileName)}";
            var filePath = Path.Combine("wwwroot/uploads/profile_photos", fileName);

            // Guardar la imagen en el sistema de archivos
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            // Actualizar la URL de la foto de perfil en la base de datos
            usuario.FotoPerfil = $"/uploads/profile_photos/{fileName}";
            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Foto de perfil subida exitosamente.", url = usuario.FotoPerfil });
        }

        // Eliminar Usuario (sincronizado con Identity)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var identityUser = await _userManager.FindByIdAsync(usuario.UserId);
            if (identityUser != null)
            {
                await _userManager.DeleteAsync(identityUser);
            }

            //_context.Usuarios.Remove(usuario);
            //await _context.SaveChangesAsync();

            // Retornar los datos del usuario eliminado
            var usuarioEliminado = new
            {
                usuario.IdUsuario,
                usuario.Nombre,
                usuario.Apellido1,
                usuario.Apellido2,
                usuario.Email
            };

            return Ok(usuarioEliminado);
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
                new UsuarioDTO { Cedula = "123456789", Nombre = "Jimmy", Apellido1 = "Bogantes", Apellido2 = "Rodriguez", Email = "admin@nextek.com", Telefono = "87587272", ContrasennaHash = "Password123!", FechaNacimiento = "10-05-1985" },
                new UsuarioDTO { Cedula = "218860349", Nombre = "Carlos", Apellido1 = "Gomez", Apellido2 = "Lopez", Email = "carlosg@gmail.com", Telefono = "83123456", ContrasennaHash = "Password123!", FechaNacimiento = "15-08-1990" },
                new UsuarioDTO { Cedula = "318860349", Nombre = "Maria", Apellido1 = "Perez", Apellido2 = "Jimenez", Email = "mariap@gmail.com", Telefono = "83234567", ContrasennaHash = "Password123!", FechaNacimiento = "12-03-1992" },
                new UsuarioDTO { Cedula = "418860349", Nombre = "Juan", Apellido1 = "Rojas", Apellido2 = "Mora", Email = "juanr@gmail.com", Telefono = "83345678", ContrasennaHash = "Password123!", FechaNacimiento = "20-12-1988" },
                new UsuarioDTO { Cedula = "518860349", Nombre = "Luis", Apellido1 = "Chacon", Apellido2 = "Soto", Email = "luiss@nextek.com", Telefono = "83456789", ContrasennaHash = "Password123!", FechaNacimiento = "22-04-1995" },
                new UsuarioDTO { Cedula = "618860349", Nombre = "Sofia", Apellido1 = "Castro", Apellido2 = "Vargas", Email = "sofiac@nextek.com", Telefono = "83567890", ContrasennaHash = "Password123!", FechaNacimiento = "17-09-1997" },
                new UsuarioDTO { Cedula = "718860349", Nombre = "Andres", Apellido1 = "Vega", Apellido2 = "Quesada", Email = "andresv@nextek.com", Telefono = "83678901", ContrasennaHash = "Password123!", FechaNacimiento = "05-06-1993" },
                new UsuarioDTO { Cedula = "118860349", Nombre = "Laura", Apellido1 = "Solis", Apellido2 = "Cruz", Email = "lauras@nextek.com", Telefono = "83789012", ContrasennaHash = "Password123!", FechaNacimiento = "11-01-1989" },
                new UsuarioDTO { Cedula = "228860349", Nombre = "Diego", Apellido1 = "Morales", Apellido2 = "Ulate", Email = "diegom@nextek.com", Telefono = "83890123", ContrasennaHash = "Password123!", FechaNacimiento = "30-07-1998" },
                new UsuarioDTO { Cedula = "338860349", Nombre = "Ana", Apellido1 = "Herrera", Apellido2 = "Diaz", Email = "anah@nextek.com", Telefono = "83901234", ContrasennaHash = "Password123!", FechaNacimiento = "25-11-1996" }
            };

            foreach (var usuarioDTO in usuariosIniciales)
            {
                // Verificar si el email ya existe en AspNetUsers
                var usuarioIdentity = await _userManager.FindByEmailAsync(usuarioDTO.Email);
                if (usuarioIdentity == null)
                {
                    // Crear el usuario en AspNetUsers
                    usuarioIdentity = new IdentityUser { Email = usuarioDTO.Email, UserName = usuarioDTO.Cedula };
                    var createResult = await _userManager.CreateAsync(usuarioIdentity, usuarioDTO.ContrasennaHash);

                    // Manejar errores específicos de creación en AspNetUsers
                    if (!createResult.Succeeded)
                    {
                        foreach (var error in createResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, $"Error al crear usuario en AspNetUsers: {error.Description}");
                        }
                        continue; // Saltar la creación en Usuarios si falla en AspNetUsers
                    }
                }

                // Verificar si el usuario ya existe en la tabla Usuarios
                if (_context.Usuarios.Any(u => u.Email == usuarioDTO.Email))
                {
                    continue; // Saltar si el usuario ya está en la tabla Usuarios
                }

                // Crear el usuario en la tabla Usuarios
                var usuarioDatos = new Usuario
                {
                    IdUsuario = Guid.NewGuid(),
                    UserId = usuarioIdentity.Id, // Relacionar con AspNetUsers
                    Cedula = usuarioDTO.Cedula,
                    Nombre = usuarioDTO.Nombre,
                    Apellido1 = usuarioDTO.Apellido1,
                    Apellido2 = usuarioDTO.Apellido2,
                    Email = usuarioDTO.Email,
                    Telefono = usuarioDTO.Telefono,
                    Estado = true,
                    ContrasennaHash = Encrypt.GetSHA256(usuarioDTO.ContrasennaHash),
                    FechaNacimiento = DateOnly.ParseExact(usuarioDTO.FechaNacimiento, "dd-MM-yyyy")
                };

                _context.Usuarios.Add(usuarioDatos);
            }

            await _context.SaveChangesAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Retornar errores si existen
            }

            return Ok("Usuarios iniciales agregados exitosamente.");
        }

    }
}
