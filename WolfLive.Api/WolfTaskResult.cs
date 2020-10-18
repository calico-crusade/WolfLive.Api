using SocketIOClient;
using System;

namespace WolfLive.Api
{
	public class WolfTaskResult
	{
		public Action<SocketIOResponse> OnReceive { get; set; }

		public bool RemoveAfterRun { get; set; }
	}
}
