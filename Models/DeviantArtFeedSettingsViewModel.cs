using DeviantArtFs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DANotify.Models {
    public class DeviantArtFeedSettingsViewModel {
        public bool Statuses { get; set; }

        public bool Deviations { get; set; }

        public bool Journals { get; set; }

        [Display(Name = "Group deviations")]
        public bool GroupDeviations { get; set; }

        [Display(Name = "Collection updates")]
        public bool Collections { get; set; }

        [Display(Name = "Miscellaneous")]
        public bool Misc { get; set; }
    }
}
