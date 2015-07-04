using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Synchronization
{
	public interface IRequireTranscoding
	{
		Task<bool> ForFile(CancellationToken ct, FileInfo file);
	}
}
