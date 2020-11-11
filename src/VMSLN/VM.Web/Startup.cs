using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using VM.Web.Hub;
using VM.Web.Interfaces;
using VM.Web.Options;
using VM.Web.Services;

namespace VM.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AzureAdOptions>(Configuration.GetSection("AzureAd"));
            services.Configure<SendGridOptions>(Configuration.GetSection("SendGridOptions"));
            services.AddSignalR();
            
            var sendGridSettings = Configuration.GetSection("SendGridOptions").Get<SendGridOptions>();
            services.AddScoped<IEmailService, SendGridEmailSender>(
                _ => new SendGridEmailSender(sendGridSettings.ApiKey));

            services.AddScoped<IAzureVmService, AzureVmService>();
            
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            services.AddHttpContextAccessor();
            
            services.AddMicrosoftIdentityWebAppAuthentication(Configuration);
            services.AddControllersWithViews().AddMicrosoftIdentityUI();
            
            services.AddRazorPages().AddRazorPagesOptions(options =>
            {
                options.Conventions.AddPageRoute("/Info/Index", "");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Error");

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<NotificationHub>("/notification");
            });
        }
    }
}