using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror.Transcoding
{
	/// <summary>
	/// A StreamFileAbstraction which does not close the stream when the methode CloseStream is called, but instead position it to 0.
	/// In order to close the the stream the methode Dispose must be closed
	/// </summary>
	public class AlwaysOpenStreamFileAbstraction : TagLib.File.IFileAbstraction, IDisposable
	{
		private readonly Stream _stream;
		private readonly string _name;

		public AlwaysOpenStreamFileAbstraction(string name, Stream stream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			_stream = stream;	
			_name = name;
		}

		public string Name { get { return _name; } }

		public Stream ReadStream { get { return _stream; } }

		public Stream WriteStream { get { return _stream; } }

		public void CloseStream(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			stream.Position = 0;
		}

		public void Dispose()
		{
			WriteStream.Dispose();
		}
	}
}
