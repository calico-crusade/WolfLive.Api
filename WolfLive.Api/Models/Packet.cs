using Newtonsoft.Json;
using System.Collections.Generic;

namespace WolfLive.Api
{
	public class Packet<T>
	{
		public string Command { get; set; }
		public T Body { get; set; }
		public Dictionary<string, object> Headers { get; set; }

		public bool HasHeaders => Headers != null && Headers.Count > 0;

		public Packet()
		{
			Headers = new Dictionary<string, object>();
		}

		public Packet(string command, T body) : this(command, body, null) { }

		public Packet(string command, T body, Dictionary<string, object> headers)
		{
			Command = command;
			Body = body;
			Headers = headers;
		}

		public Packet(string command, T body, int version) : this(command, body, new Dictionary<string, object> { {"version", version} }) { }

		public object ToData()
		{
			return new
			{
				headers = Headers,
				body = Body
			};
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}
	}

	public class Packet : Packet<object>
	{
		public Packet() : base() { }

		public Packet(string command) : base(command, new { }) { }

		public Packet(string command, object body) : base(command, body) { }

		public Packet(string command, object body, Dictionary<string, object> headers) : base(command, body, headers) { }

		public Packet(string command, object body, int version) : base(command, body, version) { }
	}
}
