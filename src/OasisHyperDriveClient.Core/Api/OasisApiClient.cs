using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OasisHyperDriveClient.Core.Models;
using Polly;
using Polly.CircuitBreaker;

namespace OasisHyperDriveClient.Core.Api;

public class OasisApiClient
{
    private readonly HttpClient _http;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreaker;

    // Circuit opens after 5 consecutive failures; stays open for 30 s then allows one test request
    private static AsyncCircuitBreakerPolicy<HttpResponseMessage> BuildCircuitBreaker() =>
        Policy<HttpResponseMessage>
            .HandleResult(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(30),
                minimumThroughput: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));

    public bool IsCircuitOpen => _circuitBreaker.CircuitState == CircuitState.Open;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public OasisApiClient(HttpClient http)
    {
        _http = http;
        _circuitBreaker = BuildCircuitBreaker();
    }

    public void SetBearerToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken()
    {
        _http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<OASISResult<T>> GetAsync<T>(string path, CancellationToken ct = default)
    {
        try
        {
            var response = await _circuitBreaker.ExecuteAsync(() => _http.GetAsync(path, ct));
            return await ReadResult<T>(response);
        }
        catch (BrokenCircuitException)
        {
            return new OASISResult<T> { IsError = true, Message = "Service unavailable — circuit open (too many recent failures)" };
        }
    }

    public async Task<OASISResult<T>> PostAsync<T>(string path, object? body = null, CancellationToken ct = default)
    {
        var content = body is null
            ? new StringContent("{}", Encoding.UTF8, "application/json")
            : new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        try
        {
            var response = await _circuitBreaker.ExecuteAsync(() => _http.PostAsync(path, content, ct));
            return await ReadResult<T>(response);
        }
        catch (BrokenCircuitException)
        {
            return new OASISResult<T> { IsError = true, Message = "Service unavailable — circuit open" };
        }
    }

    public async Task<OASISResult<T>> DeleteAsync<T>(string path, object? body = null, CancellationToken ct = default)
    {
        try
        {
            var response = await _circuitBreaker.ExecuteAsync(() =>
            {
                if (body is null) return _http.DeleteAsync(path, ct);
                var req = new HttpRequestMessage(HttpMethod.Delete, path)
                {
                    Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json")
                };
                return _http.SendAsync(req, ct);
            });
            return await ReadResult<T>(response);
        }
        catch (BrokenCircuitException)
        {
            return new OASISResult<T> { IsError = true, Message = "Service unavailable — circuit open" };
        }
    }

    private static async Task<OASISResult<T>> ReadResult<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new OASISResult<T>
            {
                IsError = true,
                Message = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"
            };
        }

        try
        {
            return JsonSerializer.Deserialize<OASISResult<T>>(json, JsonOptions)
                ?? new OASISResult<T> { IsError = true, Message = "Empty response" };
        }
        catch (JsonException ex)
        {
            return new OASISResult<T>
            {
                IsError = true,
                Message = $"Failed to parse response: {ex.Message}"
            };
        }
    }
}
