using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MusicMirror
{
	public interface IAudioStreamReader
	{
		Task<WaveStream> ReadWave(CancellationToken ct, Stream sourceStream, AudioFormat format);
	}

	public class WaveStream : IDisposable
	{
		private readonly WaveFormat _format;
		private readonly Stream _stream;

		public WaveStream(Stream stream, WaveFormat format)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (format == null) throw new ArgumentNullException(nameof(format));
			_stream = stream;
			_format = format;
		}

		public WaveFormat Format
		{
			get
			{
				return _format;
			}
		}

		public Stream Stream
		{
			get
			{
				return _stream;
			}
		}

		public void Dispose()
		{
			_stream.Dispose();
		}
	}

	public class AudioStreamReaderProvider : IAudioStreamReader
	{
		private readonly Dictionary<AudioFormat, IAudioStreamReader> _audioStreamReaders = new Dictionary<AudioFormat, IAudioStreamReader>();

		public void Register(AudioFormat format, IAudioStreamReader audioStreamReader)
		{
			if (format == null) throw new ArgumentNullException(nameof(format));
			if (audioStreamReader == null) throw new ArgumentNullException(nameof(audioStreamReader));
			_audioStreamReaders[format] = audioStreamReader;
		}

		public Task<WaveStream> ReadWave(CancellationToken ct, Stream sourceStream, AudioFormat format)
		{
			if (format == null) throw new ArgumentNullException(nameof(format));
			return ReadWaveInternal(ct, sourceStream, format);
		}

		private async Task<WaveStream> ReadWaveInternal(CancellationToken ct, Stream sourceStream, AudioFormat format)
		{
			IAudioStreamReader reader;
			if (_audioStreamReaders.TryGetValue(format, out reader))
			{
				return await reader.ReadWave(ct, sourceStream, format);
			}
			throw new InvalidOperationException(
				string.Format(
					CultureInfo.CurrentCulture,
					"No audio file reader was file for format {0}.",
					format));
		}
	}
}
