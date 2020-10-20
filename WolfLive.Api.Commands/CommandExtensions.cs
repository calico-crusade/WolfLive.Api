namespace WolfLive.Api.Commands
{
	public static class CommandExtensions
	{
		public static ISetupBuilder SetupCommands(this IWolfClient client)
		{
			return new SetupBuilder(client);
		}
	}
}
