using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class AutenticacionResultado
    {
        public bool Success { get; set; }
        public string Mensaje { get; set; }
        public object? Data { get; set; }

        public AutenticacionResultado(bool success, string message)
        {
            Success = success;
            Mensaje = message;
            Data = null;
        }

        public AutenticacionResultado(bool success, string message, object? data)
        {
            Success = success;
            Mensaje = message;
            Data = data;
        }

        public static AutenticacionResultado SuccessResult(string message, object? data = null)
            => new AutenticacionResultado(true, message, data);

        public static AutenticacionResultado FailureResult(string message)
            => new AutenticacionResultado(false, message);
    }
}
