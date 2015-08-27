using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TagLib;

namespace MusicMirror.Transcoding
{
	public interface IAudioTagWriter
	{
		Task WriteTags(CancellationToken ct, Stream stream, Tag tags);
	}
}
