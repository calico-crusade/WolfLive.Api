using System;
using System.Threading.Tasks;

namespace WolfLive.Api.Commands
{
	using Models;

	public class RoleAttribute : FilterAttribute
	{
		public string Role { get; }

		public RoleAttribute(string role)
		{
			Role = role;
		}

		public bool Validate(GroupUserType user)
		{
			switch(Role.ToLower().Trim())
			{
				case "admin":
					return user == GroupUserType.Admin || user == GroupUserType.Owner;
				case "mod":
					return user == GroupUserType.Mod || user == GroupUserType.Admin || user == GroupUserType.Owner;
				case "owner":
					return user == GroupUserType.Owner;

				default:
					throw new Exception($"{Role} is not a valid role!");
			}
		}

		public override Task<bool> Validate(IWolfClient client, CommandMessage message)
		{
			var (msg, _, _, role) = message;
			if (!msg.IsGroup)
				return Task.FromResult(false);

			return Task.FromResult(Validate(role));
		}
	}
}
