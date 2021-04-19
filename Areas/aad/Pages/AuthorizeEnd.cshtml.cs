using System;
using System.Threading.Tasks;
using aad_alt_exp.UserVariableTokenCache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace aad_alt_exp.Areas.aad.Pages
{
    [IgnoreAntiforgeryToken] // since our post is coming from AAD
    public class AuthorizeEndModel : PageModel
    {
        private readonly ILogger<AuthorizeEndModel> _logger;
        private readonly MsalClientFactory _msal;

        public AuthorizeEndModel(ILogger<AuthorizeEndModel> logger, MsalClientFactory msal)
        {
            _logger = logger;
            _msal = msal;
        }

        public async Task OnGet(string code, string session_state)
        {
            await ProcessAuthorizationCode(code, session_state);
            Response.Redirect("/aad/authorize");
        }

        public async Task OnPost(string code, string session_state)
        {
            await ProcessAuthorizationCode(code, session_state);
            Response.Redirect("/aad/authorize");
        }

        private async Task ProcessAuthorizationCode(string code, string session_state)
        {
            _logger.LogInformation($"received authorization code, session {session_state}, redeeming...");
            var msal = _msal.CreateForIdentifier(User);
            try
            {
                _logger.LogWarning("redeeming code with aad...");
                await msal.AcquireTokenByAuthorizationCode(new[] { "Application.ReadWrite.All", "Policy.Read.All", "Policy.ReadWrite.ConditionalAccess" }, code).ExecuteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogWarning("redeeming code with aad china...");
                var cnMsal = _msal.CreateForIdentifier(User, true);
                // api mismatch - azure cn doesn't include CA policy so maybe this is a wasted effort
                await cnMsal.AcquireTokenByAuthorizationCode(new[] { "User.Read" }, code).ExecuteAsync();
            }

            Response.Redirect("/aad/authorize");
        }
    }
}