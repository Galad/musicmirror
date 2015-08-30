using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.MediaFoundation;
using NAudio.Wave;

namespace MusicMirror.Transcoding
{
	/// <summary>
	/// Transcode a Wave stream in another stream
	/// </summary>
	public interface IWaveStreamTranscoder
	{
		string GetTranscodedFileName(string sourceFileName);
		Task Transcode(CancellationToken ct, IWaveStreamProvider stream, Stream targetStream);
	}

	public sealed class WaveToMP3Transcoder : IWaveStreamTranscoder
	{
		public string GetTranscodedFileName(string sourceFileName)
		{
			return Path.ChangeExtension(sourceFileName, AudioFormat.MP3.DefaultExtension);
		}

		public Task Transcode(CancellationToken ct, IWaveStreamProvider stream, Stream targetStream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (targetStream == null) throw new ArgumentNullException(nameof(targetStream));
			return TranscodeInternal(ct, stream, targetStream);
		}

		public static async Task TranscodeInternal(CancellationToken ct, IWaveStreamProvider stream, Stream targetStream)
		{
			using (var mp3Writer = new NAudio.Lame.LameMP3FileWriter(targetStream, stream.WaveFormat, NAudio.Lame.LAMEPreset.VBR_90))
			{
				await stream.Stream.CopyToAsync(mp3Writer, 81920, ct);
			}
		}
	}

	public sealed class WaveToMP3MediaFoundationTranscoder : IWaveStreamTranscoder
	{
		public string GetTranscodedFileName(string sourceFileName)
		{
			return Path.ChangeExtension(sourceFileName, AudioFormat.MP3.DefaultExtension);
		}

		public Task Transcode(CancellationToken ct, IWaveStreamProvider stream, Stream targetStream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (targetStream == null) throw new ArgumentNullException(nameof(targetStream));
			return TranscodeInternal(ct, stream, targetStream);
		}

		public async Task TranscodeInternal(CancellationToken ct, IWaveStreamProvider stream, Stream targetStream)
		{
			var tempFilename = Path.Combine(Path.GetTempPath(), GetTranscodedFileName(Guid.NewGuid().ToString() + ".extension"));			
			MediaFoundationEncoder.EncodeToMp3(stream, tempFilename, 320000);			
			using (var tempStream = new FileStream(tempFilename, FileMode.Open, FileAccess.Read))
			{
				await tempStream.CopyToAsync(targetStream, 81920, ct);
			}
			File.Delete(tempFilename);
		}
	}

	public sealed class RawWaveTranscoder : IWaveStreamTranscoder
	{
		public string GetTranscodedFileName(string sourceFileName)
		{
			return Path.ChangeExtension(sourceFileName, ".wav");
		}

		public async Task Transcode(CancellationToken ct, IWaveStreamProvider stream, Stream targetStream)
		{
			using (var fileWriter = new NAudio.Wave.WaveFileWriter(targetStream, stream.WaveFormat))
			{
				await stream.Stream.CopyToAsync(fileWriter);
			}
		}
	}
}
