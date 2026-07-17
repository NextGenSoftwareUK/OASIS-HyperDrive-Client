using OasisHyperDriveClient.Core.Api;
using OasisHyperDriveClient.Core.Models;

namespace OasisHyperDriveClient.Core.Auth;

public class AuthService
{
    private readonly AvatarService _avatarService;
    private readonly OasisApiClient _apiClient;
    private readonly ICredentialStore _credentialStore;

    public AvatarInfo? CurrentAvatar { get; private set; }
    public bool IsAuthenticated => CurrentAvatar is not null;

    public AuthService(AvatarService avatarService, OasisApiClient apiClient, ICredentialStore credentialStore)
    {
        _avatarService = avatarService;
        _apiClient = apiClient;
        _credentialStore = credentialStore;
    }

    public async Task<bool> TryRestoreSessionAsync(CancellationToken ct = default)
    {
        var token = _credentialStore.LoadToken();
        if (string.IsNullOrEmpty(token))
            return false;

        _apiClient.SetBearerToken(token);
        var avatar = await _avatarService.GetLoggedInAvatarAsync(ct);
        if (avatar is null)
        {
            _apiClient.ClearToken();
            _credentialStore.ClearToken();
            return false;
        }

        CurrentAvatar = avatar;
        return true;
    }

    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var response = await _avatarService.AuthenticateAsync(email, password, ct);
        if (response?.JwtToken is null)
            return (false, "Invalid email or password.");

        _apiClient.SetBearerToken(response.JwtToken);
        _credentialStore.SaveToken(response.JwtToken);

        CurrentAvatar = new AvatarInfo
        {
            Id = response.Id,
            Username = response.Username,
            Email = response.Email,
            FirstName = response.FirstName,
            LastName = response.LastName
        };

        return (true, null);
    }

    public void Logout()
    {
        _apiClient.ClearToken();
        _credentialStore.ClearToken();
        CurrentAvatar = null;
    }
}
