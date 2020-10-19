using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WolfLive.Api.Commands
{
	public static class CommandExtensions
	{
		public static ICommandBuilder AddCommands<T>(this IServiceCollection services)
		{
			return services.AddCommands(typeof(T));
		}

		public static ICommandBuilder AddCommands(this IServiceCollection services, params Type[] types)
		{
			var builder = new CommandBuilder();
			builder.AddCommands(types);

			CommandService.CommandBuilders.Add(builder);
			return builder;
		}

		public static IServiceCollection AddSetups<T>(this IServiceCollection services)
		{
			return services.AddSetups(typeof(T));
		}

		public static IServiceCollection AddSetups(this IServiceCollection services, params Type[] types)
		{
			foreach (var type in types)
			{
				foreach (var method in type.GetMethods())
				{
					var attributes = method.GetCustomAttributes<SetupAttribute>().ToList();

					if (attributes.Count <= 0)
						continue;

					CommandService.Setups.Add(method);
				}
			}

			return services;
		}

		public static IWolfClient AddCommands(this IWolfClient client, Action<IServiceCollection> config = null, Action<ILoggingBuilder> logging = null)
		{
			var collection = new ServiceCollection();

			config?.Invoke(collection);

			return client.AddCommands(collection);
		}

		public static IWolfClient AddCommands(this IWolfClient client, IServiceCollection services, Action<ILoggingBuilder> logging = null)
		{
			var provider = services
				.AddTransient<IReflectionService, ReflectionService>()
				.AddTransient<ICommandService, CommandService>()
				.AddSingleton(client)
				.AddLogging(_ =>
				{
					if (logging != null)
					{
						logging(_);
						return;
					}

					_.SetMinimumLevel(LogLevel.Debug);
					_.AddSerilog(new LoggerConfiguration()
						.WriteTo.File(Path.Combine("logs", "log.txt"), rollingInterval: RollingInterval.Day)
						.WriteTo.Console()
						.MinimumLevel.Debug()
						.CreateLogger());
				})
				.BuildServiceProvider();

			services.AddSingleton<IServiceProvider>(provider);

			var commands = provider.GetRequiredService<ICommandService>();

			client.Messaging.OnMessage += async (c, m) =>
			{
				await commands.ProcessMessage(c, m);
			};

			commands.TriggerSetup(client);

			return client;
		}

		public static ICommandBuilder WithPrefix(this IServiceCollection services, string prefix)
		{
			var builder = new CommandBuilder();
			builder.WithPrefix(prefix);

			CommandService.CommandBuilders.Add(builder);
			return builder;
		}
	}
}
