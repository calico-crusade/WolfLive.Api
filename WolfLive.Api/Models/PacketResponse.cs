using Newtonsoft.Json;

namespace WolfLive.Api
{
	public class PacketResponse
	{
		[JsonProperty("code")]
		public int Code { get; set; }

		[JsonIgnore]
		public bool Successful => Code >= 200 && Code < 300;
	}

	public class PacketResponse<T> : PacketResponse
	{
		[JsonProperty("body")]
		public T Body { get; set; }
	}
}
