using System.Collections.Generic;

namespace WolfLive.Api.Commands.Common
{
	public class AuthModel
	{
		public Dictionary<string, UserRole> GloablRoles { get; set; } = new Dictionary<string, UserRole>();

		public Dictionary<string, Dictionary<string, UserRole>> GroupRoles { get; set; } = new Dictionary<string, Dictionary<string, UserRole>>();
	}
}
