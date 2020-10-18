using Newtonsoft.Json;

namespace WolfLive.Api.Models
{
    public class Group : IdHash
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("reputation")]
        public double Reputation { get; set; }

        [JsonProperty("premium")]
        public bool Premium { get; set; }

        [JsonProperty("official")]
        public bool Official { get; set; }

        [JsonProperty("icon")]
        public long? Icon { get; set; }

        [JsonProperty("ownerId")]
        public long? OwnerId { get; set; }

        [JsonProperty("discoverable")]
        public bool Discoverable { get; set; }

        [JsonProperty("advancedAdmin")]
        public bool AdvancedAdmin { get; set; }

        [JsonProperty("peekable")]
        public bool Peekable { get; set; }

        [JsonProperty("members")]
        public long Members { get; set; }

        [JsonProperty("owner")]
        public User Owner { get; set; }
    }
}
