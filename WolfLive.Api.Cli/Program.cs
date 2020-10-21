using Newtonsoft.Json;
using SocketIOClient;
using System;
using System.Threading.Tasks;

namespace WolfLive.Api.Cli
{
	using Commands;
	using Commands.Common;
	using Models;

	public class Settings
	{
		public string Prefix { get; set; }
	}

	public class Program
	{
		private static readonly bool DebugEvents = false;

		public static void Main(string[] args)
		{
			Start(args)
				.GetAwaiter()
				.GetResult();

			while (Console.ReadKey().Key != ConsoleKey.E)
				Console.WriteLine("Press \"E\" to exit");
		}

		public static async Task Start(string[] args)
		{
			var client = new WolfClient()
				.SetupCommands()
				.WithConfig("settings_file.json")
				.GetConfig(out Settings settings)
				.WithCommandSet(c =>
				{
					c.WithPrefix("!")
					 .WithDescription("Standard tests")
					 .AddCommands<TestCommands>()
					 .AddCommands<SomeStaticClass>();
				})
				.WithCommandSet(c =>
				{
					c.WithPrefix(">")
					 .WithDescription("Different prefix tests! - Admin only")
					 .AddCommands<TestCommands>()
					 .AddFilters(new RoleAttribute("Admin"));
				})
				.WithCommandSet(c =>
				{
					c.WithPrefix(settings.Prefix)
					 .WithDescription("Scoped filters test - Alec Only")
					 .AddCommands<TestCommands>()
					 .AddFilters<AlecOnly>();
				})
				.WithSetup<TestCommands>()
				.WithAuthRoleAttributes()
				.WithHelpCommand("!")
				.WithSerilog()
				.Done();

			client.Messaging.OnMessage += Messaging_OnMessage;
			client.Messaging.OnGroupMessage += Messaging_OnGroupMessage;
			client.Messaging.OnPrivateMessage += Messaging_OnPrivateMessage;
			client.OnError += Client_OnError;
			client.OnConnected += Client_OnConnected;
			client.OnConnectionError += Client_OnConnectionError;
			client.OnDisconnected += Client_OnDisconnected;
			client.OnReconnecting += Client_OnReconnected;
			client.Packeting.OnDataReceived += Client_OnDataReceived;

			var loggedIn = await client.Login(args[0], args[1]);

			if (!loggedIn)
			{
				Console.WriteLine("Failed to login!");
				return;
			}

			Console.WriteLine("Login success!");
		}

		private static void Messaging_OnPrivateMessage(IWolfClient client, Message message, User sender)
		{
			if (DebugEvents)
				Console.WriteLine($"PM: {sender.Nickname}: {message.Content}");
		}

		private static void Messaging_OnGroupMessage(IWolfClient client, Message message, GroupUser sender)
		{
			if (DebugEvents)
				Console.WriteLine($"GM: {sender.User.Nickname}@{sender.Group.Name}: {message.Content}");

			//if (message.Content.ToLower().Trim().StartsWith("!user"))
			//{
			//	var cmd = message.Content.Trim().Remove(0, 5).Trim();
			//	if (!int.TryParse(cmd, out int userid))
			//	{
			//		await message.Reply(client, "Invalid user id!");
			//		return;
			//	}

			//	var gu = await client.GetGroupUser(message.GroupId, userid.ToString());
			//	await message.Reply(client, $"{gu.User.Nickname} is a(n) {gu.Capabilities}");
			//}
		}

		private static void Client_OnDataReceived(IWolfClient client, string eventName, SocketIOResponse eventData)
		{
			if (DebugEvents)
				Console.WriteLine($"COMMAND: {eventName}\r\nDATA: {eventData.GetValue()}");
		}

		private static void Client_OnReconnected(IWolfClient client, int reconnectionCount)
		{
			Console.WriteLine($"Reconnected: {reconnectionCount}");
		}

		private static void Client_OnDisconnected(IWolfClient client, string error)
		{
			Console.WriteLine($"Disconnected: {error}");
		}

		private static void Client_OnConnectionError(IWolfClient client, string error)
		{
			Console.WriteLine($"Connection Error: {error}");
		}

		private static void Client_OnConnected(IWolfClient client)
		{
			Console.WriteLine($"Connected");
		}

		private static void Messaging_OnMessage(IWolfClient client, Message message)
		{
			if (DebugEvents)
				Console.WriteLine($"Message Received: {JsonConvert.SerializeObject(message, Formatting.Indented)}");
		}

		private static void Client_OnError(IWolfClient client, Exception ex)
		{
			Console.WriteLine($"Error: {ex}");
		}
	}
}
