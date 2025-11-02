using Application.Abstractions;
using Application.AppSettingConfigurations;
using Application.Dtos.CitizenIdentity.Request;
using Application.Dtos.DriverLicense.Request;
using Application.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Infrastructure.ExternalService
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;

        public GeminiService(HttpClient httpClient, IOptions<GeminiSettings> options, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.HttpTimeoutSeconds);
        }
        public async Task<string?> DetectDocumentTypeAsync(string imageUrl)
        {
            var prompt = """
            You are a classification AI.
            Look at the uploaded image and determine what type of document it is.

            Return one of these exact strings (no extra text):
            - "CitizenID" if it is a Vietnamese Citizen Identity Card (Căn cước công dân Việt Nam)
            - "DriverLicense" if it is a Vietnamese Driver License (Giấy phép lái xe Việt Nam)
            - "Unknown" if it is neither or unclear
            """;

            var text = await CallGeminiAndExtractText(imageUrl, prompt);
            if (string.IsNullOrWhiteSpace(text)) return "Unknown";

            text = text.Trim().ToLowerInvariant();
            if (text.Contains("citizen")) return "CitizenID";
            if (text.Contains("driver")) return "DriverLicense";
            return "Unknown";
        }
        public async Task<CreateCitizenIdentityReq?> ExtractCitizenIdAsync(string imageUrl)
        {
            var prompt = """
                You are an OCR AI. Extract text from the image of a Vietnamese Citizen ID Card (Căn cước công dân Việt Nam).

                Return only this JSON object:
                {
                  "id_number": "...",
                  "full_name": "...",
                  "nationality": "...",
                  "sex": "...",
                  "date_of_birth": "yyyy-MM-dd",
                  "expires_at": "yyyy-MM-dd"
                }

                Guidelines:
                - If text is in uppercase (e.g. NGUYEN VAN A), still extract normally.
                - Keep Vietnamese accents (e.g. Việt Nam, Nguyễn Văn A).
                - Do NOT include additional text, explanations, or Markdown.
                - Do NOT return empty strings for missing fields. Always try to guess if possible.
                """; ;

            var text = await CallGeminiAndExtractText(imageUrl, prompt);
            if (string.IsNullOrWhiteSpace(text)) return null;

            try
            {
                return JsonSerializer.Deserialize<CreateCitizenIdentityReq>(text);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<CreateDriverLicenseReq?> ExtractDriverLicenseAsync(string imageUrl)
        {
            var prompt = """
            You are an OCR assistant for Vietnamese Driver Licenses (Bằng lái xe Việt Nam).

            Extract and return ONLY this JSON:
            {
              "Number": "string",
              "FullName": "string",
              "Nationality": "string",
              "Sex": "string",
              "DateOfBirth": "string (yyyy-MM-dd)",
              "ExpiresAt": "string (yyyy-MM-dd)",
              "Class": "string"
            }

            Rules:
            - Detect text in both Vietnamese and English.
            - If a field is missing, return empty string "".
            - Dates must follow yyyy-MM-dd.
            - Output JSON only, no markdown or extra commentary.
            """;

            var text = await CallGeminiAndExtractText(imageUrl, prompt);
            if (string.IsNullOrWhiteSpace(text)) return null;

            try
            {
                return JsonSerializer.Deserialize<CreateDriverLicenseReq>(text);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<string?> CallGeminiAndExtractText(string imageUrl, string prompt)
        {
            object imagePart;
            try
            {
                imagePart = await GeminiRequestHelper.BuildInlineImagePart(
                    _httpClient, imageUrl, _settings.MaxImageBytes);
            }
            catch (Exception ex)
            {
                return null;
            }

            var request = new
            {
                contents = new[]
                {
                    new {
                        parts = new object[]
                        {
                            new { text = prompt },
                            imagePart
                        }
                    }
                }
            };

            var url = $"{_settings.ApiBaseUrl}/models/{_settings.ModelName}:generateContent?key={_settings.ApiKey}";

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync(url, request);
            }
            catch (Exception ex)
            {
                return null;
            }

            var raw = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            try
            {
                var doc = JsonDocument.Parse(raw);
                string? text = null;

                if (doc.RootElement.TryGetProperty("candidates", out var cands) && cands.GetArrayLength() > 0)
                {
                    var candidate = cands[0];
                    if (candidate.TryGetProperty("content", out var content))
                    {
                        if (content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                        {
                            if (parts[0].TryGetProperty("text", out var t))
                                text = t.GetString();
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }

                text = text.Replace("```json", "").Replace("```", "").Trim();
                var match = Regex.Match(text, "{.*}", RegexOptions.Singleline);
                if (match.Success) text = match.Value;

                return text;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}