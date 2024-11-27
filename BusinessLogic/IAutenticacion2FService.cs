using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public interface IAutenticacion2FService
    {
        Task<AutenticacionResultado> EnableTwoFactorAsync(string email, string password);
        bool ValidateTwoFactorCode(string email, string totpCode);
        Task<AutenticacionResultado> DisableTwoFactorAsync(string email, string password);

    }
}
