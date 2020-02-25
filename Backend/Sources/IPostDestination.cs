using System;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public interface IPostDestination {
        Task<string> GetProfileUrlAsync();
        Task<Uri> PostStatusAsync(string text);
    }
}
