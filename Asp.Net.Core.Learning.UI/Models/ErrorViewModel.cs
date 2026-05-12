namespace Asp.Net.Core.Learning.UI.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class TokensViewModel
    {
        public string IdentityTokenDisplay { get; set; } = string.Empty;
        public string AccessTokenDisplay { get; set; } = string.Empty;
        public string RefreshTokenDisplay { get; set; } = string.Empty;
    }
}
