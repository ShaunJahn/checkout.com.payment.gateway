using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Auth;

namespace PaymentGateway.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IJwtTokenManager jwtTokenManager) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public IActionResult Login([FromBody] ClientCredentials clientCredentials)
        {
            if (clientCredentials.UserName != "DemoClient" || clientCredentials.Password != "SomePassword")
            {
                return Unauthorized();
            }

            var token = jwtTokenManager.Authenticate(clientCredentials.UserName, clientCredentials.Password);
            return Ok(token);
        }
    }
}
