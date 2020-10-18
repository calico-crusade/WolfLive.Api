using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace WolfLive.Api.Commands
{
	using Models;

	public interface ICommandService
	{
		Task ProcessMessage(IWolfClient client, Message message);
		void TriggerSetup(IWolfClient client);
	}

	public class CommandService : ICommandService
	{
		private readonly IServiceProvider _provider;
		private readonly IReflectionService _reflection;
		private readonly ILogger _logger;

		public readonly static List<(MethodInfo, List<IMessageFilter>)> Commands = new List<(MethodInfo, List<IMessageFilter>)>();
		public readonly static List<MethodInfo> Setups = new List<MethodInfo>();
		public static string Prefix { get; set; }

		public CommandService(IServiceProvider provider, 
			IReflectionService reflection,
			ILogger<CommandService> logger)
		{
			_provider = provider;
			_reflection = reflection;
			_logger = logger;
		}

		public async void TriggerSetup(IWolfClient client)
		{
			foreach(var setup in Setups)
			{
				await _reflection.ExecuteMethod(setup, _provider, client);
			}
		}

		public async Task ProcessMessage(IWolfClient client, Message message)
		{
			try
			{
				var cmd = new CommandMessage
				{
					Message = message,
					Prefix = Prefix
				};

				if (message.IsGroup)
				{
					cmd.GroupUser = await client.GetGroupUser(message.GroupId, message.UserId);
					cmd.User = cmd.GroupUser.User;
				}
				else
				{
					cmd.User = await client.GetUser(message.UserId);
				}

				foreach (var (method, filters) in Commands)
				{
					if (!await CheckFilters(filters, client, cmd))
						continue;

					var context = new StaticContext
					{
						Client = client,
						Command = cmd
					};

					await _reflection.ExecuteMethod(method, _provider,
						cmd, cmd.Message, cmd.Remainder,
						cmd.GroupUser, cmd.GroupUser?.Group,
						cmd.User, context, context.Capabilities,
						client, this);
					return;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error occurred while processing a command: {message.Content}");
			}
		}

		public async Task<bool> CheckFilters(List<IMessageFilter> filters, IWolfClient client, CommandMessage cmd)
		{
			foreach (var filter in filters)
			{
				if (!await filter.Validate(client, cmd))
					return false;
			}

			return true;
		}
	}
}
