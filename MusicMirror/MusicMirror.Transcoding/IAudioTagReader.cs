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
	public interface IAudioTagReader
	{
		Task<Tag> ReadTags(CancellationToken ct, Stream stream);
	}
}
