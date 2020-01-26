using System.ComponentModel.DataAnnotations;

namespace ArtworkInbox.Models {
    public class InkbunnyLoginViewModel {
        public string Username { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
