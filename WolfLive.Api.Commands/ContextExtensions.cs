using System;
using System.Drawing;
using System.Threading.Tasks;

namespace WolfLive.Api.Commands
{
	using Models;

	public static class ContextExtensions
	{
		public static Task<WolfMessage> DeleteMessage(this WolfContext context, Message message)
		{
			return context.Client.Delete(message);
		}

		public static Task<WolfMessage> DeleteMessage(this WolfContext context)
		{
			return context.Client.Delete(context.Message);
		}

		public static Task<MessageResponse> Reply(this WolfContext context, string message)
		{
			return context.Client.Reply(context.Message, message);
		}

		public static Task<MessageResponse> Reply(this WolfContext context, Bitmap message)
		{
			return context.Client.Reply(context.Message, message);
		}

		public static Task<MessageResponse> Reply(this WolfContext context, byte[] message)
		{
			return context.Client.Reply(context.Message, message);
		}

		public static Task<Message> Next(this WolfContext context)
		{
			return context.Client.NextMessage(context.Message);
		}

		public static Task<Message> Next(this WolfContext context, Func<Message, bool> predicate)
		{
			return context.Client.NextMessage(predicate);
		}


		public static Task<(Message message, GroupUser user)> NextGroupMessage(this WolfContext context)
		{
			if (!context.Message.IsGroup)
				throw new Exception("Current context is not a group. Cannot find group Id");

			return context.NextGroupMessage(context.Message.GroupId);
		}

		public static async Task<(Message message, GroupUser user)> NextGroupMessage(this WolfContext context, string groupId)
		{
			var msg = await context.Client.NextGroupMessage(groupId);
			var user = await context.Client.GetGroupUser(msg.GroupId, msg.UserId);
			return (msg, user);
		}

		public static async Task<(Message message, User user)> NextPrivateMessage(this WolfContext context, string userId)
		{
			var msg = await context.Client.NextPrivateMessage(userId);
			var user = await context.Client.GetUser(userId);
			return (msg, user);
		}

		public static Task<(Message message, User user)> NextPrivateMessage(this WolfContext context)
		{
			return NextPrivateMessage(context, context.Message.UserId);
		}
	}
}
