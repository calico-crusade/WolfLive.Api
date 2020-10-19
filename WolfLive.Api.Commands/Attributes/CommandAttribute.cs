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

		public bool Validate(ref string content)
		{
			if (!content.ToLower().StartsWith(Comparitor))
				return false;

			content = content.Remove(0, Comparitor.Length).Trim();
			return true;
		}

		public override Task<bool> Validate(IWolfClient client, CommandMessage message)
		{
			var content = message.Remainder;
			var validation = Validate(ref content);
			message.Remainder = content;
			message.Command = Comparitor;
			return Task.FromResult(validation);
		}
	}
}
