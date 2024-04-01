Artwork Inbox
=============

https://artworkinbox.azurewebsites.net/

An ASP.NET Core web application that provides an alternate UI for viewing your feed.

Supported sites:

* [DeviantArt](https://www.deviantart.com/)
* [Fur Affinity](https://www.furaffinity.net/) via [FAExport](faexport.spangle.org.uk)
* [Tumblr](https://www.tumblr.com)
* [Reddit](https://www.reddit.com/new/)
* [Weasyl](https://www.weasyl.com) (artwork only; API key required)

Support can also be added for a Mastodon server by editing Startup.cs:

    services.AddMastodon("example.com", o => {
        o.Scope.Add("read:statuses");
        o.Scope.Add("read:accounts");
        o.Scope.Add("read:notifications");
        o.ClientId = Configuration["Authentication:Mastodon:example.com:client_id"];
        o.ClientSecret = Configuration["Authentication:Mastodon:example.com:client_secret"];
        o.SaveTokens = true;
    });
