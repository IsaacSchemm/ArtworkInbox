using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using ArtworkInbox.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DeviantArtFs;
using Tweetinvi.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace ArtworkInbox {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"),
                    o => o.EnableRetryOnFailure()));
            services.AddAuthentication()
                .AddDeviantArt(d => {
                    d.Scope.Add("feed");
                    d.ClientId = Configuration["Authentication:DeviantArt:ClientId"];
                    d.ClientSecret = Configuration["Authentication:DeviantArt:ClientSecret"];
                    d.SaveTokens = true;
                })
                .AddTwitter(t => {
                    t.ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"];
                    t.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
                    t.SaveTokens = true;
                })
                .AddTumblr(t => {
                    t.ConsumerKey = Configuration["Authentication:Tumblr:ConsumerKey"];
                    t.ConsumerSecret = Configuration["Authentication:Tumblr:ConsumerSecret"];
                    t.SaveTokens = true;
                })
                .AddOAuth("botsin.space", o => {
                    // https://medium.com/@mauridb/using-oauth2-middleware-with-asp-net-core-2-0-b31ffef58cd0

                    o.ClientId = Configuration["Authentication:botsin.space:ClientId"];
                    o.ClientSecret = Configuration["Authentication:botsin.space:ClientSecret"];

                    o.AuthorizationEndpoint = "https://botsin.space/oauth/authorize";
                    o.TokenEndpoint = "https://botsin.space/oauth/token";
                    o.UserInformationEndpoint = "https://botsin.space/api/v1/accounts/verify_credentials";
                    o.CallbackPath = new Microsoft.AspNetCore.Http.PathString("/signin-botsin-space");

                    o.Scope.Add("read");

                    o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    o.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
                    o.ClaimActions.MapJsonKey("urn:botsin.space:id", "id");
                    o.ClaimActions.MapJsonKey("urn:botsin.space:username", "username");

                    o.Events = new OAuthEvents {
                        OnCreatingTicket = async context => {
                            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                            response.EnsureSuccessStatusCode();

                            //context.HttpContext.Response.Cookies.Append("token", context.AccessToken);
                            if (context.Options.SaveTokens) {
                                context.Properties.StoreTokens(new[] {
                                    new AuthenticationToken { Name = "access_token", Value = context.AccessToken }
                                });
                            }

                            using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                            context.RunClaimActions(user.RootElement);
                        },
                        OnRemoteFailure = context => {
                            context.HandleResponse();
                            context.Response.Redirect("/Home/Error?message=" + context.Failure.Message);
                            return Task.FromResult(0);
                        }
                    };
                });
            services.AddSingleton<IDeviantArtAuth>(new DeviantArtAuth(
                int.Parse(Configuration["Authentication:DeviantArt:ClientId"]),
                Configuration["Authentication:DeviantArt:ClientSecret"]));
            services.AddSingleton<IConsumerCredentials>(new ConsumerCredentials(
                Configuration["Authentication:Twitter:ConsumerKey"],
                Configuration["Authentication:Twitter:ConsumerSecret"]));
            services.AddSingleton(new ArtworkInboxTumblrClientFactory(
                Configuration["Authentication:Tumblr:ConsumerKey"],
                Configuration["Authentication:Tumblr:ConsumerSecret"]));
            services.AddDefaultIdentity<ApplicationUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            } else {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
