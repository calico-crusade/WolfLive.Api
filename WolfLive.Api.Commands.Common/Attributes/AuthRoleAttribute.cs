using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace WolfLive.Api.Commands
{
	using Common;

	public class AuthRoleAttribute : DIFilterAttribute
	{
		public string Roles { get; }
		public char RoleSplitChar { get; } = ',';

		public AuthRoleAttribute(string role)
		{
			Roles = role;
		}

		public AuthRoleAttribute(string role, char roleSplitChar) : this(role)
		{
			RoleSplitChar = roleSplitChar;
		}

		public override Task<bool> Validate(IWolfClient client, CommandMessage message)
		{
			var (msg, user) = message;

			var groupId = msg.IsGroup ? msg.GroupId : null;

			var authService = Provider.GetService<IAuthService>();
			if (authService == null)
				return Task.FromResult(false);

			var splitRoles = Roles.Split(RoleSplitChar);

			foreach(var role in splitRoles)
			{
				if (!authService.UserHasRole(role, user.Id, groupId))
					return Task.FromResult(false);
			}

			return Task.FromResult(true);
		}
	}
}
