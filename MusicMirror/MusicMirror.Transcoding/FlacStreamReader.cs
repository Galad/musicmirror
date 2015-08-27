﻿using System;
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
		public Task<IWaveStream> ReadWave(CancellationToken ct, Stream sourceStream, AudioFormat format)
		{
			return Task.Run(() =>
			{
				var stream = new NAudio.Flac.FlacReader(sourceStream);
				return (IWaveStream)(new WaveStream(stream, stream.WaveFormat));
			});			
		}
	}

	public class FlacStreamReaderInternalNAudioFlac : IAudioStreamReader
	{
		public Task<IWaveStream> ReadWave(CancellationToken ct, Stream sourceStream, AudioFormat format)
		{
			return Task.Run(() =>
			{
				var stream = new WaveStreamFromFlacStream(sourceStream);
				return (IWaveStream)(new WaveStream(stream, stream.WaveFormat));
			});
		}
	}
}