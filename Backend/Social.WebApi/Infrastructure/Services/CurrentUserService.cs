using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Social.WebApi.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Guid? UserId
        {
            get
            {
                var value = GetClaimValue(ClaimTypes.NameIdentifier)
                            ?? GetClaimValue("userId")
                            ?? GetClaimValue(ClaimTypes.Sid)
                            ?? GetClaimValue("sub");

                return Guid.TryParse(value, out var guid) ? guid : null;
            }
        }

        public string? Email => GetClaimValue(ClaimTypes.Email) ?? GetClaimValue("email");

        public string? UserName => GetClaimValue(ClaimTypes.Name) ?? GetClaimValue("username");

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        private string? GetClaimValue(string claimType)
            => User?.FindFirst(claimType)?.Value;
    }
}
