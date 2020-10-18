using SocketIOClient;

namespace WolfLive.Api.Delegates
{
	public delegate void SocketEventCarrier(IWolfClient client, string eventName, SocketIOResponse eventData);
}
