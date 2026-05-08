using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Social.Common.Handlers;
using Social.Data.Model.Base;
using Social.Data.Model.Response.Login;
using Social.Data.Repository;
using Social.JwtServices.Interface;
using Social.WebApi.Models.Api.Login;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Social.JwtServices.Services
{
    public class JwtService : IJwtService
    {
        private readonly SocialDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public JwtService(SocialDbContext dbContext, IConfiguration configuration)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task<LoginResponseModel?> Authenticate(LoginRequestModel request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return null;
                }

                var userAccount = await _dbContext
                    .Users
                    .FirstOrDefaultAsync(x => x.UserName == request.UserName
                                              && x.RecordStatusId == RecordStatus.Status.Active);

                if (userAccount is null || !PasswordHashHandler.VerifyPassWord(request.Password, userAccount.PassWord))
                {
                    return null;
                }

                var issuer = _configuration["JwtConfig:Issuer"];
                var audience = _configuration["JwtConfig:Audience"];
                var key = _configuration["JwtConfig:Key"];
                var tokenValidityMins = _configuration.GetValue<int>("JwtConfig:TokenValidityMins");
                var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(JwtRegisteredClaimNames.Name, request.UserName),
                }),
                    Expires = tokenExpiryTimeStamp,
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), SecurityAlgorithms.HmacSha512Signature),
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var accessToken = tokenHandler.WriteToken(securityToken);

                return new LoginResponseModel
                {
                    AccessToken = accessToken,
                    UserName = request.UserName,
                    ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.UtcNow).TotalSeconds
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
