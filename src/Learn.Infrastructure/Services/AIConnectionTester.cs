using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Learn.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Learn.Infrastructure.Services;

public class AIConnectionTester : IAIConnectionTester
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ClaudeOptions _claudeOptions;
    private readonly OllamaOptions _ollamaOptions;

    public AIConnectionTester(
        IHttpClientFactory httpClientFactory,
        IOptions<ClaudeOptions> claudeOptions,
        IOptions<OllamaOptions> ollamaOptions)
    {
        _httpClientFactory = httpClientFactory;
        _claudeOptions = claudeOptions.Value;
        _ollamaOptions = ollamaOptions.Value;
    }

    public async Task<TestConnectionResult> TestAsync(string model, string? apiKey, CancellationToken ct)
    {
        return string.Equals(model, "ollama", StringComparison.OrdinalIgnoreCase)
            ? await TestOllamaAsync(ct)
            : await TestClaudeAsync(apiKey, ct);
    }

    private async Task<TestConnectionResult> TestClaudeAsync(string? apiKey, CancellationToken ct)
    {
        string effectiveKey = string.IsNullOrWhiteSpace(apiKey) ? _claudeOptions.ApiKey : apiKey;

        if (string.IsNullOrWhiteSpace(effectiveKey))
            return new TestConnectionResult { Success = false, Message = "No API key provided" };

        var payload = new
        {
            model = _claudeOptions.Model,
            max_tokens = 16,
            messages = new[] { new { role = "user", content = "Reply with OK" } }
        };

        using HttpClient client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(20);

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        request.Headers.Add("x-api-key", effectiveKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        var sw = Stopwatch.StartNew();
        try
        {
            using HttpResponseMessage response = await client.SendAsync(request, ct);
            sw.Stop();

            if (response.IsSuccessStatusCode)
                return new TestConnectionResult { Success = true, ResponseTimeMs = sw.ElapsedMilliseconds, Message = "Connected" };

            string body = await response.Content.ReadAsStringAsync(ct);
            string hint = response.StatusCode == System.Net.HttpStatusCode.Unauthorized ? "Invalid API key (401)" : $"HTTP {(int)response.StatusCode}";
            return new TestConnectionResult { Success = false, ResponseTimeMs = sw.ElapsedMilliseconds, Message = hint };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new TestConnectionResult { Success = false, ResponseTimeMs = sw.ElapsedMilliseconds, Message = ex.Message };
        }
    }

    private async Task<TestConnectionResult> TestOllamaAsync(CancellationToken ct)
    {
        var payload = new
        {
            model = _ollamaOptions.Model,
            prompt = "Reply with OK",
            stream = false,
            options = new { num_predict = 8 }
        };

        using HttpClient client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        var sw = Stopwatch.StartNew();
        try
        {
            using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await client.PostAsync($"{_ollamaOptions.BaseUrl}/api/generate", content, ct);
            sw.Stop();

            if (response.IsSuccessStatusCode)
                return new TestConnectionResult { Success = true, ResponseTimeMs = sw.ElapsedMilliseconds, Message = "Connected" };

            return new TestConnectionResult { Success = false, ResponseTimeMs = sw.ElapsedMilliseconds, Message = $"HTTP {(int)response.StatusCode}" };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new TestConnectionResult { Success = false, ResponseTimeMs = sw.ElapsedMilliseconds, Message = ex.Message };
        }
    }
}
