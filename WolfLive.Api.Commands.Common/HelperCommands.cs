using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WolfLive.Api.Commands.Common
{
	public class HelperCommands : WolfContext
	{
		private readonly ILogger _logger;

		public HelperCommands(ILogger<HelperCommands> logger)
		{
			_logger = logger;
		}

		[Command("help")]
		public async Task HelpMenu()
		{
			await this.Reply($"Hello {User.Nickname} ({User.Id}) - {Group.Name}");

		}
	}

	public class HelloWorld
	{
		[Command("hello")]
		public static async Task TestCommand(StaticContext context)
		{
			await context.Reply("Hello world");
		}
	}
}
