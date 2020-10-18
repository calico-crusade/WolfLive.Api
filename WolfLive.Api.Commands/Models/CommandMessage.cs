namespace WolfLive.Api.Commands
{
	using Models;

	public class CommandMessage
	{
		public string Command { get; set; }

		public string Prefix { get; set; }

		public string Remainder { get; set; }

		public Message Message { get; set; }

		public GroupUser GroupUser { get; set; }

		public User User { get; set; }

		public void Deconstruct(out Message message, out User user)
		{
			message = Message;
			user = User;
		}

		public void Deconstruct(out Message message, out User user, out Group group)
		{
			message = Message;
			user = User;
			group = GroupUser?.Group;
		}

		public void Deconstruct(out Message message, out User user, out Group group, out GroupUserType capabilities)
		{
			message = Message;
			user = User;
			group = GroupUser?.Group;
			capabilities = GroupUser?.Capabilities ?? GroupUserType.User;
		}
	}
}
