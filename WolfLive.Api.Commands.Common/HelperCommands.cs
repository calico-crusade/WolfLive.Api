using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WolfLive.Api.Commands.Common
{
	public class HelperCommands : WolfContext
	{
		private readonly ILogger _logger;
		private readonly ICommandService _commands;
		private readonly IAuthService _auth;

		public HelperCommands(
			ILogger<HelperCommands> logger,
			ICommandService commands,
			IAuthService auth)
		{
			_logger = logger;
			_commands = commands;
			_auth = auth;
		}

		[Command("help"), Description("Shows a help message!")]
		public async Task HelpMenu(string remainder)
		{
			if (!string.IsNullOrEmpty(remainder))
			{
				//Resolve command specific help menus

			}

			var bob = new StringBuilder();
			bob.AppendLine("-= Help Menu =-");

			foreach(var set in _commands.CommandBuilders)
			{
				if (!await _commands.CheckFilters(set.ScopedFilters, Client, Command))
					continue;

				bob.AppendLine($"{set.Prefix} - {set.Description}");

				foreach(var command in set.Commands)
				{
					if (command.Item1.GetCustomAttributes<HiddenAttribute>().Count() > 0)
						continue;

					var authFilters = command.Item2.Where(t => !(t is CommandAttribute)).ToList();
					if (!await _commands.CheckFilters(authFilters, Client, Command))
						continue;

					var cmd = command.Item1.GetCustomAttributes<CommandAttribute>().FirstOrDefault()?.Comparitor;
					var des = command.Item1.GetCustomAttributes<DescriptionAttribute>().FirstOrDefault()?.Text;

					bob.AppendLine($"{set.Prefix} {cmd} - {des}");
				}
			}

			await this.Reply(bob.ToString());
		}
	}
}
