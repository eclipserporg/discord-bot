using DiscordBot.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace DiscordBot.Middlewares;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;
    private readonly Credentials _credentials;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IOptions<Credentials> credentials,
        IConfiguration configuration) : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
        _credentials = credentials.Value;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string authHeader = Request.Headers["Authorization"];
        if (authHeader != null && authHeader.StartsWith("Basic"))
        {

            var authHeaderValue = authHeader.Replace("Basic ", "");
            var decodedAuthHeaderValue = Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderValue));
            var userPassArray = decodedAuthHeaderValue.Split(":");
            var extractedUsername = userPassArray[0];
            var extractedPassword = userPassArray[1];

            if (string.Equals(_credentials.Username, extractedUsername) && string.Equals(extractedPassword, _credentials.Password))
            {
                var claims = new[] { new Claim(ClaimTypes.Name, _credentials.Username) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
        }
        return AuthenticateResult.Fail("Failed to authenticate");
    }
}
