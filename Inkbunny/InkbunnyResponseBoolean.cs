using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtworkInbox.Inkbunny {
	[JsonConverter(typeof(InkbunnyResponseBooleanConverter))]
	public struct InkbunnyResponseBoolean {
		public readonly bool value;

        public InkbunnyResponseBoolean(bool v) {
            value = v;
        }

		public static implicit operator bool(InkbunnyResponseBoolean i) {
			return i.value;
        }

        public static implicit operator InkbunnyResponseBoolean(bool v) {
            return new InkbunnyResponseBoolean(v);
        }

        public override string ToString() {
			return value ? "t" : "f";
		}
	}

	public class InkbunnyResponseBooleanConverter : JsonConverter {
		public override bool CanConvert(Type objectType) {
			return objectType == typeof(InkbunnyResponseBoolean);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			string v = reader.Value?.ToString();
            if (v == "t") return new InkbunnyResponseBoolean(true);
            if (v == "f") return new InkbunnyResponseBoolean(false);
			throw new JsonReaderException("Expected value of 't' or 'f' for InkbunnyTFBoolean. Path: " + reader.Path);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			var i = (InkbunnyResponseBoolean)value;
			writer.WriteValue(i ? "t" : "f");
		}
	}
}
