using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BusinessLogic
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendResetPasswordEmail(string email, string resetUrl)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var fromEmail = _configuration["SendGrid:FromEmail"];
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(fromEmail, "Soporte de Aplicación");
            var subject = "Restablecimiento de Contraseña";
            var to = new EmailAddress(email);
            var plainTextContent = $"Haga clic en el siguiente enlace para restablecer su contraseña: {resetUrl}";
            var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; line-height: 1.5;'>
                <h2>Restablecimiento de Contraseña</h2>
                <p>Haga clic en el siguiente enlace para restablecer su contraseña:</p>
                <a href='{resetUrl}' style='color: #4CAF50;'>Restablecer Contraseña</a>
                <br><br>
                <p>Si no solicitó este cambio, ignore este mensaje.</p>
                <p>Atentamente,<br>Soporte de Aplicación</p>
            </div>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            // Opcional: Log para depuración
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                var errorMessage = await response.Body.ReadAsStringAsync();
                Console.WriteLine($"Error al enviar el correo: {errorMessage}");
            }
        }
    }
}
