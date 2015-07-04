using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror
{
	public static class Guard
	{
		public static T ForNull<T>(T value, string argumentName) where T : class
		{
			if (value == null)
			{
				throw new ArgumentNullException(argumentName);
			}
			return value;
		}

		public static string ForNullOrWhiteSpace(string value, string argumentName)
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentNullException(argumentName);
			}
			return value;
		}
	}
}
