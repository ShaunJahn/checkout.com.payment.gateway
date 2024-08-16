using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace PaymentGateway.Api.Auth
{
    public class JwtTokenManager : IJwtTokenManager
    {
        public string Authenticate(string userName, string password)
        {
            const string key = "SomeKeyVaultKeyOrAppSettingsKeyHardCodedForDEMO!]]]]";
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, userName)
                ]),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public interface IJwtTokenManager
    {
        string Authenticate(string userName, string password);
    }
}
