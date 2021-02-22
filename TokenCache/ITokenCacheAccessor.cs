using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.Identity.Client;


namespace aad_alt_exp.UserVariableTokenCache
{
    public interface ITokenCacheAccessor
    {
        void BeforeAccess(TokenCacheNotificationArgs args);
        void AfterAccess(TokenCacheNotificationArgs args);
        void Configure(string identifier);
        Task<bool> UpdateCacheKey(string partition, string temp, string actual);
    }

    public static class UserExtensions
    {
        public static string DeriveUserIdentifier(this ClaimsPrincipal principal)
        {
            var identifier = principal.HasClaim(x => x.Type == ClaimTypes.NameIdentifier) ?
             principal.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value : throw new System.ArgumentException("identifier is null - login first");
            return identifier;
        }
    }

}
