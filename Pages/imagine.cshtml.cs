using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WebAppAutorization.Pages
{
    [Authorize]
    public class ImagineModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly List<string> _apiKeys = new()
        {
            "api key"
        };
        private readonly Random _random = new();

        public ImagineModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

            if (_apiKeys.Count == 0)
            {
                throw new Exception("Нет API ключей");
            }
        }

        [BindProperty]
        public IFormFile ImageFile { get; set; } = null!;

        [BindProperty]
        public string Text { get; set; } = string.Empty;

        public string ProcessedImageBase64 { get; set; } = string.Empty;

        [BindProperty]
        public string SearchPrompt { get; set; } = "";

        [BindProperty]
        public string OutputFormat { get; set; } = "webp";

        private string GetRandomApiKey()
        {
            return _apiKeys[_random.Next(_apiKeys.Count)];
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (ImageFile == null || ImageFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Пожалуйста, загрузите изображение");
                return Page();
            }

            var client = _httpClientFactory.CreateClient();

            try
            {
                var boundary = "------------------------" + DateTime.Now.Ticks.ToString("x");
                using var content = new MultipartFormDataContent(boundary);
                var imageContent = new StreamContent(ImageFile.OpenReadStream());

                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(ImageFile.ContentType);
                imageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"image\"",
                    FileName = $"\"{ImageFile.FileName}\""
                };
                content.Add(imageContent);

                void AddFormField(string fieldName, string value)
                {
                    var fieldContent = new StringContent(value);
                    fieldContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = $"\"{fieldName}\""
                    };
                    content.Add(fieldContent);
                }

                AddFormField("prompt", Text);
                AddFormField("search_prompt", SearchPrompt);
                AddFormField("output_format", OutputFormat);

                HttpClient apiClient = new();
                var selectedApiKey = GetRandomApiKey();
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", selectedApiKey);
                apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/*"));

                var requestUri = "https://api.stability.ai/v2beta/stable-image/edit/search-and-replace";
                var response = await apiClient.PostAsync(requestUri, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Ошибка API: {errorContent}");
                    return Page();
                }

                var resultBytes = await response.Content.ReadAsByteArrayAsync();
                ProcessedImageBase64 = Convert.ToBase64String(resultBytes);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка: {ex.Message}");
            }

            return Page();
        }
    }
}