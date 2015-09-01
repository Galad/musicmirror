using FluentAssertions;
using MusicMirror.Transcoding;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MusicMirror.FunctionalTests.Utils
{
	internal static class TestContextUtils
	{
		public static void AssertThatTargetFileExistInTargetDirectory(this TestContext context, string relativePath)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentNullException(nameof(relativePath));
			Assert.True(File.Exists(context.GetTargetFileFromRelativePath(relativePath)), $"The file {relativePath} does not exist in the target directory");
		}

		public static string GetTargetFileFromRelativePath(this TestContext context, string relativePath)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentNullException(nameof(relativePath));
			return Path.Combine(context.TargetDirectory, relativePath);
		}

		public static string GetSourceFileFromRelativePath(this TestContext context, string relativePath)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentNullException(nameof(relativePath));
			return Path.Combine(context.SourceDirectory, relativePath);
		}

		public static void AssertThatTargetFileHasTheCorrectDuration(this TestContext context, string expectedFile, string actualFile)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (string.IsNullOrWhiteSpace(expectedFile)) throw new ArgumentNullException(nameof(expectedFile));
			if (string.IsNullOrWhiteSpace(actualFile)) throw new ArgumentNullException(nameof(actualFile));
			var expectedDuration = GetFileDuration(context.GetSourceFileFromRelativePath(expectedFile));
			var actualDuration = GetFileDuration(context.GetTargetFileFromRelativePath(actualFile));
			actualDuration.Should().BeCloseTo(expectedDuration, 5000);
		}

		private static TimeSpan GetFileDuration(string file)
		{
			switch (Path.GetExtension(file))
			{
				case ".mp3":
					return GetFileDurationMP3(file);
				case ".flac":
					return GetFileDurationFlac(file);
				default:
					throw new InvalidOperationException($"The file {file} is not supported");
			}
		}

		public static TimeSpan GetFileDurationFlac(string file)
		{
			using (var fs = File.OpenRead(file))
			using (var reader = new NAudio.Flac.FlacReader(fs))
			{
				return reader.TotalTime;
			}
		}

		public static TimeSpan GetFileDurationMP3(string file)
		{
			double duration = 0.0;
			using (FileStream fs = File.OpenRead(file))
			{
				Mp3Frame frame = Mp3Frame.LoadFromStream(fs);
				while (frame != null)
				{
					duration += (double)frame.SampleCount / frame.SampleRate;
					frame = Mp3Frame.LoadFromStream(fs);
				}
			}
			return TimeSpan.FromSeconds(duration);
		}

		/// <summary>
		/// Gets the duration based on MediaFoundation APIs
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public static TimeSpan GetMediaFoundationDurationMP3(string file)
		{
			using (var reader = new MediaFoundationReader(file))
			{
				return reader.TotalTime;
			}
		}

		public static CancellationToken CreateShortTimedOutCancellationToken()
		{
			return new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
		}
	}
}
