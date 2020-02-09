using System.ComponentModel.DataAnnotations;

namespace ArtworkInbox.Models {
    public class InoreaderSettingsViewModel {
        [Display(Name = "Treat all items as blog posts (ignore thumbnails)")]
        public bool AllAsText { get; set; }

        [Display(Name = "Skip items marked as read on Inoreader")]
        public bool UnreadOnly { get; set; }
    }
}
