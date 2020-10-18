using Newtonsoft.Json;

namespace WolfLive.Api.Models.Welcome
{
    public class Banner
    {
        [JsonProperty("notification")]
        public LanguageUris Notification { get; set; }

        [JsonProperty("promotion")]
        public LanguageUris Promotion { get; set; }
    }
}
