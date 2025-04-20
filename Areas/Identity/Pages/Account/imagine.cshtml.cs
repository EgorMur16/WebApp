using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebAppAuthorization.Controllers
{
    public class ImagineModel : PageModel
    {
        public void OnGet()
        {
        }
    }
    public class ImageProcessingController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private const string StabilityAIToken = "sk-kvhHGkymfCAx60RIYHFhQq4kGc5zl2PfTDdwar2aW3ZTF1KY";

        public ImageProcessingController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage([FromBody] ImageProcessingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ImageBase64))
                {
                    return BadRequest(new { error = "No image provided" });
                }

                // Process the image with Stability AI
                var processedImage = await ProcessImageWithStabilityAI(request.ImageBase64, request.Text);

                return Ok(new
                {
                    message = "Image processed successfully",
                    processedImage = processedImage
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async Task<string> ProcessImageWithStabilityAI(string imageBase64, string prompt)
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {StabilityAIToken}");
            client.DefaultRequestHeaders.Add("Accept", "image/*");

            // Convert base64 to byte array
            var imageBytes = Convert.FromBase64String(imageBase64);
            using var imageStream = new MemoryStream(imageBytes);

            using var formData = new MultipartFormDataContent();
            formData.Add(new StreamContent(imageStream), "image", "input.png");
            formData.Add(new StringContent(prompt ?? "enhance image"), "prompt");
            formData.Add(new StringContent("png"), "output_format");

            var response = await client.PostAsync(
                "https://api.stability.ai/v2beta/stable-image/edit/inpaint",
                formData);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {response.StatusCode} - {errorContent}");
            }

            var processedImageBytes = await response.Content.ReadAsByteArrayAsync();
            return Convert.ToBase64String(processedImageBytes);
        }
    }

    public class ImageProcessingRequest
    {
        public string ImageBase64 { get; set; }
        public string Text { get; set; }
    }
}