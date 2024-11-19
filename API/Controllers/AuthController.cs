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
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AppDbContext context, IEmailService emailService, INotificationService notificationService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
            _emailService = emailService; // Asigna el servicio
            _notificationService = notificationService;
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
            var identityUser = await _userManager.FindByEmailAsync(userData.Email);
            if (identityUser == null)
            {
                return BadRequest("Usuario no encontrado.");
            }

            // Verificar la contraseña
            var passwordValid = await _userManager.CheckPasswordAsync(identityUser, userData.Password);
            if (!passwordValid)
            {
                return Unauthorized("Contraseña incorrecta.");
            }

            // Obtener el rol del usuario
            var roles = await _userManager.GetRolesAsync(identityUser);
            var role = roles.FirstOrDefault(); // Suponiendo que el usuario tiene un solo rol

            // Generar el token JWT
            var token = await GenerateJwtToken(identityUser);

            // Buscar el usuario en la tabla Usuarios usando el mismo email
            var usuario = await _context.Usuarios
                .Where(u => u.Email == userData.Email)
                .Select(u => new { u.IdUsuario })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return BadRequest("Usuario no encontrado en la tabla Usuarios.");
            }

            // Retornar el token, IdUsuario y role
            return Ok(new { token, userId = usuario.IdUsuario, role });
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

        [HttpGet]
        public IActionResult ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("El token no puede ser nulo o vacío.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                // Validar el token y extraer las claims
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero // Opcional, elimina el tiempo de tolerancia
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "sub")?.Value; // "sub" es el email
                var role = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "role")?.Value;

                return Ok(new
                {
                    userId,
                    role
                });
            }
            catch (Exception ex)
            {
                return Unauthorized($"Token inválido: {ex.Message}");
            }
        }



        [HttpPost]
        public async Task<IActionResult> Register([FromBody] LogUpDTO newUser, string roleName = "Usuario Final")
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

            // Crear el usuario en AspNetUsers sin hashear manualmente la contraseña
            var createdUserResult = await _userManager.CreateAsync(usuarioIdentity, newUser.Password);

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
                    ContrasennaHash = Encrypt.GetSHA256(newUser.Password),
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

                // Obtener el rol en la tabla personalizada de Roles
                var rol = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == roleName);
                if (rol == null)
                {
                    return BadRequest($"Rol '{roleName}' no encontrado en la tabla de Roles.");
                }

                // Crear la relación en la tabla UsuarioXRol
                var usuarioRol = new UsuarioXRol
                {
                    UsuarioId = usuarioDatos.IdUsuario,
                    RolId = rol.IdRol
                };
                _context.UsuarioRoles.Add(usuarioRol);

                await _context.SaveChangesAsync();

                return Created("Usuario creado exitosamente", null);
            }

            foreach (var error in createdUserResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUsuarioAdmin([FromBody] LogUpDTO newUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Generar contraseña aleatoria si no está en el DTO
            var generatedPassword = string.IsNullOrEmpty(newUser.Password) ? PasswordGenerator.GenerateRandomPassword() : newUser.Password;

            // Crear usuario en Identity
            var usuarioIdentity = new AppUser
            {
                Email = newUser.Email,
                UserName = newUser.Email
            };

            var createdUserResult = await _userManager.CreateAsync(usuarioIdentity, generatedPassword);

            if (createdUserResult.Succeeded)
            {
                var userId = usuarioIdentity.Id;

                var usuarioDatos = new Usuario
                {
                    IdUsuario = Guid.NewGuid(),
                    UserId = userId,
                    Cedula = newUser.Cedula,
                    Nombre = newUser.Nombre,
                    Apellido1 = newUser.Apellido1,
                    Apellido2 = newUser.Apellido2,
                    ContrasennaHash = Encrypt.GetSHA256(generatedPassword),
                    Email = newUser.Email,
                    FechaNacimiento = DateOnly.ParseExact(newUser.FechaNacimiento, "dd-MM-yyyy"),
                    Telefono = newUser.Telefono,
                    FotoPerfil = newUser.FotoPerfil,
                    Estado = true
                };

                _context.Usuarios.Add(usuarioDatos);
                await _context.SaveChangesAsync();

                // Retornar los detalles del usuario creado, incluyendo el ID para asignación de rol posterior
                return Created("Usuario creado exitosamente", new
                {
                    Email = newUser.Email,
                    Password = generatedPassword,
                    IdUsuario = usuarioDatos.IdUsuario // Devuelve el ID del usuario para asignación de rol
                });
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

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe por razones de seguridad
                return Ok("Si el correo está registrado, se enviará un enlace para restablecer la contraseña.");
            }

            // Generar el token para el restablecimiento de contraseña
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Crear la URL para restablecer la contraseña
            var resetUrl = $"{model.ResetUrl}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";

            // Enviar el correo
            await _emailService.SendResetPasswordEmail(user.Email, resetUrl);

            return Ok("Si el correo está registrado, se enviará un enlace para restablecer la contraseña.");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("Correo inválido.");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return Ok("La contraseña se ha restablecido correctamente.");
            }

            return BadRequest("No se pudo restablecer la contraseña.");
        }

        [HttpPost]

        public async Task<IActionResult> SendEmail([FromBody] SendEmailDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Ok("Error al encontrar correo");
            }

            //Envio del correo
            await _notificationService.SendEmail(model.Email, model.Message);
            return Ok("El mensaje ha sido enviado correctamente.");
        }
    }
}