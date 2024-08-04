using FirstMicroService.Gateway.YARP.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FirstMicroService.Gateway.YARP.Services;

public sealed class JwtProvider(IConfiguration configuration)
{
    public string CreateToken(User user)
    {
        var claims = new List<Claim>()
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("UserName", user.UserName)
        };

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("JWT:SecretKey").Value ?? ""));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha512);

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: configuration.GetSection("JWT:Issuer").Value,
            audience: configuration.GetSection("JWT:Audience").Value,
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: signingCredentials
            );

        var handler = new JwtSecurityTokenHandler();

        var token = handler.WriteToken(jwtSecurityToken);

        return token;
    }
}
