using System;

namespace WolfLive.Api.Commands
{
	[AttributeUsage(AttributeTargets.Method)]
	public class DescriptionAttribute : Attribute
	{
		public string Text { get; }

		public DescriptionAttribute(string text)
		{
			Text = text;
		}
	}
}
