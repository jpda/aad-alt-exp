using System;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace aad_alt_exp.UserVariableTokenCache
{
    public class MsalClientFactory
    {
        private readonly MsalOptions _config;
        private readonly MsalOptions _chinaConfig;
        private readonly ITokenCacheAccessor _tokenCacheAccessor;
        private readonly ILogger _log;

        public MsalClientFactory(IOptionsSnapshot<MsalOptions> config, ITokenCacheAccessor tokenCacheAccessor, ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger<MsalClientFactory>();
            _log.LogDebug($"Entering MsalClientFactory without certificate");

            _config = config.Get("Commercial");
            _chinaConfig = config.Get("China");
            _tokenCacheAccessor = tokenCacheAccessor;
        }

        public MsalClientFactory(IOptionsSnapshot<MsalOptions> config, ITokenCacheAccessor tokenCacheAccessor, X509Certificate2 cert, ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger<MsalClientFactory>();
            _log.LogDebug($"Entering MsalClientFactory with injected certificate {cert.Subject}, {cert.Thumbprint}");

            _config = config.Get("Commercial");
            _chinaConfig = config.Get("China");
            _config.Certificate = cert;
            _tokenCacheAccessor = tokenCacheAccessor;
        }

        public IConfidentialClientApplication Create(bool useChina)
        {
            var config = useChina ? _chinaConfig : _config;
            var app = ConfidentialClientApplicationBuilder
                .Create(config.ClientId)
                .WithRedirectUri(config.RedirectUri)
                .WithAuthority(config.Instance)
                // form post causes issues with the cookie container from the original request
                // we could work around this with an anonymous landing page to receive the auth_code
                // then redirect to the /aad/authorizeend endpoint
                // or we could just stuff the auth_code in the querystring. QS for now.
                //.WithExtraQueryParameters(new Dictionary<string, string>() { { "response_mode", "form_post" } })
                ;

            if (!string.IsNullOrWhiteSpace(config.TenantId))
            {
                app.WithTenantId(config.TenantId);
            }
            if (config.Certificate == null)
            {
                app.WithClientSecret(config.ClientSecret);
            }
            else
            {
                app.WithCertificate(config.Certificate);
            }
            return app.Build();
        }

        public IConfidentialClientApplication CreateForIdentifier(ClaimsPrincipal principal, bool useChina = false)
        {
            _log.LogTrace("Creating msal client for principal");
            var app = this.Create(useChina);
            _log.LogTrace($"Principal identifer: {principal.DeriveUserIdentifier()}");
            _tokenCacheAccessor.Configure(principal.DeriveUserIdentifier());
            app.AddPerUserTokenCache(_tokenCacheAccessor);
            return app;
        }

        public IConfidentialClientApplication CreateForIdentifier(string identifier, bool useChina = false)
        {
            _log.LogTrace($"Creating msal client for identifer: {identifier}");
            var app = this.Create(useChina);
            _tokenCacheAccessor.Configure(identifier);
            app.AddPerUserTokenCache(_tokenCacheAccessor);
            return app;
        }

        // todo: refactor this
        [Obsolete("For handling transient identities; usually in authorization/token intial round-trip (before sign-in when no suitable identifier is available)", true)]
        public IConfidentialClientApplication CreateWithTransientIdentity(string transientId)
        {
            var app = ConfidentialClientApplicationBuilder.Create(_config.ClientId).WithRedirectUri(_config.RedirectUri).WithTenantId(_config.TenantId).WithCertificate(_config.Certificate).Build();
            _tokenCacheAccessor.Configure(transientId);
            app.AddPerUserTokenCache(_tokenCacheAccessor);
            return app;
        }

        public async Task<bool> SwitchTransientKeyToActual(string temp, string actual)
        {
            return await _tokenCacheAccessor.UpdateCacheKey(_config.ClientId, temp, actual).ConfigureAwait(true);
        }
    }
}
