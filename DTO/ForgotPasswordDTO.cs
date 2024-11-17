using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ForgotPasswordDto
    {
        public string Email { get; set; }
        public string ResetUrl { get; set; } // La URL base para restablecer la contraseña
    }
}
