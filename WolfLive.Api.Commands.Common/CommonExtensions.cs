using Microsoft.Extensions.DependencyInjection;

namespace WolfLive.Api.Commands.Common
{
	public static class CommonExtensions
	{
		public static ISetupBuilder WithAuthRoleAttributes(this ISetupBuilder builder, string authFile = null)
		{
			return builder.WithServices(c =>
			{
				var service = new AuthService();
				if (!string.IsNullOrEmpty(authFile))
					service.AuthUsersFilePath = authFile;

				c.AddSingleton<IAuthService>(service);
			});
		}

		public static ISetupBuilder WithHelpCommand(this ISetupBuilder builder, string prefix)
		{
			return builder.WithCommandSet(c =>
			{
				c.WithPrefix(prefix)
				 .WithDescription("Help menu")
				 .AddCommands<HelperCommands>();
			});
		}
	}
}
