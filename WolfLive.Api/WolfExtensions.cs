using System;
using System.Collections.Generic;
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

		public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> data, int chunkSize)
		{
			var current = new List<T>();

			foreach(var item in data)
			{
				current.Add(item);
				if (current.Count == chunkSize)
				{
					yield return current.ToArray();
					current = new List<T>();
				}
			}

			if (current.Count == 0)
				yield break;

			yield return current.ToArray();
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
