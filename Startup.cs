using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ArtworkInbox.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DeviantArtFs;
using Tweetinvi.Models;
using Google.Apis.Auth.OAuth2;

namespace ArtworkInbox {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddHttpClient();
            if (Configuration.GetConnectionString("CosmosDB") is string cosmosDb) {
                services.AddDbContext<ApplicationDbContext>(options => {
                    options.UseCosmos(cosmosDb, "ArtworkInbox");
                });
            } else {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(
                        Configuration.GetConnectionString("DefaultConnection"),
                        o => o.EnableRetryOnFailure()));
            }
            services.AddAuthentication()
                .AddDeviantArt(d => {
                    d.Scope.Add("browse");
                    d.Scope.Add("message");
                    d.ClientId = Configuration["Authentication:DeviantArt:ClientId"];
                    d.ClientSecret = Configuration["Authentication:DeviantArt:ClientSecret"];
                    d.SaveTokens = true;
                })
                .AddReddit(o => {
                    o.Scope.Add("read");
                    o.Scope.Add("privatemessages");
                    o.ClientId = Configuration["Authentication:Reddit:ClientId"];
                    o.ClientSecret = Configuration["Authentication:Reddit:ClientSecret"];
                    o.SaveTokens = true;
                })
                .AddTumblr(t => {
                    t.ConsumerKey = Configuration["Authentication:Tumblr:ConsumerKey"];
                    t.ConsumerSecret = Configuration["Authentication:Tumblr:ConsumerSecret"];
                    t.SaveTokens = true;
                })
                .AddTwitter(t => {
                    t.ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"];
                    t.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
                    t.SaveTokens = true;
                })
                .AddMastodon("mastodon.social", o => {
                    o.Scope.Add("read:statuses");
                    o.Scope.Add("read:accounts");
                    o.Scope.Add("read:notifications");
                    o.ClientId = Configuration["Authentication:Mastodon:mastodon.social:client_id"];
                    o.ClientSecret = Configuration["Authentication:Mastodon:mastodon.social:client_secret"];
                    o.SaveTokens = true;
                })
                .AddMastodon("mstdn.jp", o => {
                    o.Scope.Add("read:statuses");
                    o.Scope.Add("read:accounts");
                    o.Scope.Add("read:notifications");
                    o.ClientId = Configuration["Authentication:Mastodon:mstdn.jp:client_id"];
                    o.ClientSecret = Configuration["Authentication:Mastodon:mstdn.jp:client_secret"];
                    o.SaveTokens = true;
                })
                .AddMastodon("botsin.space", o => {
                    o.Scope.Add("read:statuses");
                    o.Scope.Add("read:accounts");
                    o.Scope.Add("read:notifications");
                    o.ClientId = Configuration["Authentication:Mastodon:botsin.space:client_id"];
                    o.ClientSecret = Configuration["Authentication:Mastodon:botsin.space:client_secret"];
                    o.SaveTokens = true;
                })
                .AddGoogle("YouTube", "YouTube", o => {
                    o.Scope.Add("https://www.googleapis.com/auth/youtube.readonly");
                    o.AccessType = "offline";
                    o.ClientId = Configuration["Authentication:Google:ClientId"];
                    o.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                    o.SaveTokens = true;
                });
            services.AddSingleton(new DeviantArtApp(
                Configuration["Authentication:DeviantArt:ClientId"],
                Configuration["Authentication:DeviantArt:ClientSecret"]));
            services.AddSingleton<IReadOnlyConsumerCredentials>(new ReadOnlyConsumerCredentials(
                Configuration["Authentication:Twitter:ConsumerKey"],
                Configuration["Authentication:Twitter:ConsumerSecret"]));
            services.AddSingleton(new ArtworkInboxTumblrClientFactory(
                Configuration["Authentication:Tumblr:ConsumerKey"],
                Configuration["Authentication:Tumblr:ConsumerSecret"]));
            services.AddSingleton(new ArtworkInboxRedditCredentials(
                Configuration["Authentication:Reddit:ClientId"],
                Configuration["Authentication:Reddit:ClientSecret"]));
            services.AddSingleton(new ClientSecrets {
                ClientId = Configuration["Authentication:Google:ClientId"],
                ClientSecret = Configuration["Authentication:Google:ClientSecret"]
            });
            services.AddDefaultIdentity<ApplicationUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();    
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
