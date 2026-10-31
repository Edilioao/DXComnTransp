using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Authentication;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using Microsoft.IdentityModel.Tokens;

namespace DXComnTransp.WebApi.JWT;

public class JwtTokenProviderService : IAuthenticationTokenProvider {
    readonly IStandardAuthenticationService securityAuthenticationService;
    readonly IConfiguration configuration;

    public JwtTokenProviderService(IStandardAuthenticationService securityAuthenticationService, IConfiguration configuration) {
        this.securityAuthenticationService = securityAuthenticationService;
        this.configuration = configuration;
    }
    public string Authenticate(object logonParameters) {
        ClaimsPrincipal user = securityAuthenticationService.Authenticate(logonParameters);

        if(user != null) {
            string issuer = configuration["Authentication:Jwt:Issuer"];
            string audience = configuration["Authentication:Jwt:Audience"];
            string signingKeyValue = configuration["Authentication:Jwt:IssuerSigningKey"];
            if(string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience)) {
                throw new InvalidOperationException(
                    "Configure 'Authentication:Jwt:Issuer' y 'Authentication:Jwt:Audience'.");
            }
            if(string.IsNullOrWhiteSpace(signingKeyValue) || Encoding.UTF8.GetByteCount(signingKeyValue) < 32) {
                throw new InvalidOperationException(
                    "Configure 'Authentication:Jwt:IssuerSigningKey' con al menos 32 caracteres.");
            }
            var issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKeyValue));
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: user.Claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(issuerSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        throw new AuthenticationException("User name or password is incorrect.");
    }
}
