using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WolfLive.Api
{
	using Models;
	using System;

	public interface IWolfProfiling
	{
		User Profile { get; set; }
		Dictionary<string, Group> Groups { get; }

		Task Initialize();

		Task<Group> GetGroup(string groupid);
		Task<GroupUser> GetGroupUser(string groupid, string userid);
		Task<GroupUser[]> GetGroupUsers(string groupid, params string[] userids);
		Task<User> GetUser(string userid);
		Task<User[]> GetUsers(params string[] userids);
	}

	public class WolfProfiling : IWolfProfiling
	{
		private const string CMD_PRF_GROUP = "group profile";
		private const string CMD_PRF_USER = "subscriber profile";
		private const string CMD_GRP_UPDATE = "group member update";
		private const string CMD_GRP_DELETE = "group member delete";
		private const string CMD_GRP_ADD = "group member add";
		private const string CMD_GRP_LIST = "group member list";
		private const int PROFILE_REQUEST_LIMIT = 25;
		private const int PROFILE_BREAK_COUNT = 4;
		private const int PROFILE_BREAK_TIME = 1000;

		private readonly IWolfClient _client;

		public Dictionary<string, Group> Groups { get; } = new Dictionary<string, Group>();
		public Dictionary<string, User> Users { get; } = new Dictionary<string, User>();
		public Dictionary<string, Dictionary<string, GroupUserType>> GroupUsers { get; } = new Dictionary<string, Dictionary<string, GroupUserType>>();
		public User Profile { get; set; }

		public WolfProfiling(IWolfClient client)
		{
			_client = client;
		}

		public async Task Initialize()
		{
			await GetGroups();
			_client.On<GroupUserUpdate>(CMD_GRP_ADD, (g) => GroupUserUpdate(g));
			_client.On<GroupUserUpdate>(CMD_GRP_UPDATE, (g) => GroupUserUpdate(g));
			_client.On<GroupUserUpdate>(CMD_GRP_DELETE, (g) => GroupUserUpdate(g, true));
		}

		public async Task<Group> GetGroup(string groupid)
		{
			if (Groups.ContainsKey(groupid))
				return Groups[groupid];

			return await ClientGetGroup(groupid);
		}

		public async Task<User> GetUser(string userid)
		{
			if (Users.ContainsKey(userid))
				return Users[userid];

			return await ClientGetUser(userid);
		}

		public async Task<User[]> GetUsers(params string[] userids)
		{
			if (userids == null || userids.Length <= 0)
				return null;

			if (userids.Length == 1)
				return new[] { await GetUser(userids[0]) };

			var users = new List<User>();
			var ids = new List<int>();

			foreach(var user in userids)
			{
				if (Users.ContainsKey(user))
				{
					users.Add(Users[user]);
					continue;
				}

				ids.Add(int.Parse(user));
			}

			var fetch = await ClientGetUsers(ids.ToArray());
			return fetch.Concat(users).ToArray();
		}

		public async Task<GroupUser> GetGroupUser(string groupid, string userid)
		{
			return (await GetGroupUsers(groupid, userid)).FirstOrDefault();
		}

		public async Task<GroupUser[]> GetGroupUsers(string groupid, params string[] userids)
		{
			var group = await GetGroup(groupid);
			var members = await ClientGroupMemberList(groupid);

			userids = userids == null || userids.Length <= 0 ? 
				members.Select(t => t.Id.ToString()).ToArray() : 
				userids;

			var users = await GetUsers(userids);

			return users.Select(t => new GroupUser
			{
				User = t,
				Group = group,
				Capabilities = members.FirstOrDefault(a => a.Id.ToString() == t.Id)?.Capabilities ?? GroupUserType.User
			}).ToArray();
		}

		#region Packet Senders
		private async Task<Group> ClientGetGroup(string groupid)
		{
			var group = await _client.Emit<Group>(new Packet(CMD_PRF_GROUP, new
			{
				id = int.Parse(groupid),
				extended = true
			}));

			GroupUpdate(group);
			return group;
		}

		private async Task<User> ClientGetUser(string userid)
		{
			var user = await _client.Emit<User>(new Packet(CMD_PRF_USER, new
			{
				id = int.Parse(userid),
				extended = true
			}));
			UserUpdate(user);
			return user;
		}

		private async Task<User[]> ClientGetUsers(int[] ids)
		{
			if (ids.Length <= PROFILE_REQUEST_LIMIT)
				return await ClientGetUsersPacket(ids);

			var idChunks = ids.Chunk(PROFILE_REQUEST_LIMIT).ToArray();
			var users = new List<User>();

			for(var i = 0; i < idChunks.Length; i++)
			{
				if (i % PROFILE_BREAK_COUNT == 0)
					await Task.Delay(PROFILE_BREAK_TIME);

				var chunk = idChunks[i];

				var userChunk = await ClientGetUsersPacket(chunk);
				users.AddRange(userChunk);
			}

			return users.ToArray();
		}

		private async Task<User[]> ClientGetUsersPacket(int[] ids)
		{
			var fetchedUsers = await _client.Emit<List<PacketResponse<User>>>(new Packet(CMD_PRF_USER, new
			{
				extended = false,
				subscribe = true,
				idList = ids
			}));

			var users = new List<User>();

			foreach (var user in fetchedUsers)
			{
				if (!user.Successful)
					continue;

				UserUpdate(user.Body);
				users.Add(user.Body);
			}

			return users.ToArray();
		}

		private async Task<GroupUserUpdate[]> ClientGroupMemberList(string groupid, bool force = false)
		{
			if (GroupUsers.ContainsKey(groupid) && !force)
				return GroupUsers[groupid].Select(t => new GroupUserUpdate
				{
					Id = int.Parse(t.Key),
					GroupId = int.Parse(groupid),
					Capabilities = t.Value
				}).ToArray();

			var group = await _client.Emit<GroupUserUpdate[]>(new Packet(CMD_GRP_LIST, new
			{
				id = int.Parse(groupid),
				subscribe = true
			}, 3));

			if (!GroupUsers.ContainsKey(groupid))
				GroupUsers.Add(groupid, new Dictionary<string, GroupUserType>());

			foreach(var member in group)
			{
				member.GroupId = int.Parse(groupid);
				GroupUserUpdate(member);
			}

			return group;
		}
		#endregion

		private async Task GetGroups()
		{
			var groups = await _client.Emit<Group[]>(new Packet("group list"));
			foreach (var group in groups)
			{
				GroupUpdate(group);
			}
		}

		private void GroupUpdate(Group group)
		{
			if (Groups.ContainsKey(group.Id))
				Groups[group.Id] = group;
			else
				Groups.Add(group.Id, group);
		}

		private void UserUpdate(User user)
		{
			if (Users.ContainsKey(user.Id))
				Users[user.Id] = user;
			else
				Users.Add(user.Id, user);
		}

		private void GroupUserUpdate(GroupUserUpdate user, bool kicked = false)
		{
			var userid = user.SubscriberId.ToString();
			var groupid = user.GroupId.ToString();

			if (!GroupUsers.ContainsKey(groupid))
				GroupUsers.Add(groupid, new Dictionary<string, GroupUserType>());

			if (kicked)
			{
				if (GroupUsers[groupid].ContainsKey(userid))
				{
					GroupUsers[groupid].Remove(userid);
					return;
				}

				return;
			}

			if (!GroupUsers.ContainsKey(groupid))
			{
				GroupUsers[groupid].Add(userid, user.Capabilities ?? GroupUserType.User);
				return;
			}

			GroupUsers[groupid][userid] = user.Capabilities ?? GroupUserType.User;
		}
	}
}
