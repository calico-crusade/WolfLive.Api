using System.Collections.Generic;

namespace WolfLive.Api.Commands.Common
{
	public class AuthModel
	{
		public List<string> SuperUsers { get; set; } = new List<string>();

		public Dictionary<string, string> GloablRoles { get; set; } = new Dictionary<string, string>();

		public Dictionary<string, Dictionary<string, string>> GroupRoles { get; set; } = new Dictionary<string, Dictionary<string, string>>();
	}
}
