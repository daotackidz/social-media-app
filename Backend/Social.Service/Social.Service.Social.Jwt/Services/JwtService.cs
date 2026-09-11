using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Social.Common.Handlers;
using Social.Data.Model.Base;
using Social.Data.Model.Request.User;
using Social.Data.Model.Response.User;
using Social.Data.Repository;
using Social.Service.Social.Jwt.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Social.Service.Social.Jwt.Services
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

        public async Task<LoginResponse?> Authenticate(LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return null;
                }

                // Đăng ký chuẩn hoá email về chữ thường + trim trước khi lưu (AuthService.RegisterAsync),
                // nên so khớp lúc đăng nhập cũng phải chuẩn hoá y hệt — nếu không, sai hoa/thường hoặc
                // khoảng trắng thừa (rất dễ gặp do gõ tay/autofill) sẽ khiến đăng nhập luôn thất bại dù
                // đúng mật khẩu.
                var normalizedEmail = request.Email.Trim().ToLowerInvariant();

                var userAccount = await _dbContext
                    .Users
                    .FirstOrDefaultAsync(x => x.Email == normalizedEmail
                                              && x.RecordStatusId == RecordStatus.Status.Active);

                if (userAccount is null || !PasswordHashHandler.VerifyPassWord(request.Password, userAccount.PasswordHash))
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
                        new Claim(ClaimTypes.NameIdentifier, userAccount.Id.ToString()),
                        new Claim("userId", userAccount.Id.ToString()),
                        new Claim(ClaimTypes.Name, userAccount.UserName ?? userAccount.Email),
                        new Claim("username", userAccount.UserName ?? userAccount.Email),
                        new Claim(JwtRegisteredClaimNames.Email, userAccount.Email),
                    }),
                    Expires = tokenExpiryTimeStamp,
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), SecurityAlgorithms.HmacSha512Signature),
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var accessToken = tokenHandler.WriteToken(securityToken);

                return new LoginResponse
                {
                    AccessToken = accessToken,
                    Email = userAccount.Email,
                    Username = userAccount.UserName,
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
