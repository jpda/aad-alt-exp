using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using aad_alt_exp.UserVariableTokenCache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace aad_alt_exp.Pages
{
    [Authorize]
    public class ApplicationsModel : PageModel
    {
        public IList<DisplayClaim> UserClaims;
        public string Token;

        private readonly ILogger<ClaimsModel> _logger;
        private readonly MsalClientFactory _msal;
        private readonly List<string> _appScopes = new() { "Application.ReadWrite.All", "Policy.Read.All", "Policy.ReadWrite.ConditionalAccess" };

        public ApplicationsModel(ILogger<ClaimsModel> logger, MsalClientFactory msal)
        {
            _logger = logger;
            _msal = msal;
        }

        public async Task OnGet()
        {
            _logger.LogDebug($"Getting Graph token for {User.DeriveUserIdentifier()} for scopes {_appScopes}");
            var msal = _msal.CreateForIdentifier(User);
            var token = await msal.AcquireTokenSilent(_appScopes, (await msal.GetAccountsAsync()).First()).ExecuteAsync();
            Token = token.AccessToken;
        }

        private async Task GetApplications()
        {
            
        }
    }
}