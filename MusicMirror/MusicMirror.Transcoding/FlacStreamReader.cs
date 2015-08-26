using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BigMansStuff.NAudio.FLAC;
using FlacBox;

namespace MusicMirror.Transcoding
{
	public class FlacStreamReader : IAudioStreamReader
	{
		public Task<WaveStream> ReadWave(CancellationToken ct, Stream sourceStream, AudioFormat format)
		{
			return Task.Run(() =>
			{
				var stream = new NAudio.Flac.FlacReader(sourceStream);
				return new WaveStream(stream, stream.WaveFormat);
			});			
		}
	}

	public class FlacStreamReaderInternalNAudioFlac : IAudioStreamReader
	{
		public Task<WaveStream> ReadWave(CancellationToken ct, Stream sourceStream, AudioFormat format)
		{
			return Task.Run(() =>
			{
				var stream = new WaveStreamFromFlacStream(sourceStream);
				return new WaveStream(stream, stream.WaveFormat);
			});
		}
	}
}
