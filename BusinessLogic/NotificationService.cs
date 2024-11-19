using Microsoft.Extensions.Configuration;
using SendGrid.Helpers.Mail;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        
        public NotificationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmail(string email, string message)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var fromEmail = _configuration["SendGrid:FromEmail"];
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(fromEmail, "Soporte de Aplicación");
            var subject = "Notificacion del Sistema de Multas";
            var to = new EmailAddress(email);
            var plainTextContent = $"Sistema de Multas: {message}";
            var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; line-height: 1.5;'>
                <h2>Sistema de notificaciones de Multas le informa:</h2>
                <p>{message}</p>
                <br><br>
                <p>Si no esta informado de este cambio por favor consulte con un administrador.</p>
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
            Console.WriteLine($"Enviando correo a {email} con el mensaje: {message}");
        }
    }
}
