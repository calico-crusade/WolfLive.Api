using Newtonsoft.Json;
using System;

namespace WolfLive.Api.Models
{
	public class MessageResponse
	{
		[JsonProperty("uuid")]
		public string Uuid { get; set; }

		[JsonProperty("timestamp")]
		public long TimestampEpoch { get; set; }

		[JsonIgnore]
		public DateTime Timestamp => new DateTime(TimestampEpoch);

		[JsonIgnore]
		public bool Success => !string.IsNullOrEmpty(Uuid);
	}
}
