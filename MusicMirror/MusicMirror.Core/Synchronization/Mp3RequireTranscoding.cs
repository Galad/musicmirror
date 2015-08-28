using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Synchronization
{
	public abstract class FileExtensionRequireTranscoding : IRequireTranscoding
	{
		private readonly string[] _extensions;

		protected FileExtensionRequireTranscoding(params string[] extensions)
		{
			_extensions = Guard.ForNull(extensions, nameof(extensions));
		}

		public Task<bool> ForFile(CancellationToken ct, FileInfo file)
		{
			Guard.ForNull(file, nameof(file));
			if (!_extensions.Any(e => e.Equals(file.Extension, StringComparison.OrdinalIgnoreCase)))
			{
				return Task.FromResult(false);
			}
			return RequireTranscodingForFile(ct, file);
		}

		protected virtual Task<bool> RequireTranscodingForFile(CancellationToken ct, FileInfo file)
		{
			return Task.FromResult(false);
		}
	}

	public sealed class MP3RequireTranscoding : FileExtensionRequireTranscoding
	{
		public MP3RequireTranscoding() : base(AudioFormat.MP3.AllExtensions.ToArray())
		{
		}
	}

	public sealed class FlacRequireTranscoding : FileExtensionRequireTranscoding
	{
		public FlacRequireTranscoding() : base(AudioFormat.FLAC.AllExtensions.ToArray())
		{
		}

		protected override Task<bool> RequireTranscodingForFile(CancellationToken ct, FileInfo file)
		{
			return Task.FromResult(true);
		}
	}

	public sealed class DefaultRequireTranscoding : CompositeRequireTranscoding
	{
		public DefaultRequireTranscoding()
			: base(new IRequireTranscoding[] { new MP3RequireTranscoding(), new FlacRequireTranscoding() })
		{
		}
	}
}
