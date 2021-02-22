using System.Security.Cryptography.X509Certificates;
namespace aad_alt_exp.UserVariableTokenCache
{
    public class MsalOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string TenantId { get; set; }
        public X509Certificate2 Certificate { get; set; }
    }
}
