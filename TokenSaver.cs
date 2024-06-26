﻿using ArtworkInbox.Data;
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
                    .AsQueryable()
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
            } else if (info.LoginProvider == "Tumblr") {
                var token = await _context.UserTumblrTokens
                    .AsQueryable()
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
            } else if (info.LoginProvider == "Reddit") {
                var token = await _context.UserRedditTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserRedditToken {
                        UserId = user.Id
                    };
                    _context.UserRedditTokens.Add(token);
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
            } else if (MastodonOAuthExtensions.Hosts.Contains(info.LoginProvider)) {
                var token = await _context.UserMastodonTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserMastodonToken {
                        UserId = user.Id,
                        Host = info.LoginProvider
                    };
                    _context.UserMastodonTokens.Add(token);
                }
                token.AccessToken = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                await _context.SaveChangesAsync();
            } else if (info.LoginProvider == "Weasyl") {
                var token = await _context.UserWeasylTokens
                    .AsQueryable()
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
            } else if (info.LoginProvider == "FurAffinity") {
                var token = await _context.UserFurAffinityTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserFurAffinityToken {
                        UserId = user.Id
                    };
                    _context.UserFurAffinityTokens.Add(token);
                }
                token.FA_COOKIE = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveTokensAsync(ApplicationUser user, string loginProvider) {
            if (loginProvider == "DeviantArt") {
                var token = await _context.UserDeviantArtTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token != null) {
                    _context.Remove(token);
                    await _context.SaveChangesAsync();
                }
            } else if (loginProvider == "Tumblr") {
                var token = await _context.UserTumblrTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token != null) {
                    _context.Remove(token);
                    await _context.SaveChangesAsync();
                }
            } else if (loginProvider == "Reddit") {
                var token = await _context.UserRedditTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token != null) {
                    _context.Remove(token);
                    await _context.SaveChangesAsync();
                }
            } else if (MastodonOAuthExtensions.Hosts.Contains(loginProvider)) {
                var tokens = await _context.UserMastodonTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .Where(t => t.Host == loginProvider)
                    .ToListAsync();
                if (tokens.Any()) {
                    _context.RemoveRange(tokens);
                    await _context.SaveChangesAsync();
                }
            } else if (loginProvider == "Weasyl") {
                var token = await _context.UserWeasylTokens
                    .AsQueryable()
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
