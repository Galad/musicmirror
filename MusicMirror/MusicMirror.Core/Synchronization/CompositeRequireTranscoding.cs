using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Synchronization
{
	public class CompositeRequireTranscoding : IRequireTranscoding
	{
		private readonly IEnumerable<IRequireTranscoding> _innerRequireTranscoding;

		public CompositeRequireTranscoding(IEnumerable<IRequireTranscoding> innerRequireTranscoding)
		{
			this._innerRequireTranscoding = Guard.ForNull(innerRequireTranscoding, nameof(innerRequireTranscoding));
		}

		public IEnumerable<IRequireTranscoding> InnerRequireTranscoding { get { return _innerRequireTranscoding; } }

        public Task<bool> ForFile(CancellationToken ct, FileInfo file)
		{
			Guard.ForNull(file, nameof(file));
			return ForFileInternal(ct, file);
		}

		private async Task<bool> ForFileInternal(CancellationToken ct, FileInfo file)
		{
			foreach (var t in _innerRequireTranscoding)
			{
				if (await t.ForFile(ct, file))
				{
					return true;
				}
			}
			return false;
		}
	}
}
