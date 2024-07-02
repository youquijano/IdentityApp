using Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Service
{
    public class JWTService
    {
        private readonly IConfiguration config;
        private readonly SymmetricSecurityKey _jwtKey;
        public JWTService(IConfiguration config)
        {
            this.config = config;
            _jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.config["JWT:Key"]));
        }
        public string CreateJWT(User user)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),                
                new Claim(ClaimTypes.Email, user.UserName),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName)
            };

            var credentials = new SigningCredentials(_jwtKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = DateTime.UtcNow.AddDays(int.Parse(config["JWT:ExpiresInDays"])),
                SigningCredentials = credentials,
                Issuer = config["JWT:Issuer"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(jwt);
        }
    }
}
