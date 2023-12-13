using Reactive.Bindings;
using System.Collections.Generic;

namespace FileOrganizer.Models
{
	internal static class Log
	{
		public static ReactivePropertySlim<string> Text { get; } = new();

		private static readonly Queue<string> strings = new();

		public static void Append(string str)
		{
			foreach (var l in str.Split("\r\n")) {
				foreach (var s in l.Split(new char[] { '\r', '\n' })) {
					System.Diagnostics.Debug.WriteLine(s);
					strings.Enqueue(s);
				}
			}
			while (strings.Count > 100) {
				strings.Dequeue();
			}
			Text.Value = string.Join("\r\n", strings);
		}
	}
}
