namespace OasisHyperDriveClient.Core.Models;

public class AvatarInfo
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AvatarImage { get; set; }

    public string DisplayName => !string.IsNullOrEmpty(Username)
        ? $"@{Username}"
        : $"{FirstName} {LastName}".Trim();
}

public class AuthenticateRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthenticateResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JwtToken { get; set; }
    public string? RefreshToken { get; set; }
}
