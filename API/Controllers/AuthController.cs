using DataAccess.EF.Models;
using DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LogInDTO userData)
        {
            var usuario = await _userManager.FindByNameAsync(userData.Email);
            if (usuario != null && await _userManager.CheckPasswordAsync(usuario, userData.Password))
            {
                var token = await GenerateJwtToken(usuario);
                return Ok(new { token });
            }
            return Unauthorized();
        }

        private async Task<string> GenerateJwtToken(IdentityUser usuario)
        {
            var roles = await _userManager.GetRolesAsync(usuario);  // Retrieve the roles for the usuario
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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
        public async Task<IActionResult> Register([FromBody] LogUpDTO newUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = new AppUser
            {
                UserName = newUser.Cedula,
                Email = newUser.Email
            };

            var createdUserResult = await _userManager.CreateAsync(usuario, newUser.Password);

            if (createdUserResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(usuario, "User");
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