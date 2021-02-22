using Microsoft.Identity.Client;

namespace aad_alt_exp.UserVariableTokenCache
{
    public static class TokenCacheExtensions
    {
        public static void AddPerUserTokenCache(this IConfidentialClientApplication app, ITokenCacheAccessor cache)
        {
            app.AppTokenCache.SetBeforeAccess(cache.BeforeAccess);
            app.UserTokenCache.SetBeforeAccess(cache.BeforeAccess);
            app.AppTokenCache.SetAfterAccess(cache.AfterAccess);
            app.UserTokenCache.SetAfterAccess(cache.AfterAccess);
        }
    }
}
