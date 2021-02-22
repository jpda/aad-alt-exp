using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage;
using aad_alt_exp.UserVariableTokenCache;
using Microsoft.WindowsAzure.Storage.Table;

namespace aad_alt_exp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAdB2C"))
                // dummy scope for sign-in; b2c-only thing for now.
                .EnableTokenAcquisitionToCallDownstreamApi(new[] { Configuration.GetValue<string>("AzureAdB2C:InitialScopes") })
                .AddInMemoryTokenCaches() // cache here doesn't really matter since we're not accessing B2C APIs
                ;

            // used to force authorization_code flow on sign-in, no hybrid flow 'ere
            services.Configure<MicrosoftIdentityOptions>(opts =>
            {
                opts.ResponseType = "code";

            });

            // we have two options for UX for user authorization:
            // cache the user's AAD tokens, which requires reliably and securely knowing who they are (B2C object <--> AAD object map)
            // but results in a better user experience - no re-authentication between sign-in sessions
            // alternatively, you could ask the user to sign-in to AAD once during each session, which removes the mapping and caching requirement
            // this sample will use once-per-session authorization for simplicity, but will incorporate persistent cache in the future

            var aadConfig = Configuration.GetSection("AzureAdAuthorization");

            // services.AddScoped<Microsoft.Identity.Client.IConfidentialClientApplication>(x =>
            // {

            //     // we use a new MSAL instance here for handling the user's AAD authorization
            //     var cca = ConfidentialClientApplicationBuilder.Create(aadConfig["ClientId"])
            //                 .WithClientSecret(aadConfig["ClientSecret"])
            //                 .WithRedirectUri(aadConfig["RedirectUri"])
            //                 .WithExtraQueryParameters(new Dictionary<string, string>() { { "response_mode", "form_post" } })
            //                 .Build()
            //                 ;

            //     return cca;
            // });

            services.AddSingleton<CloudTable>(x =>
            {
                var sa = CloudStorageAccount.Parse(aadConfig["Cache:ConnectionString"]);
                var table = sa.CreateCloudTableClient().GetTableReference("MsalCache");
                table.CreateIfNotExistsAsync().Wait();
                return table;
            });

            services.Configure<MsalOptions>(x =>
            {
                x.ClientId = aadConfig["ClientId"];
                x.ClientSecret = aadConfig["ClientSecret"];
                x.RedirectUri = aadConfig["RedirectUri"];
            });

            services.AddTransient<ITokenCacheAccessor, PerUserTableTokenCacheAccessor>();
            services.AddTransient<MsalClientFactory>();

            services.AddRazorPages(opts =>
            {
                opts.Conventions.AuthorizeAreaFolder("aad", "/");
                opts.Conventions.AllowAnonymousToPage("/Index");
            })
                .AddMicrosoftIdentityUI();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
