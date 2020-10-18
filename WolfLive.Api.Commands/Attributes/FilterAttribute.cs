using System;
using System.Threading.Tasks;

namespace WolfLive.Api.Commands
{
	[AttributeUsage(AttributeTargets.Method)]
	public abstract class FilterAttribute : Attribute, IMessageFilter
	{
		public abstract Task<bool> Validate(IWolfClient client, CommandMessage message);
	}
}
