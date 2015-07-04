using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Transcoding
{
	/// <summary>
	/// Transcode a Wave stream in another stream
	/// </summary>
	public interface IWaveStreamTranscoder
	{
		string GetTranscodedFileName(string sourceFileName);
		Task Transcode(CancellationToken ct, WaveStream stream, Stream targetStream);
	}

	public class WaveToMp3Transcoder : IWaveStreamTranscoder
	{
		public string GetTranscodedFileName(string sourceFileName)
		{
			return Path.ChangeExtension(sourceFileName, AudioFormat.Mp3.DefaultExtension);
		}

		public Task Transcode(CancellationToken ct, WaveStream stream, Stream targetStream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (targetStream == null) throw new ArgumentNullException(nameof(targetStream));
			return TranscodeInternal(ct, stream, targetStream);
		}

		public async Task TranscodeInternal(CancellationToken ct, WaveStream stream, Stream targetStream)
		{
			using (var mp3Writer = new NAudio.Lame.LameMP3FileWriter(targetStream, stream.Format, NAudio.Lame.LAMEPreset.ABR_320))
			{
				await stream.Stream.CopyToAsync(mp3Writer);
			}
		}
	}
}
