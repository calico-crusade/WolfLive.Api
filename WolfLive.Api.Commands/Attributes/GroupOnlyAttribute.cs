using System.Threading.Tasks;

namespace WolfLive.Api.Commands
{
	public class GroupOnlyAttribute : FilterAttribute
	{ 
		public override Task<bool> Validate(IWolfClient client, CommandMessage message)
		{
			var (msg, _) = message;

			return Task.FromResult(msg.IsGroup);
		}
	}
}
