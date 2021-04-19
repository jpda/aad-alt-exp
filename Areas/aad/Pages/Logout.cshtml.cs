using System.Linq;
using System.Threading.Tasks;
using aad_alt_exp.UserVariableTokenCache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace aad_alt_exp.Areas.aad.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<AuthorizeEndModel> _logger;
        private readonly MsalClientFactory _msal;

        public LogoutModel(ILogger<AuthorizeEndModel> logger, MsalClientFactory msal)
        {
            _logger = logger;
            _msal = msal;
        }

        public async Task OnGet(string account)
        {
            _logger.LogTrace($"Logout requested for {account}");
            var client = _msal.CreateForIdentifier(User);
            IAccount accountToRemove;

            if (!string.IsNullOrEmpty(account))
            {
                accountToRemove = await client.GetAccountAsync(account);
            }
            else
            {
                accountToRemove = (await client.GetAccountsAsync()).FirstOrDefault();
            }

            _logger.LogInformation($"Logging out {accountToRemove.HomeAccountId.Identifier}");
            await client.RemoveAsync(accountToRemove);
            Response.Redirect("/aad/authorize");
        }
    }
}