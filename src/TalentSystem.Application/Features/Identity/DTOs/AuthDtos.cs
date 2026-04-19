namespace TalentSystem.Application.Features.Identity.DTOs;

public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponseDto
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }

    public string TokenType { get; init; } = "Bearer";
}
