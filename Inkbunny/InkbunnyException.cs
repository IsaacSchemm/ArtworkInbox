using ArtworkInbox.Inkbunny.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArtworkInbox.Inkbunny {
	public class InkbunnyException : Exception {
        public InkbunnyResponse Response { get; }

        public InkbunnyException(InkbunnyResponse response) {
            Response = response ?? throw new ArgumentNullException(nameof(response));
        }
    }
}
