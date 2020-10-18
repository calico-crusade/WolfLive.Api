namespace WolfLive.Api.Delegates
{
	using Models;

	public delegate void MessageCarrier(IWolfClient client, Message message);
	public delegate void MessageCarrier<T>(IWolfClient client, Message message, T sender);
}
