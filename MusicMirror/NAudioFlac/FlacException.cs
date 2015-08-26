using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BigMansStuff.NAudio.FLAC
{
	public class FlacException : Exception
	{
		public FlacException()
		{
		}

		public FlacException(string message) : base(message)
		{
		}
	}
}
