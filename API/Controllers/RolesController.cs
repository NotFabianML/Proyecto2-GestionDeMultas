using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.EF;
using DataAccess.EF.Models;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
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

        // Obtener roles asignados a un usuario
        [HttpGet("{id}/roles-por-usuario")]
        public async Task<ActionResult<IEnumerable<RolDTO>>> GetRolesPorUsuario(Guid id)
        {
            if (!UsuarioExists(id))
            {
                return NotFound("Usuario no encontrado.");
            }

            var roles = await _context.Roles
                .Where(r => r.UsuarioRoles.Any(ur => ur.UsuarioId == id))
                .Select(r => new RolDTO
                {
                    IdRol = r.IdRol,
                    NombreRol = r.NombreRol,
                    Descripcion = r.Descripcion
                })
                .ToListAsync();

            return roles;
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
        public async Task<IActionResult> PostRol([FromBody] RolDTO nuevoRol)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Crear el rol en la tabla Rol
            var rol = new Rol
            {
                NombreRol = nuevoRol.NombreRol,
                Descripcion = nuevoRol.Descripcion
            };

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            // También crear el rol en Identity si no existe
            if (!await _roleManager.RoleExistsAsync(nuevoRol.NombreRol))
            {
                await _roleManager.CreateAsync(new IdentityRole(nuevoRol.NombreRol));
            }

            return Ok("Rol creado exitosamente.");
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

        // Asignar rol a usuario
        [HttpPost("{id}/asignar-rol/{rolId}")]
        public async Task<IActionResult> AsignarRol(Guid id, Guid rolId)
        {
            if (!UsuarioExists(id) || !_context.Roles.Any(r => r.IdRol == rolId))
            {
                return NotFound("Usuario o rol no encontrado.");
            }

            // Obtener el usuario y el rol para la relación
            var usuario = await _context.Usuarios.FindAsync(id);
            var rol = await _context.Roles.FindAsync(rolId);

            if (usuario == null || rol == null)
            {
                return NotFound("Usuario o rol no encontrado.");
            }

            // Asignar el rol en la tabla personalizada
            var usuarioRol = new UsuarioXRol
            {
                UsuarioId = usuario.IdUsuario,
                RolId = rol.IdRol
            };

            _context.UsuarioRoles.Add(usuarioRol);
            await _context.SaveChangesAsync();

            // Asignar el rol en Identity
            var usuarioIdentity = await _userManager.FindByIdAsync(usuario.UserId);
            if (usuarioIdentity == null)
            {
                return NotFound("Usuario en Identity no encontrado.");
            }

            var rolNombre = rol.NombreRol; // Obtener el nombre del rol
            var result = await _userManager.AddToRoleAsync(usuarioIdentity, rolNombre);

            if (!result.Succeeded)
            {
                // Revertir la asignación en la tabla personalizada si falla en Identity
                _context.UsuarioRoles.Remove(usuarioRol);
                await _context.SaveChangesAsync();

                return BadRequest("Error al asignar el rol en Identity.");
            }

            return NoContent();
        }

        // Eliminar rol de usuario
        [HttpDelete("{id}/eliminar-rol/{rolId}")]
        public async Task<IActionResult> DeleteRolDeUsuario(Guid id, Guid rolId)
        {
            if (!UsuarioExists(id) || !_context.Roles.Any(r => r.IdRol == rolId))
            {
                return NotFound("Usuario o rol no encontrado.");
            }

            var usuarioRol = await _context.UsuarioRoles
                .FirstOrDefaultAsync(ur => ur.UsuarioId == id && ur.RolId == rolId);

            if (usuarioRol == null)
            {
                return BadRequest("El usuario no tiene asignado el rol especificado.");
            }

            _context.UsuarioRoles.Remove(usuarioRol);
            await _context.SaveChangesAsync();

            // También eliminar el rol en Identity (si es necesario)
            var usuarioIdentity = await _userManager.FindByIdAsync(id.ToString());
            var rolNombre = (await _context.Roles.FirstOrDefaultAsync(r => r.IdRol == rolId))?.NombreRol;

            if (usuarioIdentity != null && rolNombre != null)
            {
                var result = await _userManager.RemoveFromRoleAsync(usuarioIdentity, rolNombre);
                if (!result.Succeeded)
                {
                    return BadRequest("Error al eliminar el rol en Identity.");
                }
            }

            return NoContent();
        }


        private bool RolExists(Guid id)
        {
            return _context.Roles.Any(e => e.IdRol == id);
        }

        private bool UsuarioExists(Guid id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }

        // POST: api/Roles/Inicializar
        [HttpPost("Inicializar")]
        public async Task<ActionResult> InicializarRoles()
        {
            var rolesIniciales = new List<RolDTO>
            {
                new RolDTO { NombreRol = "Administrador", Descripcion = "Usuario con acceso completo al sistema, incluyendo la gestión de usuarios, roles, permisos, infracciones y generación de informes." },
                new RolDTO { NombreRol = "Usuario Final", Descripcion = "Usuario que puede consultar sus multas, gestionar reclamos y realizar pagos de multas." },
                new RolDTO { NombreRol = "Oficial de Tránsito", Descripcion = "Oficial autorizado para crear y modificar multas en el sistema." },
                new RolDTO { NombreRol = "Juez de Tránsito", Descripcion = "Usuario encargado de gestionar los reclamos realizados por los usuarios." }
            };

            foreach (var rolDTO in rolesIniciales)
            {
                // Crear el rol en AspNetRoles si no existe
                if (!await _roleManager.RoleExistsAsync(rolDTO.NombreRol))
                {
                    var createResult = await _roleManager.CreateAsync(new IdentityRole(rolDTO.NombreRol));

                    // Manejo de errores específicos en la creación de AspNetRoles
                    if (!createResult.Succeeded)
                    {
                        foreach (var error in createResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, $"Error al crear rol en AspNetRoles: {error.Description}");
                        }
                        continue; // Saltar la creación en Roles si falla en AspNetRoles
                    }
                }

                // Verificar si el rol ya existe en la tabla personalizada Roles
                if (_context.Roles.Any(r => r.NombreRol == rolDTO.NombreRol))
                {
                    continue; // Saltar si el rol ya está en la tabla Roles personalizada
                }

                // Crear el rol en la tabla Roles personalizada
                var rol = new Rol
                {
                    IdRol = Guid.NewGuid(),
                    NombreRol = rolDTO.NombreRol,
                    Descripcion = rolDTO.Descripcion
                };

                _context.Roles.Add(rol);
            }

            await _context.SaveChangesAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Retornar errores si existen
            }

            return Ok("Roles iniciales agregados exitosamente.");
        }

        // POST: api/Usuarios/InicializarRoles
        [HttpPost("InicializarRoles")]
        public async Task<ActionResult> InicializarRolesParaUsuarios()
        {
            var usuariosRoles = new List<(string emailUsuario, string nombreRol)>
            {
                ("admin@nextek.com", "Administrador"),
                ("carlosg@gmail.com", "Usuario Final"),
                ("mariap@gmail.com", "Usuario Final"),
                ("juanr@gmail.com", "Usuario Final"),
                ("luiss@nextek.com", "Oficial de Tránsito"),
                ("sofiac@nextek.com", "Oficial de Tránsito"),
                ("andresv@nextek.com", "Oficial de Tránsito"),
                ("lauras@nextek.com", "Juez de Tránsito"),
                ("diegom@nextek.com", "Juez de Tránsito"),
                ("anah@nextek.com", "Juez de Tránsito")
            };

            foreach (var (emailUsuario, nombreRol) in usuariosRoles)
            {
                // Obtener el usuario en Identity por correo electrónico
                var usuarioIdentity = await _userManager.FindByEmailAsync(emailUsuario);
                if (usuarioIdentity == null)
                {
                    ModelState.AddModelError(string.Empty, $"Usuario con email '{emailUsuario}' no encontrado en AspNetUsers.");
                    continue;
                }

                // Obtener el usuario en la tabla Usuarios
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == emailUsuario);
                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, $"Usuario con email '{emailUsuario}' no encontrado en la tabla Usuarios.");
                    continue;
                }

                // Obtener el rol en la tabla Roles
                var rol = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == nombreRol);
                if (rol == null)
                {
                    ModelState.AddModelError(string.Empty, $"Rol '{nombreRol}' no encontrado en la tabla Roles.");
                    continue;
                }

                // Asignar el rol al usuario en Identity
                var addToRoleResult = await _userManager.AddToRoleAsync(usuarioIdentity, nombreRol);
                if (!addToRoleResult.Succeeded)
                {
                    foreach (var error in addToRoleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, $"Error al asignar rol '{nombreRol}' al usuario '{emailUsuario}': {error.Description}");
                    }
                    continue;
                }

                // Verificar si la relación entre usuario y rol ya existe en la tabla personalizada
                var existeUsuarioRol = await _context.UsuarioRoles
                    .AnyAsync(ur => ur.UsuarioId == usuario.IdUsuario && ur.RolId == rol.IdRol);

                if (!existeUsuarioRol)
                {
                    // Crear la relación entre usuario y rol en la tabla personalizada
                    var usuarioRol = new UsuarioXRol
                    {
                        UsuarioId = usuario.IdUsuario,
                        RolId = rol.IdRol
                    };

                    _context.UsuarioRoles.Add(usuarioRol);
                }
            }

            await _context.SaveChangesAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok("Roles asignados a los usuarios exitosamente en Identity y en las tablas personalizadas.");
        }
    }
}
