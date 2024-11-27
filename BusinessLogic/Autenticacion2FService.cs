using DataAccess.EF;
using DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Common;

namespace BusinessLogic
{
    public class Autenticacion2FService : IAutenticacion2FService
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly PasswordHasher<IdentityUser> _passwordHasher;


        public Autenticacion2FService(AppDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _passwordHasher = new PasswordHasher<IdentityUser>();
        }

        public Autenticacion2FService()
        {
        }

        public async Task<AutenticacionResultado> EnableTwoFactorAsync(string email, string password)
        {
            var identityUser = await _userManager.FindByEmailAsync(email);
            if (identityUser == null || !ValidatePassword(password, identityUser.PasswordHash))
                return new AutenticacionResultado(false, "Email o Contraseña incorrectos.");

            var user = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return new AutenticacionResultado(false, "Usuario no encontrado.");

            var secretKey = GenerateSecretKey();
            user.DobleFactorSecret = secretKey;

            user.DobleFactorActivo = true;
            await _dbContext.SaveChangesAsync();

            var totpUri = GenerateTotpUri(secretKey, email);
            var qrCodeBase64 = GenerateQrCode(totpUri);

            return new AutenticacionResultado(true, "2FA Habilitado correctamente.", qrCodeBase64);
        }

        public async Task<AutenticacionResultado> DisableTwoFactorAsync(string email, string password)
        {
            var identityUser = await _userManager.FindByEmailAsync(email);
            if (identityUser == null || !ValidatePassword(password, identityUser.PasswordHash))
                return new AutenticacionResultado(false, "Email o Contraseña incorrectos.");

            var user = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return new AutenticacionResultado(false, "Usuario no encontrado.");

            user.DobleFactorSecret = null;
            user.DobleFactorActivo = false;
            await _dbContext.SaveChangesAsync();

            return new AutenticacionResultado(true, "Doble Factor deshabilitado");
        }

        private string GenerateSecretKey()
        {
            var secretKey = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(secretKey);
        }

        private string GenerateTotpUri(string secretKey, string email)
        {
            string appName = "Nextek";
            return $"otpauth://totp/{appName}:{email}?secret={secretKey}&issuer={appName}";
        }

        private string GenerateQrCode(string totpUri)
        {
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 300,
                    Height = 300,
                    Margin = 1
                }
            };

            using var qrCodeImage = barcodeWriter.Write(totpUri);
            using var memoryStream = new MemoryStream();
            qrCodeImage.Save(memoryStream, ImageFormat.Png);
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        private bool ValidatePassword(string password, string storedHash)
        {
            var identityUser = new IdentityUser();
            var result = _passwordHasher.VerifyHashedPassword(identityUser, storedHash, password);

            return result == PasswordVerificationResult.Success;

        }

        public bool ValidateTwoFactorCode(string email, string totpCode)
        {
            var user = _dbContext.Usuarios.FirstOrDefault(u => u.Email == email);
            if (user == null || !user.DobleFactorActivo)
            {
                return false;
            }

            var secretKey = user.DobleFactorSecret;
            if (string.IsNullOrEmpty(secretKey))
            {
                return false;
            }

            var secretBytes = Base32Encoding.ToBytes(secretKey);

            var totp = new Totp(secretBytes);

            var isValid = totp.VerifyTotp(totpCode, out long timeStepMatched, new VerificationWindow(1, 1));

            return isValid;
        }

    }
}
