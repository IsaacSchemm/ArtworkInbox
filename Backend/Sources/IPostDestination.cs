using ArtworkInbox.Backend.Types;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public interface IPostDestination {
        Task<string> GetProfileUrlAsync();
        Task PostStatusAsync(string text);
    }
}
