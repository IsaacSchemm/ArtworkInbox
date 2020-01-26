using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtworkInbox.Inkbunny.Responses {
    public class InkbunnyEditSubmissionResponse : InkbunnyResponse {
        public int submission_id;
        public bool? twitter_authentication_success;
    }
}
