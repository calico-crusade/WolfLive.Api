namespace WolfLive.Api.Commands
{
	using Models;

	public abstract class WolfContext
	{
		public IWolfClient Client { get; set; }
		public CommandMessage Command { get; set; }
		public Message Message => Command?.Message;
		public User User => Command?.User;
		public Group Group => Command?.GroupUser?.Group;
		public GroupUserType Capabilities => Command?.GroupUser?.Capabilities ?? GroupUserType.User;
		public string Content => Command?.Remainder;
		public string Prefix => Command?.Prefix;

		public void Deconstruct(out string msg, out User user)
		{
			msg = Content;
			user = User;
		}

		public void Deconstruct(out string msg, out Group group, out User user)
		{
			Deconstruct(out msg, out user);
			group = Group;
		}

		public void Deconstruct(out string msg, out Group group, out User user, out GroupUserType capabilities)
		{
			Deconstruct(out msg, out group, out user);
			capabilities = Capabilities;
		}

		public void Deconstruct(out IWolfClient client, out string msg, out User user)
		{
			msg = Content;
			user = User;
			client = Client;
		}

		public void Deconstruct(out IWolfClient client, out string msg, out Group group, out User user)
		{
			Deconstruct(out client, out msg, out user);
			group = Group;
		}

		public void Deconstruct(out IWolfClient client, out string msg, out Group group, out User user, out GroupUserType capabilities)
		{
			Deconstruct(out client, out msg, out group, out user);
			capabilities = Capabilities;
		}
	}
}
