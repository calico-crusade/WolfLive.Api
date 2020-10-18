using System.Threading.Tasks;

namespace WolfLive.Api.Cli
{
	using Commands;
	using WolfLive.Api.Models;

	public class TestCommands : WolfContext
	{
		[Command("test1")]
		public async Task Test1()
		{
			await this.Reply("Hello world");
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
}
