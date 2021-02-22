using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace aad_alt_exp.Pages
{
    public class ClaimsModel : PageModel
    {
        public IList<DisplayClaim> UserClaims;

        private readonly ILogger<ClaimsModel> _logger;

        public ClaimsModel(ILogger<ClaimsModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // definitely get weird problems serializing the claims collection,
            // so just picking out the little pieces we need
            this.UserClaims = User.Claims.Select(x => new DisplayClaim(x)).ToList();
        }
    }

    public class DisplayClaim
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public DisplayClaim(System.Security.Claims.Claim c)
        {
            this.Type = c.Type;
            this.Value = c.Value;
        }
    }
}
