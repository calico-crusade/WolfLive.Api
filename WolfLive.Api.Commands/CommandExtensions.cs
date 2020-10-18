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
		public static IServiceCollection AddCommands<T>(this IServiceCollection services)
		{
			return services.AddCommands(typeof(T));
		}

		public static IServiceCollection AddCommands(this IServiceCollection services, params Type[] types)
		{
			foreach(var type in types)
			{
				foreach(var method in type.GetMethods())
				{
					var attributes = method.GetCustomAttributes()
										   .Where(t => t is IMessageFilter)
										   .Cast<IMessageFilter>()
										   .ToList();

					if (attributes.Count <= 0)
						continue;

					CommandService.Commands.Add((method, attributes));
				}
			}

			return services;
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

		public static IServiceCollection FindAllCommands(this IServiceCollection services)
		{
			var reflection = new ReflectionService();

			var types = reflection.GetTypes(t => true).ToArray();

			return services.AddCommands(types);
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

		public static IServiceCollection WithPrefix(this IServiceCollection services, string prefix)
		{
			CommandService.Prefix = prefix;
			return services;
		}
	}
}
