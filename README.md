Artwork Inbox
=============

https://artworkinbox.azurewebsites.net/

An ASP.NET Core web application that provides an alternate UI for viewing your feed.

Supported sites:

* [DeviantArt](https://www.deviantart.com/)
* [Twitter](https://www.twitter.com)
* [Tumblr](https://www.tumblr.com)
* [Reddit](https://www.reddit.com/new/)
* [Weasyl](https://www.weasyl.com) (artwork only; API key required)
* Mastodon
    * [mastodon.social](https://mastodon.social/)
    * [mstdn.jp](https://mstdn.jp/)
    * [botsin.space](https://botsin.space/) (for debugging only)
    * file an issue on GitHub if you have another one you want added!

Note: clearing your feed items from within this app does not actually clear
them from your feed - it just hides them from this app's view. (This is
because most of the APIs this app can connect with do not actually support
marking items as "read".)
