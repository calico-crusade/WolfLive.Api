using System;
using System.Threading.Tasks;

namespace WolfLive.Api.Commands
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public abstract class FilterAttribute : Attribute, IMessageFilter
	{
		public abstract Task<bool> Validate(IWolfClient client, CommandMessage message);
	}
}
