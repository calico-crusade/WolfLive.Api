using Newtonsoft.Json;

namespace WolfLive.Api.Models
{
	public class GroupUserUpdate
	{
		[JsonProperty("id")]
		public int Id { get => SubscriberId; set => SubscriberId = value; }

		[JsonProperty("groupId")]
		public int GroupId { get; set; }

		[JsonProperty("subscriberId")]
		public int SubscriberId { get; set; }

		[JsonProperty("capabilities")]
		public GroupUserType? Capabilities { get; set; }
	}
}
