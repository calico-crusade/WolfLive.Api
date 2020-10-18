using System;

namespace WolfLive.Api.Exceptions
{
	public class WolfSocketPacketException : Exception
	{
		public int Code { get; set; }
		public string Result { get; set; }

		public WolfSocketPacketException(int code, string result) : base($"Error while deserializing packet: {code}\r\n{result}")
		{
			Code = code;
			Result = result;
		}
	}
}
