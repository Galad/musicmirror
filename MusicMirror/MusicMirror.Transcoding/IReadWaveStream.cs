﻿using System;
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
		Task<IWaveStream> ReadWave(CancellationToken ct, Stream sourceStream, AudioFormat format);
	}

	public interface IWaveStream : IWaveProvider, IDisposable
	{		
		Stream Stream { get; }	
	}

	public sealed class WaveStream : IWaveStream
	{
		private readonly WaveFormat _format;
		private readonly NAudio.Wave.WaveStream _stream;

		public WaveStream(NAudio.Wave.WaveStream stream, WaveFormat format)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (format == null) throw new ArgumentNullException(nameof(format));
			_stream = stream;
			_format = format;
		}

		public WaveFormat WaveFormat
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

		public int Read(byte[] buffer, int offset, int count)
		{
			return _stream.Read(buffer, offset, count);
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

		public Task<IWaveStream> ReadWave(CancellationToken ct, Stream sourceStream, AudioFormat format)
		{
			if (format == null) throw new ArgumentNullException(nameof(format));
			return ReadWaveInternal(ct, sourceStream, format);
		}

		private async Task<IWaveStream> ReadWaveInternal(CancellationToken ct, Stream sourceStream, AudioFormat format)
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
