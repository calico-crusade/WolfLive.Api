using Newtonsoft.Json;

namespace WolfLive.Api.Models
{
    using Welcome;

	public class WelcomePacket
	{
        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("endpointConfig")]
        public EndpointConfig EndpointConfig { get; set; }

        [JsonProperty("loggedInUser")]
        public User LoggedInUser { get; set; }
    }
}
