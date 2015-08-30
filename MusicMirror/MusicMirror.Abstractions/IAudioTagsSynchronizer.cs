using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror
{
	public interface IAudioTagsSynchronizer
	{
		Task SynchronizeTags(CancellationToken ct, FileInfo sourceFile, FileInfo targetFile);
	}	
}
