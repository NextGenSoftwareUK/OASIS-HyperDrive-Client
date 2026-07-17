using OasisHyperDriveClient.Core.Models;

namespace OasisHyperDriveClient.Core.Api;

public class AvatarService
{
    private readonly OasisApiClient _api;

    public AvatarService(OasisApiClient api)
    {
        _api = api;
    }

    public async Task<AuthenticateResponse?> AuthenticateAsync(string email, string password, CancellationToken ct = default)
    {
        var result = await _api.PostAsync<AuthenticateResponse>("api/avatar/authenticate",
            new AuthenticateRequest { Email = email, Password = password }, ct);

        return result.IsError ? null : result.Result;
    }

    public async Task<AvatarInfo?> GetLoggedInAvatarAsync(CancellationToken ct = default)
    {
        var result = await _api.GetAsync<AvatarInfo>("api/avatar/get-logged-in-avatar", ct);
        return result.IsError ? null : result.Result;
    }

    public async Task<IReadOnlyList<AvatarInfo>> SearchAvatarsAsync(string query, CancellationToken ct = default)
    {
        var result = await _api.GetAsync<List<AvatarInfo>>($"api/avatar/search?searchQuery={Uri.EscapeDataString(query)}", ct);
        return result.IsError || result.Result is null ? [] : result.Result;
    }
}
