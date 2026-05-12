using Asp.Net.Core.Learning.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Asp.Net.Core.Learning.IdentityServer.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;

    public LogoutModel(
        SignInManager<ApplicationUser> signInManager,
        IIdentityServerInteractionService interaction)
    {
        _signInManager = signInManager;
        _interaction = interaction;
    }

    public async Task<IActionResult> OnGet(string? logoutId)
        => await PerformLogoutAsync(logoutId);

    public async Task<IActionResult> OnPost(string? logoutId)
        => await PerformLogoutAsync(logoutId);

    private async Task<IActionResult> PerformLogoutAsync(string? logoutId)
    {
        await _signInManager.SignOutAsync();

        if (!string.IsNullOrEmpty(logoutId))
        {
            var ctx = await _interaction.GetLogoutContextAsync(logoutId);
            if (!string.IsNullOrEmpty(ctx?.PostLogoutRedirectUri))
            {
                return Redirect(ctx.PostLogoutRedirectUri);
            }
        }

        return Page();
    }
}
