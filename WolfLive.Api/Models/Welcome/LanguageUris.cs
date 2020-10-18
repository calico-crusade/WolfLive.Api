using Newtonsoft.Json;
using System;

namespace WolfLive.Api.Models.Welcome
{
	public class LanguageUris
	{
		[JsonProperty("en")]
		public Uri English { get; set; }

		[JsonProperty("ar")]
		public Uri Arabic { get; set; }
	}
}
