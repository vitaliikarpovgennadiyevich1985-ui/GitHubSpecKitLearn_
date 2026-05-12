using Asp.Net.Core.Learning.IdentityServer.Models;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Asp.Net.Core.Learning.IdentityServer.Infrastructure
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            if (user == null)
            {
                return;
            }

            var claims = new List<Claim>
            {
                new Claim("email", user.Email ?? ""),
                new Claim("name", user.UserName ?? ""),
                new Claim("phone", "+380679295538"),
                new Claim("role", "Role1"),
                new Claim("role", "Role2"),
            };

            //var roles = await _userManager.GetRolesAsync(user);
            //claims.AddRange(roles.Select(r => new Claim("role", r)));            

            context.IssuedClaims.AddRange(claims.Where(c => context.RequestedClaimTypes.Contains(c.Type)));
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.CompletedTask;
        }
    }
}
