namespace OasisHyperDriveClient.Core.Auth;

public interface ICredentialStore
{
    void SaveToken(string token);
    string? LoadToken();
    void ClearToken();
}
