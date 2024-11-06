using BusinessLogic;
using DataAccess.EF;
using DataAccess.EF.Models;
using DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        //[HttpPost]
        //public async Task<IActionResult> Login([FromBody] LogInDTO userData)
        //{
        //    // Cambia FindByNameAsync a FindByEmailAsync para buscar por correo electrónico
        //    var usuario = await _userManager.FindByEmailAsync(userData.Email);
        //    if (usuario == null)
        //    {
        //        return BadRequest("Usuario no encontrado."); // Error específico si el usuario no existe
        //    }

        //    // Verifica si la contraseña es correcta sin aplicar hashing manual 
        //    //var passwordHashed = Encrypt.GetSHA256(userData.Password);
        //    var passwordValid = await _userManager.CheckPasswordAsync(usuario, userData.Password);
        //    if (!passwordValid)
        //    {
        //        return Unauthorized("Contraseña incorrecta."); // Error específico si la contraseña no coincide
        //    }

        //    // Genera el token JWT si la autenticación es exitosa
        //    var token = await GenerateJwtToken(usuario);
        //    return Ok(new { token });
        //}

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LogInDTO userData)
        {
            // Buscar usuario por email
            var usuario = await _userManager.FindByEmailAsync(userData.Email);
            if (usuario == null)
            {
                return BadRequest("Usuario no encontrado.");
            }

            // Verificar la contraseña
            var passwordValid = await _userManager.CheckPasswordAsync(usuario, userData.Password);
            if (!passwordValid)
            {
                return Unauthorized("Contraseña incorrecta.");
            }

            // Obtener el rol del usuario
            var roles = await _userManager.GetRolesAsync(usuario);
            var role = roles.FirstOrDefault(); // Suponiendo que el usuario tiene un solo rol

            // Generar el token JWT
            var token = await GenerateJwtToken(usuario);

            // Retornar el token, userId y role
            return Ok(new { token, userId = usuario.Id, role });
        }


        private async Task<string> GenerateJwtToken(IdentityUser usuario)
        {
            var roles = await _userManager.GetRolesAsync(usuario); // Obtener roles del usuario
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Email), // Reemplazar por Email si no usas UserName
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("role", roles.FirstOrDefault() ?? "Usuario Final") // Incluir el primer rol en el token
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpPost]
        public async Task<IActionResult> Register([FromBody] LogUpDTO newUser, string roleName = "Usuario")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Crear el usuario en AspNetUsers usando Email como identificador único
            var usuarioIdentity = new AppUser
            {
                Email = newUser.Email,
                UserName = newUser.Email  // Usar Email como UserName para compatibilidad con Identity
            };

            // Crear el usuario en AspNetUsers
            var passwordHashed = Encrypt.GetSHA256(newUser.Password);
            var createdUserResult = await _userManager.CreateAsync(usuarioIdentity, passwordHashed);

            if (createdUserResult.Succeeded)
            {
                var userId = usuarioIdentity.Id;

                // Crear el registro en la tabla Usuarios
                var usuarioDatos = new Usuario
                {
                    IdUsuario = Guid.NewGuid(),
                    UserId = userId,  // Relación con AspNetUsers
                    Cedula = newUser.Cedula,
                    Nombre = newUser.Nombre,
                    Apellido1 = newUser.Apellido1,
                    Apellido2 = newUser.Apellido2,
                    Email = newUser.Email,
                    FechaNacimiento = DateOnly.ParseExact(newUser.FechaNacimiento, "dd-MM-yyyy"),
                    Telefono = newUser.Telefono,
                    FotoPerfil = newUser.FotoPerfil,
                    Estado = true
                };

                _context.Usuarios.Add(usuarioDatos);
                await _context.SaveChangesAsync();

                // Asigna el rol en Identity al usuario
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }

                await _userManager.AddToRoleAsync(usuarioIdentity, roleName);

                return Created("Usuario creado exitosamente", null);
            }

            foreach (var error in createdUserResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }




        [HttpGet]
        public async Task<bool> RoleTesting(string userName)
        {
            var usuario = await _userManager.FindByNameAsync(userName);
            if (usuario == null)
            {
                // El usuario no existe
                return false;
            }

            var result = await _userManager.IsInRoleAsync(usuario, "Admin");
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> MakeAdmin(string userName)
        {
            // Buscar al usuario por su nombre de usuario
            var usuario = await _userManager.FindByNameAsync(userName);

            if (usuario == null)
            {
                return NotFound("Usuario no encontrado");
            }

            // Agregar el rol 'Admin' al usuario
            var result = await _userManager.AddToRoleAsync(usuario, "Admin");

            if (result.Succeeded)
            {
                return Ok("Usuario agregado al rol de Admin con exito");
            }

            // Si hubo alg�n error al agregar el rol
            return BadRequest("No se pudo agregar el rol de Admin al usuario");
        }
    }
}