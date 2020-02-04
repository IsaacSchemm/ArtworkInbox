using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ArtworkInbox.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DeviantArtFs;
using Tweetinvi.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

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
                .AddReddit(o => {
                    o.Scope.Add("read");
                    o.ClientId = Configuration["Authentication:Reddit:ClientId"];
                    o.ClientSecret = Configuration["Authentication:Reddit:ClientSecret"];
                    o.SaveTokens = true;
                })
                .AddOAuth("Weasyl", "Weasyl", o => {
                    o.ClientId = Configuration["Authentication:Weasyl:ClientId"];
                    o.ClientSecret = Configuration["Authentication:Weasyl:ClientSecret"];
                    o.AuthorizationEndpoint = "https://artworkinbox-weasyl-oauth.azurewebsites.net/api/auth";
                    o.TokenEndpoint = "https://artworkinbox-weasyl-oauth.azurewebsites.net/api/token";
                    o.CallbackPath = new PathString("/signin-weasyl");
                    o.SaveTokens = true;

                    o.Events = new OAuthEvents {
                        OnCreatingTicket = async context => {
                            if (context.Options.SaveTokens) {
                                context.Properties.StoreTokens(new[] {
                                    new AuthenticationToken { Name = "access_token", Value = context.AccessToken }
                                });
                            }

                            var creds = new WeasylFs.WeasylCredentials(context.AccessToken);
                            var user = await WeasylFs.Endpoints.Whoami.ExecuteAsync(creds);
                            context.Principal.AddIdentity(new ClaimsIdentity(new[] {
                                new Claim(ClaimTypes.NameIdentifier, $"{user.userid}"),
                                new Claim(ClaimTypes.Name, user.login),
                                new Claim("urn:weasyl:userid", $"{user.userid}"),
                                new Claim("urn:weasyl:login", user.login),
                            }));
                        },
                        OnRemoteFailure = context => {
                            context.HandleResponse();
                            context.Response.Redirect("/Home/Error");
                            return Task.FromResult(0);
                        }
                    };
                })
                .AddMastodon("mastodon.social", o => {
                    o.Scope.Add("read:statuses");
                    o.Scope.Add("read:accounts");
                    o.ClientId = Configuration["Authentication:Mastodon:mastodon.social:client_id"];
                    o.ClientSecret = Configuration["Authentication:Mastodon:mastodon.social:client_secret"];
                    o.SaveTokens = true;
                })
                .AddMastodon("botsin.space", o => {
                    o.Scope.Add("read:statuses");
                    o.Scope.Add("read:accounts");
                    o.ClientId = Configuration["Authentication:Mastodon:botsin.space:client_id"];
                    o.ClientSecret = Configuration["Authentication:Mastodon:botsin.space:client_secret"];
                    o.SaveTokens = true;
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
            services.AddSingleton(new ArtworkInboxRedditClientFactory(
                Configuration["Authentication:Reddit:ClientId"],
                Configuration["Authentication:Reddit:ClientSecret"]));
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
