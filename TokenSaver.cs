using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox {
    public class TokenSaver {
        private readonly ApplicationDbContext _context;

        public TokenSaver(ApplicationDbContext context) {
            _context = context;
        }

        public async Task UpdateTokensAsync(IdentityUser user, ExternalLoginInfo info) {
            if (info.LoginProvider == "DeviantArt") {
                var token = await _context.UserDeviantArtTokens
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserDeviantArtToken {
                        UserId = user.Id
                    };
                    _context.UserDeviantArtTokens.Add(token);
                }
                token.AccessToken = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                token.RefreshToken = info.AuthenticationTokens
                    .Where(t => t.Name == "refresh_token")
                    .Select(t => t.Value)
                    .Single();
                await _context.SaveChangesAsync();
            } else if (info.LoginProvider == "Twitter") {
                var token = await _context.UserTwitterTokens
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserTwitterToken {
                        UserId = user.Id
                    };
                    _context.UserTwitterTokens.Add(token);
                }
                token.AccessToken = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                token.AccessTokenSecret = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token_secret")
                    .Select(t => t.Value)
                    .Single();
                await _context.SaveChangesAsync();
            } else if (info.LoginProvider == "Tumblr") {
                var token = await _context.UserTumblrTokens
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserTumblrToken {
                        UserId = user.Id
                    };
                    _context.UserTumblrTokens.Add(token);
                }
                token.AccessToken = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                token.AccessTokenSecret = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token_secret")
                    .Select(t => t.Value)
                    .Single();
                await _context.SaveChangesAsync();
            } else if (info.LoginProvider == "botsin.space") {
                var token = await _context.UserBotsinSpaceTokens
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserBotsinSpaceToken {
                        UserId = user.Id
                    };
                    _context.UserBotsinSpaceTokens.Add(token);
                }
                token.AccessToken = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                await _context.SaveChangesAsync();
            } else if (info.LoginProvider == "Weasyl") {
                var token = await _context.UserWeasylTokens
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserWeasylToken {
                        UserId = user.Id
                    };
                    _context.UserWeasylTokens.Add(token);
                }
                token.ApiKey = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                await _context.SaveChangesAsync();
            } else if (info.LoginProvider == "Inkbunny") {
                var token = await _context.UserInkbunnyTokens
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserInkbunnyToken {
                        UserId = user.Id
                    };
                    _context.UserInkbunnyTokens.Add(token);
                }
                token.Sid = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                token.Username = info.Principal.Claims
                    .Where(t => t.Type == "urn:inkbunny:username")
                    .Select(t => t.Value)
                    .Single();
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveTokensAsync(ApplicationUser user, string loginProvider) {
            if (loginProvider == "DeviantArt") {
                var token = await _context.UserDeviantArtTokens
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token != null) {
                    _context.Remove(token);
                    await _context.SaveChangesAsync();
                }
            } else if (loginProvider == "Twitter") {
                var token = await _context.UserTwitterTokens
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token != null) {
                    _context.Remove(token);
                    await _context.SaveChangesAsync();
                }
            } else if (loginProvider == "Tumblr") {
                var token = await _context.UserTumblrTokens
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token != null) {
                    _context.Remove(token);
                    await _context.SaveChangesAsync();
                }
            } else if (loginProvider == "botsin.space") {
                var token = await _context.UserBotsinSpaceTokens
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token != null) {
                    _context.Remove(token);
                    await _context.SaveChangesAsync();
                }
            } else if (loginProvider == "Weasyl") {
                var token = await _context.UserWeasylTokens
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token != null) {
                    _context.Remove(token);
                    await _context.SaveChangesAsync();
                }
            } else if (loginProvider == "Inkbunny") {
                var token = await _context.UserInkbunnyTokens
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token != null) {
                    _context.Remove(token);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
