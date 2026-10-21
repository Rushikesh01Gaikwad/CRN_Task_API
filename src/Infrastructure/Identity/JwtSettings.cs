namespace Infrastructure.Identity;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = "CRN_Secure_Super_Secret_Key_2026_For_Jwt_Authentication!";
    public string Issuer { get; set; } = "CRN_API";
    public string Audience { get; set; } = "CRN_Clients";
    public int ExpiryMinutes { get; set; } = 30;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
