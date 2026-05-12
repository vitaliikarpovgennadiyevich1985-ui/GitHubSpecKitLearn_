using Asp.Net.Core.Learning.UI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Asp.Net.Core.Learning.UI.Controllers
{
    public class AccountController : Controller
    {
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return SignOut(
                new AuthenticationProperties { RedirectUri = Url.Action("Index", "Home") },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize]
        public async Task<IActionResult> Tokens()
        {
            var authResult = await HttpContext.AuthenticateAsync();
            var props = authResult.Properties;

            var model = new TokensViewModel
            {
                IdentityTokenDisplay = FormatJwt(props?.GetTokenValue("id_token")),
                AccessTokenDisplay   = FormatJwt(props?.GetTokenValue("access_token")),
                RefreshTokenDisplay  = props?.GetTokenValue("refresh_token") ?? "(not available)"
            };

            return View(model);
        }

        private static string FormatJwt(string? jwt)
        {
            if (string.IsNullOrEmpty(jwt)) return "(not available)";
            var parts = jwt.Split('.');
            if (parts.Length < 3) return jwt;

            var sb = new StringBuilder();
            sb.AppendLine("=== Header ===");
            sb.AppendLine(DecodeBase64UrlJson(parts[0]));
            sb.AppendLine("=== Payload ===");
            sb.AppendLine(DecodeBase64UrlJson(parts[1]));
            sb.AppendLine("=== Signature ===");
            sb.Append(parts[2]);
            return sb.ToString();
        }

        private static string DecodeBase64UrlJson(string base64Url)
        {
            var padded = base64Url.Replace('-', '+').Replace('_', '/');
            padded = (padded.Length % 4) switch
            {
                2 => padded + "==",
                3 => padded + "=",
                _ => padded
            };
            try
            {
                var bytes = Convert.FromBase64String(padded);
                var json = Encoding.UTF8.GetString(bytes);
                using var doc = JsonDocument.Parse(json);
                return JsonSerializer.Serialize(doc.RootElement,
                    new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return base64Url;
            }
        }
    }
}
