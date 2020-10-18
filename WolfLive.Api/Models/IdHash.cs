using Newtonsoft.Json;

namespace WolfLive.Api.Models
{
	public class IdHash
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("hash")]
		public string Hash { get; set; }
	}
}
