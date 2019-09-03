namespace Cellenza.Serverless.Services
{
    using System;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    internal class BasicAuthService : IBasicAuthService
    {
        private const string BasicScheme = "Basic";

        private readonly IConfigurationRoot configuration;

        public BasicAuthService(IConfigurationRoot configuration)
        {
            this.configuration = configuration;
        }

        public AuthenticateResult Authenticate(HttpRequest httpRequest, string expectedBasicAuthentication)
        {
            if (!httpRequest.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Authorization header missing");
            }

            if (!AuthenticationHeaderValue.TryParse(
                httpRequest.Headers["Authorization"],
                out AuthenticationHeaderValue headerValue))
            {
                return AuthenticateResult.Fail("Authorization header value is incorrect");
            }

            if (!BasicScheme.Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail("Bad Authorization header scheme");
            }

            if (headerValue.Parameter != expectedBasicAuthentication)
            {
                return AuthenticateResult.Fail("Invalid username or password");
            }

            var headerValueBytes = Convert.FromBase64String(headerValue.Parameter);
            var credentials = Encoding.UTF8.GetString(headerValueBytes);
            var username = credentials.Split(':').First();

            var claims = new[] { new Claim(ClaimTypes.Name, username) };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, BasicScheme));
            var ticket = new AuthenticationTicket(principal, BasicScheme);

            return AuthenticateResult.Success(ticket);
        }
    }
}