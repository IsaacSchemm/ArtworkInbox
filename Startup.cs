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
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace ArtworkInbox {
    public class Startup {
        private class InoreaderTempToken : InoreaderFs.Auth.OAuth.IAccessToken {
            public string AccessToken { get; set; }
        }

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private string GetConfigurationSetting(string path) {
            IConfiguration conf = Configuration;
            Queue<string> split = new Queue<string>(path.Split(':'));
            while (split.Count > 1)
                conf = conf.GetSection(split.Dequeue());
            return conf[split.Single()];
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"),
                    o => o.EnableRetryOnFailure()));
            services.AddAuthentication()
                .AddDeviantArt(d => {
                    d.Scope.Add("browse");
                    d.Scope.Add("message");
                    d.ClientId = GetConfigurationSetting("Authentication:DeviantArt:ClientId");
                    d.ClientSecret = GetConfigurationSetting("Authentication:DeviantArt:ClientSecret");
                    d.SaveTokens = true;
                })
                .AddOAuth("Inoreader", "Inoreader", o => {
                    o.ClientId = GetConfigurationSetting("Authentication:Inoreader:AppId");
                    o.ClientSecret = GetConfigurationSetting("Authentication:Inoreader:AppKey");
                    o.AuthorizationEndpoint = "https://www.inoreader.com/oauth2/auth";
                    o.TokenEndpoint = "https://www.inoreader.com/oauth2/token";
                    o.CallbackPath = new PathString("/signin-inoreader");
                    o.SaveTokens = true;

                    o.Events = new OAuthEvents {
                        OnCreatingTicket = async context => {
                            if (context.Options.SaveTokens) {
                                context.Properties.StoreTokens(new[] {
                                    new AuthenticationToken { Name = "access_token", Value = context.AccessToken },
                                    new AuthenticationToken { Name = "refresh_token", Value = context.RefreshToken }
                                });
                            }

                            var token = new InoreaderTempToken { AccessToken = context.AccessToken };
                            var credentials = InoreaderFs.Auth.Credentials.NewOAuth(token);
                            var user = await InoreaderFs.Endpoints.UserInfo.ExecuteAsync(credentials);
                            context.Principal.AddIdentity(new ClaimsIdentity(new[] {
                                new Claim(ClaimTypes.NameIdentifier, $"{user.userId}"),
                                new Claim(ClaimTypes.Name, user.userName),
                                new Claim("urn:inoreader:userid", $"{user.userId}"),
                                new Claim("urn:inoreader:username", user.userName),
                            }));
                        },
                        OnRemoteFailure = context => {
                            context.HandleResponse();
                            context.Response.Redirect("/Home/Error");
                            return Task.FromResult(0);
                        }
                    };
                })
                .AddReddit(o => {
                    o.Scope.Add("read");
                    o.ClientId = GetConfigurationSetting("Authentication:Reddit:ClientId");
                    o.ClientSecret = GetConfigurationSetting("Authentication:Reddit:ClientSecret");
                    o.SaveTokens = true;
                })
                .AddTumblr(t => {
                    t.ConsumerKey = GetConfigurationSetting("Authentication:Tumblr:ConsumerKey");
                    t.ConsumerSecret = GetConfigurationSetting("Authentication:Tumblr:ConsumerSecret");
                    t.SaveTokens = true;
                })
                .AddTwitter(t => {
                    t.ConsumerKey = GetConfigurationSetting("Authentication:Twitter:ConsumerKey");
                    t.ConsumerSecret = GetConfigurationSetting("Authentication:Twitter:ConsumerSecret");
                    t.SaveTokens = true;
                })
                .AddOAuth("Weasyl", "Weasyl", o => {
                    o.ClientId = GetConfigurationSetting("Authentication:Weasyl:ClientId");
                    o.ClientSecret = GetConfigurationSetting("Authentication:Weasyl:ClientSecret");
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
                .AddOAuth("FurAffinity", "FurAffinity", o => {
                    o.ClientId = GetConfigurationSetting("Authentication:FurAffinity:ClientId");
                    o.ClientSecret = GetConfigurationSetting("Authentication:FurAffinity:ClientSecret");
                    o.AuthorizationEndpoint = "https://artworkinbox-furaffinity-oauth.azurewebsites.net/api/auth";
                    o.TokenEndpoint = "https://artworkinbox-furaffinity-oauth.azurewebsites.net/api/token";
                    o.CallbackPath = new PathString("/signin-furaffinity");
                    o.SaveTokens = true;

                    o.Events = new OAuthEvents {
                        OnCreatingTicket = async context => {
                            if (context.Options.SaveTokens) {
                                context.Properties.StoreTokens(new[] {
                                    new AuthenticationToken { Name = "access_token", Value = context.AccessToken }
                                });
                            }

                            var notifications = await FurAffinity.Notifications.GetSubmissionsAsync(context.AccessToken, sfw: true, from: 0);
                            context.Principal.AddIdentity(new ClaimsIdentity(new[] {
                                new Claim(ClaimTypes.NameIdentifier, $"{notifications.current_user.profile_name}"),
                                new Claim(ClaimTypes.Name, notifications.current_user.name),
                                new Claim("urn:furaffinity:name", notifications.current_user.name),
                                new Claim("urn:furaffinity:profile", notifications.current_user.profile),
                                new Claim("urn:furaffinity:profile_name", notifications.current_user.profile_name)
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
                    o.ClientId = GetConfigurationSetting("Authentication:Mastodon:mastodon.social:client_id");
                    o.ClientSecret = GetConfigurationSetting("Authentication:Mastodon:mastodon.social:client_secret");
                    o.SaveTokens = true;
                })
                .AddMastodon("botsin.space", o => {
                    o.Scope.Add("read:statuses");
                    o.Scope.Add("read:accounts");
                    o.ClientId = GetConfigurationSetting("Authentication:Mastodon:botsin.space:client_id");
                    o.ClientSecret = GetConfigurationSetting("Authentication:Mastodon:botsin.space:client_secret");
                    o.SaveTokens = true;
                });
            services.AddSingleton(new DeviantArtApp(
                GetConfigurationSetting("Authentication:DeviantArt:ClientId"),
                GetConfigurationSetting("Authentication:DeviantArt:ClientSecret")));
            services.AddSingleton<IReadOnlyConsumerCredentials>(new ReadOnlyConsumerCredentials(
                GetConfigurationSetting("Authentication:Twitter:ConsumerKey"),
                GetConfigurationSetting("Authentication:Twitter:ConsumerSecret")));
            services.AddSingleton(new ArtworkInboxTumblrClientFactory(
                GetConfigurationSetting("Authentication:Tumblr:ConsumerKey"),
                GetConfigurationSetting("Authentication:Tumblr:ConsumerSecret")));
            services.AddSingleton(new ArtworkInboxRedditCredentials(
                GetConfigurationSetting("Authentication:Reddit:ClientId"),
                GetConfigurationSetting("Authentication:Reddit:ClientSecret")));
            services.AddSingleton(new InoreaderFs.Auth.App(
                GetConfigurationSetting("Authentication:Inoreader:AppId"),
                GetConfigurationSetting("Authentication:Inoreader:AppKey")));
            services.AddDefaultIdentity<ApplicationUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment() || DateTime.UtcNow < new DateTime(2020, 2, 14, 4, 0, 0, DateTimeKind.Utc)) {
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
