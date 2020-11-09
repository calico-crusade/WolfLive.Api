using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WolfLive.Api.Cli
{
	using Commands;
	using Commands.Common;
	using Exceptions;
	using Models;

	public class TestCommands : WolfContext
	{
		private readonly ILogger _logger;
		private readonly IAuthService _auth;

		public TestCommands(
			ILogger<TestCommands> logger,
			IAuthService auth)
		{
			_logger = logger;
			_auth = auth;
		}

		[Command("test1"), Description("Remainder test!")]
		public async Task Test1(string remainder)
		{
			await this.Reply("Hello world: " + remainder);
		}

		[Command("test2"), PrivateOnly, Description("PM Only test!")]
		public async Task Test2()
		{
			await this.Reply("Hello world");
		}

		[Command("test3"), GroupOnly, Description("Group Only test!")]
		public async Task Test3()
		{
			await this.Reply("Hello world");
		}

		[Command("test4"), Role("Owner"), Description("Owner only role test!")]
		public async Task Test4()
		{
			await this.Reply("Hello world");
		}

		[Command("delete this"), Description("Delete message test")]
		public async Task DeleteThisTest()
		{
			try
			{
				var results = await this.DeleteMessage();
				await this.Reply("Deleted message: " + JsonConvert.SerializeObject(results, Formatting.Indented));
			}
			catch (WolfSocketPacketException ex)
			{
				if (ex.Code == 403)
				{
					await this.Reply("You either have higher or the same group permissions (Admin/Mod) that I do! I can't delete your message!");
					return;
				}

				_logger.LogError(ex, "Error occurred while deleting message, code: " + ex.Code);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deleting message");
			}
		}

		[Setup]
		public void DoSetup(IWolfClient client)
		{
			client.Messaging.OnMessage += (c, m) => _logger.LogDebug($"Found message: {m.UserId}: {m.Content}");
		}

		[Command("toggle auth global"), Hidden]
		public async Task ToggleAuth(string toggle)
		{
			var parts = toggle.Split(' ');
			if (parts.Length <= 1)
			{
				await this.Reply("You need to specify at least the user's ID and the role!");
				return;
			}

			var userid = parts[0];
			var role = string.Join(" ", parts.Skip(1)).Trim();

			var toggled = _auth.ToggleUserRole(role, userid);

			await this.Reply("Results: " + toggled);
		}

		[Command("toggle auth group"), Hidden]
		public async Task ToggleAuthGroup(string toggle)
		{
			var parts = toggle.Split(' ');

			if (parts.Length <= 2)
			{
				await this.Reply("You need to specify at least the user's ID, the group's ID and the role!");
				return;
			}

			var userid = parts[0];
			var groupId = parts[1];
			var role = string.Join(" ", parts.Skip(2)).Trim();

			var toggled = _auth.ToggleUserRole(role, userid, groupId);

			await this.Reply("Results: " + toggled);
		}

		[Command("role test1"), AuthRole("test")] public async Task TestRole1() => await this.Reply("Hello!");

		[Command("role test2"), AuthRole("again")] public async Task TestRole2() => await this.Reply("There!");

		[Command("role test3"), AuthRole("test"), AuthRole("again")] public async Task TestRole3() => await this.Reply("General!");

		[Command("role test4"), AuthRole("test, again")] public async Task TestRole4() => await this.Reply("Kenobi!");

		[Command("role test5"), AuthRole("test | again", '|')] public async Task TestRole5() => await this.Reply("/me swoosh");

		[Command("group test")]
		public async Task TestGroupUsers()
		{
			var group = await Client.GetGroup(Group.Id);
			await this.Reply("Group: " + JsonConvert.SerializeObject(group, Formatting.Indented));

			var users = await Client.GetGroupUsers(Group.Id);
			await this.Reply("Group Users:\r\n" + string.Join("\r\n", users.Select(t => $"{t.User.Nickname} ({t.User.Id}) - {t.Capabilities}")));
		}
	}

	public class SomeStaticClass
	{
		[Command("test5"), Description("static client DI resolution test")]
		public static async Task Test5(IWolfClient client, Message msg, string remainder)
		{
			await client.Reply(msg, "Hello world: " + remainder);
		}

		[Command("test6"), Description("static context test")]
		public static async Task Test6(WolfContext context, string remainder)
		{
			await context.Reply("Hello world: " + remainder);
		}
	}

	public class AlecOnly : FilterAttribute
	{
		public override Task<bool> Validate(IWolfClient client, CommandMessage message)
		{
			var (_, user) = message;
			return Task.FromResult(user.Id == "43681734");
		}
	}
}
