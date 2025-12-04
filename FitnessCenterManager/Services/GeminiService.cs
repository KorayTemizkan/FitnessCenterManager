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

    public async Task<string> GetDietPlanAsync(string height, string weight, string gender, string goal)
    {
        var apiKey = _configuration["GeminiApiKey"];

        // LİSTEDEN BULDUĞUMUZ DOĞRU MODEL İSMİ BURADA:
        var endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        // Prompt Hazırlama
        var prompt = $"Ben {height} cm boyunda, {weight} kg ağırlığında, {gender} biriyim. " +
                     $"Hedefim: {goal}. Bana uygun, maddeler halinde günlük bir egzersiz planı ve " +
                     "örnek bir beslenme listesi oluştur. Yanıtı HTML formatında (ul, li, strong etiketleri kullanarak) ver. " +
                     "Sadece içeriği ver, markdown (```html) etiketlerini kullanma.";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        // İstek Gönderme
        var response = await _httpClient.PostAsync($"{endpoint}?key={apiKey}", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetail = await response.Content.ReadAsStringAsync();
            return $"<div class='alert alert-danger'><strong>HATA:</strong> {response.StatusCode} <br> {errorDetail}</div>";
        }

        // Yanıtı Okuma
        var responseString = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var result = JsonSerializer.Deserialize<GeminiResponse>(responseString, options);
            return result?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "Yapay zeka boş bir cevap döndü.";
        }
        catch
        {
            return "Gelen cevap işlenirken bir hata oluştu.";
        }
    }
}

// JSON Modelleri
public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public Candidate[] Candidates { get; set; }
}

public class Candidate
{
    [JsonPropertyName("content")]
    public Content Content { get; set; }
}

public class Content
{
    [JsonPropertyName("parts")]
    public Part[] Parts { get; set; }
}

public class Part
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}