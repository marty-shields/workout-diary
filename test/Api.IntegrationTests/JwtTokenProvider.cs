using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Api.IntegrationTests;

public static class JwtTokenProvider
{
    // our fake issuer - can be anything
    public static string Issuer { get; } = "Integration_Tests_Issuer";

    public static string Audience { get; } = "Integration_Tests_Audience";

    // Our random signing key - used to sign and validate the tokens
    public static SecurityKey SecurityKey { get; } = new SymmetricSecurityKey(new byte[32]
    {
        // 32 bytes = 256 bits for HS256
        0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
        0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
        0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
        0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
    });

    // the signing credentials used by the token handler to sign tokens
    public static SigningCredentials SigningCredentials { get; } = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);

    // the token handler we'll use to actually issue tokens
    public static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();
}
