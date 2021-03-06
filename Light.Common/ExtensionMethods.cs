using System.Collections.Generic;

namespace System
{
	public static class ExtensionMethods
	{
		public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
		{
			foreach (var item in enumeration)
			{
				action(item);
			}
		}
	}
}