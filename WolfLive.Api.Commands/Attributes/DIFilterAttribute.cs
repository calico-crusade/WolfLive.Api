using System;

namespace WolfLive.Api.Commands
{
	public abstract class DIFilterAttribute : FilterAttribute
	{
		public IServiceProvider Provider { get; set; }
	}
}
