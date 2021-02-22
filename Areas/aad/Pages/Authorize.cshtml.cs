using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using aad_alt_exp.UserVariableTokenCache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace aad_alt_exp.Areas.aad.Pages
{
    [Authorize]
    public class AuthorizeModel : PageModel
    {
        public System.Uri AuthorizationRedirectUrl;
        public List<IAccount> Accounts;

        private readonly ILogger<AuthorizeModel> _logger;
        private readonly MsalClientFactory _msal;

        public AuthorizeModel(ILogger<AuthorizeModel> logger, MsalClientFactory msal)
        {
            _logger = logger;
            _msal = msal;
        }

        public async Task OnGet()
        {
            var msal = _msal.CreateForIdentifier(User);
            this.AuthorizationRedirectUrl = await msal.GetAuthorizationRequestUrl(new[] { "Application.ReadWrite.All", "Policy.Read.All", "Policy.ReadWrite.ConditionalAccess" }).ExecuteAsync();
            var accounts = await msal.GetAccountsAsync();
            Accounts = accounts.ToList();
        }
    }
}