using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace WolfLive.Api.Cli
{
	using Commands;
	using Exceptions;
	using Models;

	public class TestCommands : WolfContext
	{
		private readonly ILogger _logger;

		public TestCommands(ILogger<TestCommands> logger)
		{
			_logger = logger;
		}

		[Command("test1")]
		public async Task Test1(string remainder)
		{
			await this.Reply("Hello world: " + remainder);
		}

		[Command("test2"), PrivateOnly]
		public async Task Test2()
		{
			await this.Reply("Hello world");
		}

		[Command("test3"), GroupOnly]
		public async Task Test3()
		{
			await this.Reply("Hello world");
		}

		[Command("test4"), Role("Owner")]
		public async Task Test4()
		{
			await this.Reply("Hello world");
		}

		[Command("delete this")]
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
	}

	public class SomeStaticClass
	{
		[Command("test5")]
		public static async Task Test5(IWolfClient client, Message msg, string remainder)
		{
			await client.Reply(msg, "Hello world: " + remainder);
		}

		[Command("test6")]
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
