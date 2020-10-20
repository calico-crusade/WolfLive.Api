using System;
using System.Linq;
using System.Threading.Tasks;

namespace WolfLive.Api
{
	using Models;

	public static class WolfGeneralSenderExtensions
	{
		private const string CMD_LOGIN = "security login";
		private const string CMD_WELCOME = "welcome";

		private const string CMD_GRP_JOIN = "group member add";
		private const string CMD_GRP_LEAVE = "group member delete";
		private const string CMD_GRP_ADMIN = "group admin";

		#region login extensions
		public static async Task<bool> Login(this IWolfClient client, string email, string password)
		{
			try
			{
				var loggedIn = await client.Login();
				if (loggedIn)
					return true;

				var user = await client.Emit<User>(new Packet(CMD_LOGIN, new
				{
					username = email,
					password
				}));

				if (user != null)
				{
					await OnLoginSuccess(client, user);
					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				client.ErrorOccurred(ex);
				return false;
			}
		}

		public static async Task<bool> Login(this IWolfClient client)
		{
			try
			{
				var wt = client.On<WelcomePacket>(CMD_WELCOME);

				await client.Connect();

				var welcome = await wt;

				if (welcome.LoggedInUser != null)
				{
					await OnLoginSuccess(client, welcome.LoggedInUser);
					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				client.ErrorOccurred(ex);
				return false;
			}
		}

		private static async Task OnLoginSuccess(IWolfClient client, User user, bool first = true)
		{

			client.Profiling.Profile = user;

			await client.Profiling.Initialize();

			if (first)
			{
				await client.Messaging.Initialize();
				client.OnConnected += async (c) => await OnLoginSuccess(client, user, false);
				return;
			}

			await client.Messaging.GroupMessageSubscribe();
			await client.Messaging.PrivateMessageSubscribe();
		}
		#endregion

		#region Subprofiling extensions
		public static User CurrentUser(this IWolfClient client)
		{
			return client?.Profiling?.Profile;
		}

		public static Group[] Groups(this IWolfClient client)
		{
			return client?.Profiling?.Groups?.Values?.ToArray();
		}

		public static Task<Group> GetGroup(this IWolfClient client, string groupid)
		{
			return client?.Profiling?.GetGroup(groupid);
		}

		public static Task<GroupUser> GetGroupUser(this IWolfClient client, string groupid, string userid)
		{
			return client?.Profiling?.GetGroupUser(groupid, userid);
		}

		public static Task<GroupUser[]> GetGroupUsers(this IWolfClient client, string groupid, params string[] userids)
		{
			return client?.Profiling?.GetGroupUsers(groupid, userids);
		}

		public static Task<User> GetUser(this IWolfClient client, string userid)
		{
			return client?.Profiling?.GetUser(userid);
		}

		public static Task<User[]> GetUsers(this IWolfClient client, params string[] userids)
		{
			return client?.Profiling?.GetUsers(userids);
		}
		#endregion

		#region Group Join/Leave, Admin Actions

		public static async Task<bool> JoinGroup(this IWolfClient client, string groupId, string password = "")
		{
			var results = await client.Emit(new Packet(CMD_GRP_JOIN, new
			{
				groupId = int.Parse(groupId),
				password
			}));

			if (!results)
				return false;

			await client.Messaging.GroupMessageSubscribe(groupId);
			return true;
		}

		public static Task<bool> LeaveGroup(this IWolfClient client, string groupId)
		{
			return client.Emit(new Packet(CMD_GRP_LEAVE, new
			{
				groupId = int.Parse(groupId)
			}));
		}

		public static Task<bool> AdminAction(this IWolfClient client, string groupId, string userId, GroupUserType action)
		{
			return client.Emit(new Packet(CMD_GRP_ADMIN, new
			{
				groupId = int.Parse(groupId),
				id = int.Parse(userId),
				capabilities = (int)action
			}));
		}

		public static Task<bool> AdminAction(this IWolfClient client, GroupUser user, GroupUserType action)
		{
			return AdminAction(client, user.Group.Id, user.User.Id, action);
		}

		public static Task<bool> AdminAction(this GroupUser user, IWolfClient client, GroupUserType action)
		{
			return AdminAction(client, user.Group.Id, user.User.Id, action);
		}

		public static Task<bool> AdminAction(this IWolfClient client, Message message, GroupUserType action)
		{
			return AdminAction(client, message.GroupId, message.UserId, action);
		}

		public static Task<bool> AdminAction(this Message message, IWolfClient client, GroupUserType action)
		{
			return AdminAction(client, message.GroupId, message.UserId, action);
		}

		#endregion
	}
}
