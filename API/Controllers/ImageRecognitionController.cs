//using Microsoft.AspNetCore.Mvc;
//using BusinessLogic;
//using System.IO;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;

//namespace API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ImageRecognitionController : ControllerBase
//    {
//        private readonly DocumentAnalysisService _documentAnalysisService;

//        public ImageRecognitionController(DocumentAnalysisService documentAnalysisService)
//        {
//            _documentAnalysisService = documentAnalysisService;
//        }

//        [HttpPost("AnalyzeIdDocument")]
//        public async Task<IActionResult> AnalyzeIdDocument(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//            {
//                return BadRequest("Please upload a valid document.");
//            }

//            try
//            {
//                using (var stream = file.OpenReadStream())
//                {
//                    var documentNumber = await _documentAnalysisService.AnalyzeIdDocumentAsync(stream);
//                    return Ok(new { documentNumber });
//                }
//            }
//            catch (InvalidOperationException ex)
//            {
//                return BadRequest($"Error analyzing document: {ex.Message}");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Unexpected error: {ex.Message}");
//            }
//        }
//    }
//}
