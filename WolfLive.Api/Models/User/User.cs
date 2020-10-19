using Newtonsoft.Json;

namespace WolfLive.Api.Models
{
    public class User : IdHash
    {
        [JsonProperty("privileges")]
        public long? PrivilegesFlag { get; set; }

        [JsonIgnore]
        public PrivilegeType Privileges => (PrivilegeType)(PrivilegesFlag ?? 0);

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("reputation")]
        public double? Reputation { get; set; }

        [JsonProperty("icon")]
        public int? Icon { get; set; }

        [JsonProperty("onlineState")]
        public int? OnlineState { get; set; }

        [JsonProperty("deviceType")]
        public int? DeviceType { get; set; }

        [JsonProperty("contactListBlockedState")]
        public int? ContactListBlockedState { get; set; }

        [JsonProperty("contactListAuthState")]
        public int? ContactListAuthState { get; set; }

        [JsonProperty("charms")]
        public object Charms { get; set; }

        [JsonProperty("extended")]
        public Extended Extended { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
