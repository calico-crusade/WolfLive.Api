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
		List<ICommandBuilder> CommandBuilders { get; }

		Task ProcessMessage(IWolfClient client, Message message);
		void TriggerSetup(IWolfClient client);
		void AddCommandBuilders(params ICommandBuilder[] builders);
		void AddSetups(params MethodInfo[] setups);
		Task<bool> CheckFilters(List<IMessageFilter> filters, IWolfClient client, CommandMessage cmd);
	}

	public class CommandService : ICommandService
	{
		private readonly IServiceProvider _provider;
		private readonly IReflectionService _reflection;
		private readonly ILogger _logger;

		public List<ICommandBuilder> CommandBuilders { get; } = new List<ICommandBuilder>();
		public List<MethodInfo> Setups { get; } = new List<MethodInfo>();

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

		public async Task<bool> CheckCommands(ICommandBuilder commands, IWolfClient client, CommandMessage cmd)
		{
			if (!await Validate(commands, client, cmd))
				return false;

			var context = new StaticContext
			{
				Client = client,
				Command = cmd
			};

			string msgWithoutPrefix = cmd.Remainder;

			foreach (var (method, filters) in commands.Commands)
			{
				cmd.Remainder = msgWithoutPrefix;
				if (!await CheckFilters(filters, client, cmd))
					continue;

				await _reflection.ExecuteMethod(method, _provider,
					cmd, cmd.Message, cmd.Remainder,
					cmd.GroupUser, cmd.GroupUser?.Group,
					cmd.User, context, context.Capabilities,
					client, this);
				return true;
			}

			return false;
		}

		public async Task<bool> Validate(ICommandBuilder commands, IWolfClient client, CommandMessage message)
		{
			string content = message.Message.Content;
			if (!ValidatePrefix(commands, ref content))
				return false;

			if (!await CheckFilters(commands.ScopedFilters, client, message))
				return false;

			message.Prefix = commands.Prefix;
			message.Remainder = content;
			return true;
		}

		public bool ValidatePrefix(ICommandBuilder commands, ref string content)
		{
			if (string.IsNullOrEmpty(content))
				return false;

			if (string.IsNullOrEmpty(commands.Prefix))
				return true;

			if (!content.ToLower().StartsWith(commands.Prefix))
				return false;

			content = content.Remove(0, commands.Prefix.Length).Trim();
			return true;
		}

		public async Task ProcessMessage(IWolfClient client, Message message)
		{
			try
			{
				if (message.UserId == client.CurrentUser().Id)
					return;

				var cmd = new CommandMessage
				{
					Message = message
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

				foreach (var commands in CommandBuilders)
				{
					if (await CheckCommands(commands, client, cmd))
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
				if (filter is DIFilterAttribute diFilter)
					diFilter.Provider = _provider;

				if (!await filter.Validate(client, cmd))
					return false;
			}

			return true;
		}

		public void AddCommandBuilders(params ICommandBuilder[] builders)
		{
			CommandBuilders.AddRange(builders);
		}

		public void AddSetups(params MethodInfo[] setups)
		{
			Setups.AddRange(setups);
		}
	}
}
