using System.Collections.Generic;

namespace WolfLive.Api.Commands.Common
{
	public class UserRole
	{
		public string UserId { get; set; }
		public List<string> Roles { get; set; } = new List<string>();
	}
}
