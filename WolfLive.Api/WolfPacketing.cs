using SocketIOClient;
using SocketIOClient.EventArguments;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WolfLive.Api
{
	using Delegates;
	using Exceptions;

	public interface IWolfPacketing
	{
		event SocketEventCarrier OnDataReceived;

		Task<T> Emit<T>(Packet packet);

		Task<bool> Emit(Packet packet);

		void On<T>(string eventName, Action<T> action, Action<int, string> onFail = null, bool removeAfterRun = false);

		Task<T> On<T>(string eventName);
	}

	public class WolfPacketing : IWolfPacketing
	{

		public event SocketEventCarrier OnDataReceived = delegate { };
		public Dictionary<string, List<WolfTaskResult>> Events { get; } = new Dictionary<string, List<WolfTaskResult>>();

		private readonly IWolfClient _client;

		public WolfPacketing(IWolfClient client)
		{
			_client = client;
			_client.Connection.OnReceivedEvent += Connection_OnReceivedEvent;
		}

		public void On<T>(string eventName, Action<T> action, Action<int, string> onFail = null, bool removeAfterRun = false)
		{
			AddEvent(eventName, new WolfTaskResult
			{
				RemoveAfterRun = removeAfterRun,
				OnReceive = (resp) =>
				{
					OnDataReceived(_client, eventName, resp);

					if (!HandlePacket(resp, out T data, out int code))
					{
						onFail?.Invoke(code, resp.GetValue().ToString());
						return;
					}

					action(data);
				}
			});
		}

		public Task<T> On<T>(string eventName)
		{
			var tsc = new TaskCompletionSource<T>();

			On<T>(eventName,
				  (data) => tsc.SetResult(data),
				  (code, raw) => tsc.SetException(new WolfSocketPacketException(code, raw)),
				  true);

			return tsc.Task;
		}

		public Task<T> Emit<T>(Packet packet)
		{
			var tsc = new TaskCompletionSource<T>();

			_client.Connection.EmitAsync(packet.Command, (resp) =>
			{
				OnDataReceived(_client, packet.Command, resp);
				if (!HandlePacket(resp, out T data, out int code))
				{
					tsc.SetException(new WolfSocketPacketException(code, resp.GetValue().ToString()));
					return;
				}

				tsc.SetResult(data);
			}, packet.ToData());

			return tsc.Task;
		}

		public Task<bool> Emit(Packet packet)
		{
			var tsc = new TaskCompletionSource<bool>();

			_client.Connection.EmitAsync(packet.Command, (res) =>
			{
				OnDataReceived(_client, packet.Command, res);
				var code = HandlePacket(res);
				if (code >= 200 && code < 300)
					tsc.SetResult(true);
				else
					tsc.SetResult(false);
			}, packet.ToData());

			return tsc.Task;
		}

		public void Connection_OnReceivedEvent(object sender, ReceivedEventArgs e)
		{
			var eventName = FormatEventName(e.Event);
			if (!Events.ContainsKey(eventName))
			{
				OnDataReceived(_client, e.Event, e.Response);
				return;
			}

			var events = Events[eventName].ToArray();

			foreach (var evt in events)
			{
				evt.OnReceive(e.Response);

				if (evt.RemoveAfterRun)
					Events[eventName].Remove(evt);
			}
		}

		public string FormatEventName(string eventName)
		{
			return eventName.ToLower().Trim();
		}

		public void AddEvent(string eventName, WolfTaskResult result)
		{
			var name = FormatEventName(eventName);
			if (!Events.ContainsKey(name))
				Events.Add(name, new List<WolfTaskResult>());

			Events[name].Add(result);
		}
		
		public int HandlePacket(SocketIOResponse response)
		{
			try
			{
				var jvalue = response.GetValue();
				var jcode = jvalue["code"];
				if (jcode == null)
					return -1;

				var value = response.GetValue<PacketResponse>();
				return value.Code;
			}
			catch (Exception ex)
			{
				_client.ErrorOccurred(ex);
				return 500;
			}
		}

		public bool HandlePacket<T>(SocketIOResponse response, out T data, out int code)
		{
			data = default;
			try
			{
				code = HandlePacket(response);
				if (code == -1)
				{
					code = 200;
					data = response.GetValue<T>();
					return true;
				}

				var results = response.GetValue<PacketResponse<T>>();
				if (!results.Successful)
					return false;

				data = results.Body;
				return true;
			}
			catch (Exception ex)
			{
				_client.ErrorOccurred(ex);
				code = 500;
				return false;
			}
		}
	}
}
