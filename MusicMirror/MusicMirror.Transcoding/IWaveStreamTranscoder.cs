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
			using (var mp3Writer = new NAudio.Lame.LameMP3FileWriter(targetStream, stream.WaveFormat, NAudio.Lame.LAMEPreset.ABR_320))
			{				
				await stream.Stream.CopyToAsync(mp3Writer);
				//var bytesRead = 1;
    //            while (bytesRead > 0)
				//{
				//	var buffer = new byte[4096*8];
				//	bytesRead = await stream.Stream.ReadAsync(buffer, 0, buffer.Length, ct);
				//	await mp3Writer.WriteAsync(buffer, 0, bytesRead);
    //            }
			}
		}
	}

	public class WaveToMp3MediaFoundationTranscoder : IWaveStreamTranscoder
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
			await Task.Run(() =>
			{				
				var mediaType = MediaFoundationEncoder.SelectMediaType(AudioSubtypes.MFAudioFormat_MP3, stream.WaveFormat, 48000);
				using (var encoder = new MediaFoundationEncoder(mediaType))
				{
					encoder.Encode(GetTranscodedFileName("file.ww"), stream);
				}
			});
			//try
			//{
			//	var tempFilename = Path.Combine(Path.GetTempPath(), GetTranscodedFileName(Guid.NewGuid().ToString() + ".extension"));
			//	MediaFoundationEncoder.EncodeToMp3(stream, tempFilename, 192000);
			//	using (var tempStream = new FileStream(tempFilename, FileMode.Open, FileAccess.Read))
			//	{
			//		await tempStream.CopyToAsync(targetStream);
			//	}
			//}
			//catch (System.Runtime.InteropServices.COMException ex)
			//{
			//	System.Diagnostics.Debug.WriteLine(ex.ErrorCode);
			//	System.Diagnostics.Debug.WriteLine(ex.HResult);
			//}
		}
	}

	public class RawWaveTranscoder : IWaveStreamTranscoder
	{
		public string GetTranscodedFileName(string sourceFileName)
		{
			return Path.ChangeExtension(sourceFileName, ".wav");
		}

		public async Task Transcode(CancellationToken ct, WaveStream stream, Stream targetStream)
		{
			using (var fileWriter = new NAudio.Wave.WaveFileWriter(targetStream, stream.WaveFormat))
			{				
				await stream.Stream.CopyToAsync(fileWriter);
			}
		}
	}
}
