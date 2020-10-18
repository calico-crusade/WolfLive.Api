using System.Threading.Tasks;

namespace WolfLive.Api.Commands
{
	public interface IMessageFilter
	{
		Task<bool> Validate(IWolfClient client, CommandMessage message);
	}
}
