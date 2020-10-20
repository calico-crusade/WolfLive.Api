using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WolfLive.Api.Commands
{
	public interface ISetupBuilder
	{
		ISetupBuilder WithServices(Action<IServiceCollection> configure);
		ISetupBuilder WithServices(IServiceCollection baseCollection);

		ISetupBuilder WithSerilog(string logPath = "logs\\log.txt", RollingInterval interval = RollingInterval.Day, LogLevel minimumLevel = LogLevel.Debug);
		ISetupBuilder WithLogging(Action<ILoggingBuilder> logging);

		ISetupBuilder WithCommandSet(Action<ICommandBuilder> builder);

		ISetupBuilder WithSetup<T>();
		ISetupBuilder WithSetups(params Type[] types);

		IWolfClient Done();
	}

	public class SetupBuilder : ISetupBuilder
	{
		private readonly IWolfClient _client;

		public IServiceCollection Services { get; private set; } = null;
		public IConfiguration Configuration { get; private set; } = null;

		public List<ICommandBuilder> CommandSets { get; } = new List<ICommandBuilder>();
		public List<MethodInfo> Setups { get; } = new List<MethodInfo>();
		public List<Action<IServiceCollection>> DependencyInjectBuilders { get; } = new List<Action<IServiceCollection>>();

		public SetupBuilder(IWolfClient client)
		{
			_client = client;
		}

		public ISetupBuilder WithServices(Action<IServiceCollection> configure)
		{
			DependencyInjectBuilders.Add(configure);
			return this;
		}

		public ISetupBuilder WithServices(IServiceCollection baseCollection)
		{
			if (Services != null)
				throw new Exception("Service collection has already been defined.");

			Services = baseCollection;
			return this;
		}

		public ISetupBuilder WithSerilog(string logPath = "logs\\log.txt", RollingInterval interval = RollingInterval.Day, LogLevel minimumLevel = LogLevel.Debug)
		{
			return WithLogging(_ =>
			{
				var path = Path.Combine(logPath.Split('\\', '/'));

				_.SetMinimumLevel(minimumLevel);
				_.AddSerilog(new LoggerConfiguration()
					.WriteTo.File(path, rollingInterval: RollingInterval.Day)
					.WriteTo.Console()
					.MinimumLevel.Is((Serilog.Events.LogEventLevel)(int)minimumLevel)
					.CreateLogger());
			});
		}

		public ISetupBuilder WithLogging(Action<ILoggingBuilder> logging)
		{
			return WithServices(_ =>
			{
				_.AddLogging(logging);
			});
		}

		public ISetupBuilder WithCommandSet(Action<ICommandBuilder> builder)
		{
			var bob = new CommandBuilder();
			builder?.Invoke(bob);
			CommandSets.Add(bob);
			return this;
		}

		public ISetupBuilder WithSetup<T>()
		{
			return WithSetups(typeof(T));
		}

		public ISetupBuilder WithSetups(params Type[] types)
		{
			foreach (var type in types)
			{
				foreach (var method in type.GetMethods())
				{
					var attributes = method.GetCustomAttributes<SetupAttribute>().ToList();

					if (attributes.Count <= 0)
						continue;

					Setups.Add(method);
				}
			}

			return this;
		}

		public IWolfClient Done()
		{
			var services = Services ?? new ServiceCollection();

			foreach (var serviceConfigurator in DependencyInjectBuilders)
				serviceConfigurator?.Invoke(services);

			var provider = services
				.AddTransient<IReflectionService, ReflectionService>()
				.AddSingleton(_client)
				.AddSingleton<ICommandService, CommandService>()
				.BuildServiceProvider();

			services.AddSingleton<IServiceProvider>(provider);

			var commandService = provider.GetRequiredService<ICommandService>();

			foreach (var commandSet in CommandSets)
				commandService.AddCommandBuilders(commandSet);

			foreach (var setup in Setups)
				commandService.AddSetups(setup);

			_client.Messaging.OnMessage += async (c, m) => await commandService.ProcessMessage(c, m);

			commandService.TriggerSetup(_client);

			return _client;
		}
	}
}
