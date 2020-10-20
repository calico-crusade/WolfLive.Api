using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace WolfLive.Api.Commands.Common
{
	public interface IAuthService
	{
		bool UserHasRole(string role, string userId, string groupId = null);
		bool RemoveUserRole(string role, string userId, string groupId = null);
		bool AddUserRole(string role, string userId, string groupId = null);
		bool ToggleUserRole(string role, string userId, string groupId = null);
	}

	public class AuthService : IAuthService
	{
		private AuthModel _authUsers = null;

		public string AuthUsersFilePath { get; set; } = "authusers.json";

		public AuthModel AuthedUsers => _authUsers ??= LoadAuthUsers();

		public AuthModel LoadAuthUsers()
		{
			if (!File.Exists(AuthUsersFilePath))
				return new AuthModel();

			var data = File.ReadAllText(AuthUsersFilePath);
			return JsonConvert.DeserializeObject<AuthModel>(data);
		}

		public void SaveAuthUsers()
		{
			var data = JsonConvert.SerializeObject(_authUsers, Formatting.Indented);
			File.WriteAllText(AuthUsersFilePath, data);
		}

		public bool UserHasRole(string role, string userId, string groupId = null)
		{
			role = role.ToLower().Trim();

			if (!string.IsNullOrEmpty(groupId))
			{
				if (AuthedUsers.GroupRoles.ContainsKey(groupId) && 
					AuthedUsers.GroupRoles[groupId].ContainsKey(userId) && 
					AuthedUsers.GroupRoles[groupId][userId].Roles.Contains(role))
					return true;
			}

			if (!AuthedUsers.GloablRoles.ContainsKey(userId))
				return false;

			return AuthedUsers.GloablRoles[userId].Roles.Contains(role);
		}

		public bool RemoveUserRole(string role, string userId, string groupId = null)
		{
			role = role.ToLower().Trim();

			if (string.IsNullOrEmpty(groupId))
			{
				if (!AuthedUsers.GloablRoles.ContainsKey(userId))
					return false;

				if (!AuthedUsers.GloablRoles[userId].Roles.Contains(role))
					return false;

				AuthedUsers.GloablRoles[userId].Roles.Remove(role);
				SaveAuthUsers();
				return true;
			}

			if (!AuthedUsers.GroupRoles.ContainsKey(groupId))
				return false;

			if (!AuthedUsers.GroupRoles[groupId].ContainsKey(userId))
				return false;

			if (!AuthedUsers.GroupRoles[groupId][userId].Roles.Contains(role))
				return false;

			AuthedUsers.GroupRoles[groupId][userId].Roles.Remove(role);
			SaveAuthUsers();
			return true;
		}

		public bool AddUserRole(string role, string userId, string groupId = null)
		{
			role = role.ToLower().Trim();

			if (string.IsNullOrEmpty(groupId))
			{
				if (!AuthedUsers.GloablRoles.ContainsKey(userId))
					AuthedUsers.GloablRoles.Add(userId, new UserRole { UserId = userId });

				if (AuthedUsers.GloablRoles[userId].Roles.Contains(role))
					return false;

				AuthedUsers.GloablRoles[userId].Roles.Add(role);
				SaveAuthUsers();
				return true;
			}

			if (!AuthedUsers.GroupRoles.ContainsKey(groupId))
				AuthedUsers.GroupRoles.Add(groupId, new Dictionary<string, UserRole>());

			if (!AuthedUsers.GroupRoles[groupId].ContainsKey(userId))
				AuthedUsers.GroupRoles[groupId].Add(userId, new UserRole { UserId = userId });

			if (AuthedUsers.GroupRoles[groupId][userId].Roles.Contains(role))
				return false;

			AuthedUsers.GroupRoles[groupId][userId].Roles.Add(role);
			return true;
		}

		public bool ToggleUserRole(string role, string userId, string groupId = null)
		{
			role = role.ToLower().Trim();

			if (UserHasRole(role, userId, groupId))
				return RemoveUserRole(role, userId, groupId);

			return AddUserRole(role, userId, groupId);
		}
	}
}
