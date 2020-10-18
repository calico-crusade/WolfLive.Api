using System;
using System.Threading.Tasks;

namespace WolfLive.Api.Models
{
	public class MessageTask
	{
		public TaskCompletionSource<Message> TaskSource { get; set; }
		public Func<Message, bool> Predicate { get; set; }
	}
}
