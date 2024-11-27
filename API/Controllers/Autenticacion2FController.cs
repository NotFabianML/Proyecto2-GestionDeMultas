using BusinessLogic;
using DTO;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Autenticacion2FController : ControllerBase
    {
        private readonly IAutenticacion2FService _autenticacion2FService;
            public Autenticacion2FController(IAutenticacion2FService autenticacion2FService)
            {
                _autenticacion2FService = autenticacion2FService;
            }

            [HttpPost("enable")]
            public async Task<IActionResult> EnableTwoFactor([FromBody] HabilitarAutenticacion2F request)
            {
                var result = await _autenticacion2FService.EnableTwoFactorAsync(request.Email, request.Password);

                if (!result.Success)
                    return BadRequest(result.Mensaje);

                var qrCodeBase64 = "data:image/png;base64," + result.Data;

                return Ok(new { QrCodeBase64 = qrCodeBase64 });
            }


            [HttpPost("validate")]
            public IActionResult ValidateTwoFactor([FromBody] ValidarAutenticacion2F request)
            {
                var isValid = _autenticacion2FService.ValidateTwoFactorCode(request.Email, request.TotpCode);
                if (!isValid)
                    return BadRequest("Invalid 2FA code.");

                return Ok("2FA code validated successfully.");
            }

            [HttpPost("disable")]
            public async Task<IActionResult> DisableTwoFactor([FromBody] HabilitarAutenticacion2F request)
            {
                var result = await _autenticacion2FService.DisableTwoFactorAsync(request.Email, request.Password);
                if (!result.Success)
                    return BadRequest(result.Mensaje);

                return Ok(result.Mensaje);
            }
        }


    }





