using Newtonsoft.Json;

namespace WolfLive.Api.Models
{
    public class Extended
    {
        [JsonProperty("language")]
        public int? Language { get; set; }

        [JsonProperty("urls")]
        public object Urls { get; set; }

        [JsonProperty("lookingFor")]
        public int? LookingFor { get; set; }

        [JsonProperty("dateOfBirth")]
        public object DateOfBirth { get; set; }

        [JsonProperty("relationship")]
        public int? Relationship { get; set; }

        [JsonProperty("gender")]
        public int? Gender { get; set; }

        [JsonProperty("about")]
        public string About { get; set; }

        [JsonProperty("optOut")]
        public object OptOut { get; set; }

        [JsonProperty("utcOffset")]
        public object UtcOffset { get; set; }

        [JsonProperty("latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("longitude")]
        public double? Longitude { get; set; }

        [JsonProperty("name1")]
        public string Name1 { get; set; }

        [JsonProperty("after")]
        public object After { get; set; }

        [JsonProperty("dobD")]
        public int? DobD { get; set; }

        [JsonProperty("dobM")]
        public int? DobM { get; set; }

        [JsonProperty("dobY")]
        public int? DobY { get; set; }

        [JsonProperty("relationshipStatus")]
        public int? RelationshipStatus { get; set; }

        [JsonProperty("sex")]
        public int? Sex { get; set; }
    }
}
