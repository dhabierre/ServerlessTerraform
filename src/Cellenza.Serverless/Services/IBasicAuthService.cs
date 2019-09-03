namespace Cellenza.Serverless.Services
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    public interface IBasicAuthService
    {
        AuthenticateResult Authenticate(HttpRequest httpRequest, string expectedBasicAuthentication);
    }
}