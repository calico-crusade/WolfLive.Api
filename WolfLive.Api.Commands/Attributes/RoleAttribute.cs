using System;
using System.Threading.Tasks;

namespace WolfLive.Api.Commands
{
	using Models;

	public class RoleAttribute : FilterAttribute
	{
		public string Role { get; } = null;
		public PrivilegeType? Privilege { get; } = null;

		public RoleAttribute(string role) : this(role, null) { }

		public RoleAttribute(GroupUserType role) : this(role.ToString()) { }

		public RoleAttribute(PrivilegeType privilege) : this(null, privilege) { }

		public RoleAttribute(string role, PrivilegeType privilege) : this(role, (PrivilegeType?)privilege) { }

		public RoleAttribute(GroupUserType role, PrivilegeType privilege) : this(role.ToString(), privilege) { }

		public RoleAttribute(PrivilegeType privilege, string role) : this(role, privilege) { }

		public RoleAttribute(PrivilegeType privilege, GroupUserType role) : this(role, privilege) { }

		private RoleAttribute(string role, PrivilegeType? privilege)
		{
			Role = role?.ToLower()?.Trim();
			Privilege = privilege;
		}

		public bool Validate(PrivilegeType privilege)
		{
			return privilege.HasFlag(Privilege);
		}

		public bool Validate(GroupUserType capability)
		{
			switch(Role)
			{
				case "admin":
					return capability == GroupUserType.Admin || capability == GroupUserType.Owner;
				case "mod":
					return capability == GroupUserType.Mod || capability == GroupUserType.Admin || capability == GroupUserType.Owner;
				case "owner":
					return capability == GroupUserType.Owner;
				case "user":
					return capability != GroupUserType.Banned && capability != GroupUserType.Silenced;
				case "silenced":
					return capability == GroupUserType.Silenced;
				case "banned":
					return capability == GroupUserType.Banned;

				default:
					throw new Exception($"{Role} is not a valid role!");
			}
		}

		public override Task<bool> Validate(IWolfClient client, CommandMessage message)
		{
			var (msg, user, _, role) = message;

			if (string.IsNullOrEmpty(Role) && Privilege == null)
				return Task.FromResult(false);

			if (Privilege != null && !Validate(user.Privileges))
				return Task.FromResult(false);

			if (string.IsNullOrEmpty(Role))
				return Task.FromResult(true);

			if (!msg.IsGroup)
				return Task.FromResult(false);

			return Task.FromResult(Validate(role));
		}
	}
}
