using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public static class PasswordGenerator
    {
        public static string GenerateRandomPassword()
        {
            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "#?!@$%^&*";
            const int minLength = 8;
            const int maxLength = 16;

            var random = new Random();
            var passwordBuilder = new StringBuilder();

            passwordBuilder.Append(upperCase[random.Next(upperCase.Length)]);
            passwordBuilder.Append(lowerCase[random.Next(lowerCase.Length)]);
            passwordBuilder.Append(digits[random.Next(digits.Length)]);
            passwordBuilder.Append(specialChars[random.Next(specialChars.Length)]);

            for (int i = 4; i < random.Next(minLength, maxLength); i++)
            {
                string allChars = upperCase + lowerCase + digits + specialChars;
                passwordBuilder.Append(allChars[random.Next(allChars.Length)]);
            }

            return passwordBuilder.ToString();
        }
    }
}
