using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WolfLive.Api.Commands
{
	public interface ICommandBuilder
	{
		string Prefix { get; }
		string Description { get; }
		List<(MethodInfo, List<IMessageFilter>)> Commands { get; }
		List<IMessageFilter> ScopedFilters { get; }

		ICommandBuilder AddCommands(params Type[] types);
		ICommandBuilder AddCommands<T>();
		ICommandBuilder AddFilters(params IMessageFilter[] filters);
		ICommandBuilder AddFilters<T>() where T: IMessageFilter, new();
		ICommandBuilder WithDescription(string description);
		ICommandBuilder WithPrefix(string prefix);
	}

	public class CommandBuilder : ICommandBuilder
	{
		public string Prefix { get; set; }

		public string Description { get; set; }

		public List<IMessageFilter> ScopedFilters { get; } = new List<IMessageFilter>();

		public List<(MethodInfo, List<IMessageFilter>)> Commands { get; } = new List<(MethodInfo, List<IMessageFilter>)>();

		public ICommandBuilder AddCommands(params Type[] types)
		{
			foreach (var type in types)
			{
				foreach (var method in type.GetMethods())
				{
					var attributes = method.GetCustomAttributes()
										   .Where(t => t is IMessageFilter)
										   .Cast<IMessageFilter>()
										   .ToList();

					if (attributes.Count <= 0)
						continue;

					Commands.Add((method, attributes));
				}
			}

			return this;
		}

		public ICommandBuilder AddCommands<T>()
		{
			AddCommands(typeof(T));
			return this;
		}

		public ICommandBuilder AddFilters(params IMessageFilter[] filters)
		{
			ScopedFilters.AddRange(filters);
			return this;
		}

		public ICommandBuilder AddFilters<T>() where T : IMessageFilter, new()
		{
			return AddFilters(new T());
		}

		public ICommandBuilder WithDescription(string description)
		{
			Description = description;
			return this;
		}

		public ICommandBuilder WithPrefix(string prefix)
		{
			Prefix = prefix.ToLower().Trim();
			return this;
		}
	}
}
