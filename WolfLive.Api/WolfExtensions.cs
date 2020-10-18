using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace WolfLive.Api
{
	public static class WolfExtensions
	{
		public static readonly Random RANDOM = new Random();

		public static string Token(this string pattern, string characters = "abcedf1234567890", int startIndex = 0)
		{
			var results = pattern.ToCharArray();

			for (var i = startIndex; i < results.Length; i++)
				results[i] = characters[RANDOM.Next(0, characters.Length)];

			return string.Join("", results);
		}

		public static byte[] ToBuffer(this Bitmap bitmap)
		{
			using (var ms = new MemoryStream())
			using (var b = new Bitmap(bitmap))
			{
				b.Save(ms, ImageFormat.Jpeg);
				return ms.GetBuffer();
			}
		}
	}
}
