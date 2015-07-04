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
		private readonly IEnumerable<IRequireTranscoding> _requireTranscodings;

		public CompositeRequireTranscoding(IEnumerable<IRequireTranscoding> requireTranscodings)
		{
			this._requireTranscodings = Guard.ForNull(requireTranscodings, nameof(requireTranscodings));
		}

		public IEnumerable<IRequireTranscoding> RequireTranscodings { get { return _requireTranscodings; } }

        public Task<bool> ForFile(CancellationToken ct, FileInfo file)
		{
			Guard.ForNull(file, nameof(file));
			return ForFileInternal(ct, file);
		}

		private async Task<bool> ForFileInternal(CancellationToken ct, FileInfo file)
		{
			foreach (var t in _requireTranscodings)
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
