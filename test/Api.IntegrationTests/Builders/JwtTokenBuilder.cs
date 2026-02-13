using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.IntegrationTests.Builders;

public class JwtTokenBuilder
{
    private List<Claim> claims = new List<Claim>();

    public static JwtTokenBuilder Create() => new();

    public string Build()
    {
        return JwtTokenProvider.JwtSecurityTokenHandler.WriteToken(
            new JwtSecurityToken(
                JwtTokenProvider.Issuer,
                JwtTokenProvider.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: JwtTokenProvider.SigningCredentials
            )
        );
    }

    public JwtTokenBuilder WithClaim(string type, string value)
    {
        claims.Add(new Claim(type, value));
        return this;
    }

    public JwtTokenBuilder WithClaim(Claim claim)
    {
        claims.Add(claim);
        return this;
    }
}
