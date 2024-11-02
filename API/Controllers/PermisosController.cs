using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.EF;
using DataAccess.EF.Models;
using DTO;
using System.Data;

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

        // Obtener permisos asignados a un rol
        [HttpGet("{rolid}/permisos-por-rol")]
        public async Task<ActionResult<IEnumerable<PermisoDTO>>> GetPermisosPorRol(Guid rolid)
        {
            if (!RolExists(rolid))
            {
                return NotFound("Rol no encontrado.");
            }

            var permisos = await _context.Permisos
                .FromSqlRaw("EXEC sp_GetPermisosPorRol @Rol_idRol = {0}", rolid)
                .ToListAsync();

            // mapea manualmente rol a rolDTO
            var permisosDTO = permisos.Select(p => new PermisoDTO
            {
                IdPermiso = p.IdPermiso,
                NombrePermiso = p.NombrePermiso,
                Descripcion = p.Descripcion,
                Estado = p.Estado
            }).ToList();

            return permisosDTO;
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

        // Asignar permiso a un rol
        [HttpPost("{rolId}/asignar-permiso/{id}")]
        public async Task<IActionResult> AsignarPermiso(Guid rolId, Guid id)
        {
            if (!PermisoExists(id) || !_context.Roles.Any(r => r.IdRol == rolId))
            {
                return NotFound("Permiso o rol no encontrado.");
            }

            var result = await _context.Database.ExecuteSqlRawAsync("EXEC sp_AsignarPermiso @Rol_idRol = {0}, @Permiso_idPermiso = {1}", rolId, id);
            if (result == 0)
            {
                return BadRequest("Error al asignar el permiso al usuario.");
            }

            return NoContent();
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

            _context.Permisos.Remove(permiso);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Eliminar permiso de un rol
        [HttpDelete("{rolid}/eliminar-permiso/{permisoId}")]
        public async Task<IActionResult> DeletePermisoDeRol(Guid rolid, Guid permisoId)
        {
            if (!RolExists(rolid) || !_context.Permisos.Any(p => p.IdPermiso == permisoId))
            {
                return NotFound("Rol o permiso no encontrado.");
            }

            var result = await _context.Database.ExecuteSqlRawAsync("EXEC sp_DeletePermisoDeRol @Rol_idRol = {0}, @Permiso_idPermiso = {1}", rolid, permisoId);
            if (result == 0)
            {
                return BadRequest("Error al eliminar el permiso del rol.");
            }

            return NoContent();
        }

        // Método para cambiar el estado de un permiso (1.Activo - 0.Inactivo)
        [HttpPut("{id}/cambiar-estado/{estado}")]
        public async Task<IActionResult> CambiarEstado(Guid id, int estado)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null)
            {
                return NotFound();
            }

            permiso.Estado = Convert.ToBoolean(estado);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool PermisoExists(Guid id)
        {
            return _context.Permisos.Any(e => e.IdPermiso == id);
        }

        private bool RolExists(Guid id)
        {
            return _context.Roles.Any(e => e.IdRol == id);
        }

        // POST: api/Permisos/Inicializar
        [HttpPost("Inicializar")]
        public async Task<ActionResult> InicializarPermisos()
        {
            var permisosIniciales = new List<PermisoDTO>
            {
                new PermisoDTO { NombrePermiso = "Consultar Multas", Descripcion = "Permite al usuario consultar sus multas pendientes y detalles relacionados." },
                new PermisoDTO { NombrePermiso = "Gestionar Reclamos", Descripcion = "Permite al usuario presentar, revisar y gestionar reclamos sobre las multas." },
                new PermisoDTO { NombrePermiso = "Realizar Pagos", Descripcion = "Faculta al usuario para realizar el pago de las multas pendientes a través del sistema." },
                new PermisoDTO { NombrePermiso = "Gestionar Usuarios", Descripcion = "Permite administrar la lista de usuarios, incluyendo la creación, modificación y eliminación de cuentas." },
                new PermisoDTO { NombrePermiso = "Gestionar Roles", Descripcion = "Faculta la asignación y modificación de roles a usuarios en el sistema." },
                new PermisoDTO { NombrePermiso = "Gestionar Permisos", Descripcion = "Permite definir y asignar permisos específicos para cada rol del sistema." },
                new PermisoDTO { NombrePermiso = "Gestionar Infracciones", Descripcion = "Permite la creación, modificación y eliminación de tipos de infracciones en el sistema." },
                new PermisoDTO { NombrePermiso = "Gestionar Multas", Descripcion = "Permite registrar, actualizar y eliminar multas generadas en el sistema." },
                new PermisoDTO { NombrePermiso = "Generar Informes", Descripcion = "Faculta la generación de informes detallados sobre infracciones, multas y estadísticas." },
                new PermisoDTO { NombrePermiso = "Crear Multas", Descripcion = "Permite a un oficial de tránsito generar nuevas multas en el sistema." },
                new PermisoDTO { NombrePermiso = "Modificar Multas", Descripcion = "Permite a un oficial de tránsito editar multas previamente registradas." }
            };

            foreach (var permisoDTO in permisosIniciales)
            {
                // Verificar si el permiso ya existe
                if (_context.Permisos.Any(p => p.NombrePermiso == permisoDTO.NombrePermiso))
                {
                    continue; // Saltar este permiso si ya existe
                }

                // Crear la entidad Permiso
                var permiso = new Permiso
                {
                    IdPermiso = Guid.NewGuid(),
                    NombrePermiso = permisoDTO.NombrePermiso,
                    Descripcion = permisoDTO.Descripcion,
                    Estado = true
                };

                _context.Permisos.Add(permiso);
            }

            await _context.SaveChangesAsync();

            return Ok("Permisos iniciales agregados exitosamente.");
        }

        // POST: api/Roles/InicializarPermisos
        [HttpPost("InicializarPermisos")]
        public async Task<IActionResult> InicializarPermisosParaRoles()
        {
            var rolesPermisos = new List<(string nombreRol, string nombrePermiso)>
            {
                // Permisos para el rol 'Administrador'
                ("Administrador", "Gestionar Usuarios"),
                ("Administrador", "Gestionar Roles"),
                ("Administrador", "Gestionar Permisos"),
                ("Administrador", "Gestionar Infracciones"),
                ("Administrador", "Gestionar Multas"),
                ("Administrador", "Generar Informes"),
        
                // Permisos para el rol 'Usuario Final'
                ("Usuario Final", "Consultar Multas"),
                ("Usuario Final", "Gestionar Reclamos"),
                ("Usuario Final", "Realizar Pagos"),
        
                // Permisos para el rol 'Juez de Tránsito'
                ("Juez de Tránsito", "Gestionar Reclamos"),
        
                // Permisos para el rol 'Oficial de Tránsito'
                ("Oficial de Tránsito", "Crear Multas"),
                ("Oficial de Tránsito", "Modificar Multas")
            };

            foreach (var (nombreRol, nombrePermiso) in rolesPermisos)
            {
                // Obtener IDs de Rol y Permiso
                var rol = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == nombreRol);
                var permiso = await _context.Permisos.FirstOrDefaultAsync(p => p.NombrePermiso == nombrePermiso);

                // Verificar si el rol y el permiso existen
                if (rol == null || permiso == null)
                {
                    continue; // Si no existen, saltar esta asignación
                }

                // Ejecutar el stored procedure para asignar el permiso al rol
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_AsignarPermiso @Rol_idRol = {0}, @Permiso_idPermiso = {1}", rol.IdRol, permiso.IdPermiso);

                // Verificar si hubo un error en la asignación
                if (result == 0)
                {
                    return BadRequest($"Error al asignar el permiso '{nombrePermiso}' al rol '{nombreRol}'.");
                }
            }

            return Ok("Permisos asignados a los roles exitosamente.");
        }


    }
}