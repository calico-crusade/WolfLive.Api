using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WolfLive.Api
{
	using Delegates;

	public interface IWolfClient
	{
		SocketIO Connection { get; }
		IWolfMessaging Messaging { get; }
		IWolfProfiling Profiling { get; }
		IWolfPacketing Packeting { get; }
		string DeviceToken { get; }

		event ConnectionCarrier OnConnected;
		event ConnectionErrorCarrier OnDisconnected;
		event ConnectionErrorCarrier OnConnectionError;
		event ReconnectionCarrier OnReconnecting;
		event ErrorCarrier OnError;

		Task Connect();

		void ErrorOccurred(Exception ex);

		Task<T> Emit<T>(Packet packet);
		Task<bool> Emit(Packet packet);

		void On<T>(string eventName, Action<T> onReceived);
		Task<T> On<T>(string eventName);
	}

	public class WolfClient : IWolfClient
	{
		public const string SERVER_URL = "https://v3.palringo.com:3051";
		private const string TOKEN_BASE = "WExxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";

		public SocketIO Connection { get; }
		public IWolfMessaging Messaging { get; }
		public IWolfProfiling Profiling { get; }
		public IWolfPacketing Packeting { get; }
		public string DeviceToken { get; }

		public event ConnectionCarrier OnConnected = delegate { };
		public event ConnectionErrorCarrier OnDisconnected = delegate { };
		public event ConnectionErrorCarrier OnConnectionError = delegate { };
		public event ReconnectionCarrier OnReconnecting = delegate { };
		public event ErrorCarrier OnError = delegate { };

		public WolfClient(string server = SERVER_URL, string token = null, bool reconnect = true)
		{
			Connection = CreateSocketClient(server, new Dictionary<string, string>
			{
				{ "token", DeviceToken = token ?? TOKEN_BASE.Token(startIndex: 2) },
				{ "device", "web" }
			}, reconnect);
			Messaging = new WolfMessaging(this);
			Profiling = new WolfProfiling(this);
			Packeting = new WolfPacketing(this);
		}

		public void On<T>(string eventName, Action<T> action)
		{
			Packeting.On(eventName, action);
		}

		public Task<T> On<T>(string eventName)
		{
			return Packeting.On<T>(eventName);
		}

		public Task<T> Emit<T>(Packet packet)
		{
			return Packeting.Emit<T>(packet);
		}

		public Task<bool> Emit(Packet packet)
		{
			return Packeting.Emit(packet);
		}

		public Task Connect()
		{
			return Connection.ConnectAsync();
		}

		public void ErrorOccurred(Exception ex)
		{
			OnError(this, ex);
		}

		private SocketIO CreateSocketClient(string url, Dictionary<string, string> parameters, bool reconnect)
		{
			var client = new SocketIO(url, new SocketIOOptions
			{
				Query = parameters,
				Reconnection = reconnect
			});

			client.OnConnected += Client_OnConnected;
			client.OnDisconnected += Client_OnDisconnected;
			client.OnError += Client_OnError;
			client.OnReconnecting += Client_OnReconnecting;

			return client;
		}

		private void Client_OnReconnecting(object sender, int e)
		{
			OnReconnecting(this, e);
		}

		private void Client_OnError(object sender, string e)
		{
			OnConnectionError(this, e);
		}

		private void Client_OnDisconnected(object sender, string e)
		{
			OnDisconnected(this, e);
		}

		private void Client_OnConnected(object sender, EventArgs e)
		{
			OnConnected(this);
		}
	}
}
