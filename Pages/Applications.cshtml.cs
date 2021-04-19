using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        public IEnumerable<ApplicationModel> Applications;
        public string ErrorText;
        public string Token;
        private readonly ILogger<ClaimsModel> _logger;
        private readonly MsalClientFactory _msal;
        private readonly HttpClient _graphClient;
        private readonly List<string> _appScopes = new() { "Application.ReadWrite.All", "Policy.Read.All", "Policy.ReadWrite.ConditionalAccess" };

        public ApplicationsModel(ILogger<ClaimsModel> logger, MsalClientFactory msal, System.Net.Http.IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _msal = msal;
            _graphClient = httpClientFactory.CreateClient();
        }

        public async Task OnGet()
        {
            _logger.LogDebug($"Getting Graph token for {User.DeriveUserIdentifier()} for scopes {_appScopes}");
            var msal = _msal.CreateForIdentifier(User);
            var token = await msal.AcquireTokenSilent(_appScopes, (await msal.GetAccountsAsync()).First()).ExecuteAsync();
            Token = token.AccessToken;

            _graphClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);
            var apps = await _graphClient.GetAsync("https://graph.microsoft.com/v1.0/applications?$top=10"); //small page
            if (!apps.IsSuccessStatusCode)
            {
                ErrorText = apps.StatusCode.ToString(); return;
            }
            Applications = JsonSerializer.Deserialize<GraphResponse<ApplicationModel>>(await apps.Content.ReadAsStringAsync()).Value;
        }
    }

    public class GraphResponse<T>
    {
        [JsonPropertyName("@odata.context")]
        public string Context { get; set; }

        [JsonPropertyName("@odata.nextLink")]
        public string NextLink { get; set; }

        [JsonPropertyName("value")]
        public IEnumerable<T> Value { get; set; }
    }

    public class ApplicationModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("appId")]
        public string AppId { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonIgnore]
        public string Url { get; set; }

        [JsonPropertyName("createdDateTime")]
        public DateTime? Created { get; set; }

        [JsonPropertyName("web")]
        public AppPlatformModel WebPlatform { get; set; }
    }

    public class AppPlatformModel
    {
        [JsonPropertyName("homePageUrl")]
        public string HomePageUrl { get; set; }

        [JsonPropertyName("logoutUrl")]
        public string LogoutUrl { get; set; }

        [JsonPropertyName("redirectUris")]
        public IEnumerable<string> RedirectUris { get; set; }

    }
}