using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OasisHyperDriveClient.Core.Models;

namespace OasisHyperDriveClient.Core.Api;

public class OasisApiClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public OasisApiClient(HttpClient http)
    {
        _http = http;
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
        var response = await _http.GetAsync(path, ct);
        return await ReadResult<T>(response);
    }

    public async Task<OASISResult<T>> PostAsync<T>(string path, object? body = null, CancellationToken ct = default)
    {
        var content = body is null
            ? new StringContent("{}", Encoding.UTF8, "application/json")
            : new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(path, content, ct);
        return await ReadResult<T>(response);
    }

    public async Task<OASISResult<T>> DeleteAsync<T>(string path, object? body = null, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        if (body is null)
        {
            response = await _http.DeleteAsync(path, ct);
        }
        else
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, path)
            {
                Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json")
            };
            response = await _http.SendAsync(req, ct);
        }
        return await ReadResult<T>(response);
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
