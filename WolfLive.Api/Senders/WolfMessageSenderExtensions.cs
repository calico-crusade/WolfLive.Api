using System;
using System.Drawing;
using System.Threading.Tasks;

namespace WolfLive.Api
{
	using Models;

	public static class WolfMessageSenderExtensions
	{
		private const string CMD_MESSAGE = "message send";
		private const string CMD_UPDATE = "message update";
		private const string MIMETYPE_TEXT = "text/plain";
		private const string MIMETYPE_IMAGE = "image/jpeg";
		private const string FLIGHT_ID_BASE = "0000000";
		private const int MAX_LENGTH = 1000;
		private const int MSG_INTERVAL = 100;

		public static async Task<MessageResponse> Message(this IWolfClient client, string id, bool isGroup, object data, string mimeType = MIMETYPE_TEXT)
		{
			if (!int.TryParse(id, out int recipient))
				throw new FormatException("Id needs to be a number");

			string msg;
			if (MIMETYPE_TEXT != mimeType || (msg = data.ToString()).Length <= MAX_LENGTH)
				return await client.Emit<MessageResponse>(new Packet(CMD_MESSAGE, new
				{
					recipient,
					data,
					mimeType,
					isGroup,
					flightId = FLIGHT_ID_BASE.Token()
				}));

			MessageResponse results = null;
			for(var i = 0; i < msg.Length; i += MAX_LENGTH)
			{
				var length = i + MAX_LENGTH > msg.Length ? msg.Length - i : MAX_LENGTH;
				var part = msg.Substring(i, length);

				if (i != 0 && MSG_INTERVAL > 0)
					await Task.Delay(MSG_INTERVAL);

				results = await client.Emit<MessageResponse>(new Packet(CMD_MESSAGE, new
				{
					recipient,
					data = part,
					mimeType,
					isGroup,
					flightId = FLIGHT_ID_BASE.Token()
				}));
			}

			return results;
		}

		public static Task<MessageResponse> GroupMessage(this IWolfClient client, string groupId, string content)
		{
			return client.Message(groupId, true, content);
		}

		public static Task<MessageResponse> GroupMessage(this IWolfClient client, string groupId, Bitmap image)
		{
			return client.Message(groupId, true, image.ToBuffer(), MIMETYPE_IMAGE);
		}

		public static Task<MessageResponse> GroupMessage(this IWolfClient client, string groupId, byte[] data)
		{
			return client.Message(groupId, true, data, MIMETYPE_IMAGE);
		}

		public static Task<MessageResponse> PrivateMessage(this IWolfClient client, string userId, string content)
		{
			return client.Message(userId, false, content);
		}

		public static Task<MessageResponse> PrivateMessage(this IWolfClient client, string userId, Bitmap image)
		{
			return client.Message(userId, false, image.ToBuffer(), MIMETYPE_IMAGE);
		}

		public static Task<MessageResponse> PrivateMessage(this IWolfClient client, string userId, byte[] data)
		{
			return client.Message(userId, false, data, MIMETYPE_IMAGE);
		}

		public static Task<MessageResponse> Reply(this Message message, IWolfClient client, string contents)
		{
			return message.IsGroup ? 
				client.GroupMessage(message.GroupId, contents) : 
				client.PrivateMessage(message.UserId, contents);
		}

		public static Task<MessageResponse> Reply(this Message message, IWolfClient client, Bitmap contents)
		{
			return message.IsGroup ?
				client.GroupMessage(message.GroupId, contents) :
				client.PrivateMessage(message.UserId, contents);
		}

		public static Task<MessageResponse> Reply(this Message message, IWolfClient client, byte[] contents)
		{
			return message.IsGroup ?
				client.GroupMessage(message.GroupId, contents) :
				client.PrivateMessage(message.UserId, contents);
		}

		public static Task<MessageResponse> Reply(this IWolfClient client, Message message, string contents)
		{
			return message.IsGroup ?
				client.GroupMessage(message.GroupId, contents) :
				client.PrivateMessage(message.UserId, contents);
		}

		public static Task<MessageResponse> Reply(this IWolfClient client, Message message, Bitmap contents)
		{
			return message.IsGroup ?
				client.GroupMessage(message.GroupId, contents) :
				client.PrivateMessage(message.UserId, contents);
		}

		public static Task<MessageResponse> Reply(this IWolfClient client, Message message, byte[] contents)
		{
			return message.IsGroup ?
				client.GroupMessage(message.GroupId, contents) :
				client.PrivateMessage(message.UserId, contents);
		}

		public static Task<Message> NextMessage(this IWolfClient client, Func<Message, bool> predicate)
		{
			return client.Messaging.NextMessge(predicate);
		}

		public static Task<Message> NextMessage(this IWolfClient client, Message message)
		{
			return client.NextMessage(t =>
			{
				if (t.IsGroup != message.IsGroup)
					return false;

				if (t.IsGroup)
					return message.GroupId == t.GroupId && message.UserId == t.UserId;

				return message.UserId == t.UserId;
			});
		}

		public static Task<Message> NextMessage(this Message message, IWolfClient client)
		{
			return client.NextMessage(message);
		}

		public static Task<Message> NextPrivateMessage(this IWolfClient client, string userId)
		{
			return client.NextMessage(t => !t.IsGroup && t.UserId == userId);
		}

		public static Task<Message> NextGroupMessage(this IWolfClient client, string groupId)
		{
			return client.NextMessage(t => t.IsGroup && t.GroupId == groupId);
		}

		public static Task<Message> NextGroupMessage(this IWolfClient client, string groupId, string userId)
		{
			return client.NextMessage(t => t.IsGroup && t.GroupId == groupId && t.UserId == userId);
		}

		public static Task<WolfMessage> Delete(this IWolfClient client, string id, bool isGroup, DateTime timestamp)
		{
			return Delete(client, id, isGroup, timestamp.Ticks);
		}

		public static Task<WolfMessage> Delete(this IWolfClient client, string id, bool isGroup, long timestamp)
		{
			if (!int.TryParse(id, out int recipientId))
				throw new FormatException("Id needs to be a number");

			return client.Emit<WolfMessage>(new Packet(CMD_UPDATE, new
			{
				recipientId,
				timestamp,
				isGroup,
				metadata = new
				{
					isDeleted = true
				}
			}));
		}

		public static Task<WolfMessage> Delete(this IWolfClient client, Message message)
		{
			return client.Delete(message.IsGroup ? message.GroupId : message.UserId, message.IsGroup, message.Timestamp.Ticks);
		}

		public static Task<WolfMessage> Delete(this Message message, IWolfClient client)
		{
			return client.Delete(message.IsGroup ? message.GroupId : message.UserId, message.IsGroup, message.Timestamp.Ticks);
		}
	}
}
