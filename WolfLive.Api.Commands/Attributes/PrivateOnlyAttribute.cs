using System.Threading.Tasks;

namespace WolfLive.Api.Commands
{
	public class PrivateOnlyAttribute : FilterAttribute
	{
		public override Task<bool> Validate(IWolfClient client, CommandMessage message)
		{
			var (msg, _) = message;
			return Task.FromResult(!msg.IsGroup);
		}
	}
}
