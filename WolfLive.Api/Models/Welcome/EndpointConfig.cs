using Newtonsoft.Json;
using System;

namespace WolfLive.Api.Models.Welcome
{
    public class EndpointConfig
    {
        [JsonProperty("avatarEndpoint")]
        public Uri AvatarEndpoint { get; set; }

        [JsonProperty("mmsUploadEndpoint")]
        public Uri MmsUploadEndpoint { get; set; }

        [JsonProperty("banner")]
        public Banner Banner { get; set; }
    }
}
