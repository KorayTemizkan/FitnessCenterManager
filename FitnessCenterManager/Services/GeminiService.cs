using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    /// <summary>
    /// Sadece metin ile AI'dan plan al
    /// </summary>
    public async Task<string> GetDietPlanAsync(string height, string weight, string gender, string goal)
    {
        return await GetDietPlanWithImageAsync(height, weight, gender, goal, null);
    }
    public async Task ListAvailableModelsAsync()
    {
        var apiKey = _configuration["GeminiApiKey"];
        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";

        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("--- ERÝÞÝLEBÝLÝR MODELLER ---");
                Console.WriteLine(content);
                // Burada 'content' içinde hangi modellerin 'generateContent' desteklediðini görebilirsin.
            }
            else
            {
                Console.WriteLine($"Modeller listelenemedi. Hata: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Loglama hatasý: {ex.Message}");
        }
    }

    /// <summary>
    /// Metin ve opsiyonel fotoðraf ile AI'dan plan ve görsel al
    /// </summary>
    public async Task<string> GetDietPlanWithImageAsync(string height, string weight, string gender, string goal, byte[]? imageBytes)
    {
        var apiKey = _configuration["GeminiApiKey"];
        // ÖNEMLÝ: Görsel üretim yeteneði olan modeli çaðýrýyoruz
        var endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        // Prompt Hazýrlama
        var promptText = $"Ben {height} cm boyunda, {weight} kg aðýrlýðýnda, {gender} biriyim. Hedefim: {goal}. " +
                         "Lütfen þunlarý yap:\n" +
                         "1- Bana uygun, maddeler halinde günlük bir egzersiz planý ve beslenme listesi oluþtur. Yanýtý HTML formatýnda (ul, li, strong) ver.\n" +
                         "2- Bu planýn en sonuna, benim hedefe ulaþtýðýmdaki fit ve saðlýklý halimi gösteren yüksek kaliteli bir görsel üret.";

        object requestBody;

        if (imageBytes != null)
        {
            var base64Image = Convert.ToBase64String(imageBytes);
            requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = promptText },
                            new
                            {
                                inline_data = new
                                {
                                    mime_type = "image/jpeg",
                                    data = base64Image
                                }
                            }
                        }
                    }
                }
            };
        }
        else
        {
            requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = promptText } } }
                }
            };
        }

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{endpoint}?key={apiKey}", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetail = await response.Content.ReadAsStringAsync();
            return $"<div class='alert alert-danger'><strong>HATA:</strong> {response.StatusCode} <br> {errorDetail}</div>";
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        try
        {
            var result = JsonSerializer.Deserialize<GeminiResponse>(responseString, options);

            StringBuilder finalContent = new StringBuilder();
            string? generatedImageBase64 = null;

            if (result?.Candidates?[0]?.Content?.Parts != null)
            {
                foreach (var part in result.Candidates[0].Content.Parts)
                {
                    // Metin parçalarýný birleþtir
                    if (!string.IsNullOrEmpty(part.Text))
                    {
                        finalContent.Append(part.Text);
                    }

                    // Üretilen görsel parçasýný yakala
                    if (part.InlineData != null && !string.IsNullOrEmpty(part.InlineData.Data))
                    {
                        generatedImageBase64 = part.InlineData.Data;
                    }
                }
            }

            string finalOutput = finalContent.ToString();

            // Eðer yapay zeka bir görsel ürettiyse, bunu HTML'in sonuna <img> etiketi olarak ekle
            if (!string.IsNullOrEmpty(generatedImageBase64))
            {
                finalOutput += $"<div class='mt-4 text-center'>" +
                               $"<h3>Hedeflenen Görünüm</h3>" +
                               $"<img src='data:image/png;base64,{generatedImageBase64}' class='img-fluid rounded shadow' style='max-width:500px;' alt='Generated Fitness Goal' />" +
                               $"</div>";
            }

            return !string.IsNullOrEmpty(finalOutput) ? finalOutput : "Yapay zeka boþ bir cevap döndü.";
        }
        catch (Exception ex)
        {
            return $"Cevap iþlenirken bir hata oluþtu: {ex.Message}";
        }
    }
}

// --- JSON Modelleri ---

public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public Candidate[]? Candidates { get; set; }
}

public class Candidate
{
    [JsonPropertyName("content")]
    public Content? Content { get; set; }
}

public class Content
{
    [JsonPropertyName("parts")]
    public Part[]? Parts { get; set; }
}

public class Part
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("inline_data")]
    public InlineData? InlineData { get; set; }
}

public class InlineData
{
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }
}