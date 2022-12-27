using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox {
    public static class DatabaseExtensions {
        public static async Task<bool> ListAnyAsync<T>(this IQueryable<T> q) {
            var list = await q.Take(1).ToListAsync();
            return list.Any();
        }
    }
}
