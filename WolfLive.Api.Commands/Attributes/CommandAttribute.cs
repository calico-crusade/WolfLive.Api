using System.Threading.Tasks;

namespace WolfLive.Api.Commands
{
	public class CommandAttribute : FilterAttribute
	{
		public string Comparitor { get; private set; }

		public CommandAttribute(string comparitor)
		{
			Comparitor = comparitor.ToLower().Trim();
		}

		public bool Validate(string prefix, ref string content)
		{
			if (!content.ToLower().StartsWith(prefix))
				return false;

			content = content.Remove(0, prefix.Length).Trim();

			if (!content.ToLower().StartsWith(Comparitor))
				return false;

			return true;
		}

		public override Task<bool> Validate(IWolfClient client, CommandMessage message)
		{
			var content = message.Message.Content;
			var validation = Validate(message.Prefix, ref content);
			message.Remainder = content;
			message.Command = Comparitor;
			return Task.FromResult(validation);
		}
	}
}
