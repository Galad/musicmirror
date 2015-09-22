
using Hanno;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Synchronization
{
    public sealed class SynchronizedFilesRepository : ISynchronizedFilesRepository
    {
        private readonly IFileTranscoder _fileTranscoder;
        private readonly MusicMirrorConfiguration _configuration;
        private readonly ConcurrentDictionary<string, DateTimeOffset> _synchronizations;
        private readonly INow _now;

        public SynchronizedFilesRepository(
            IFileTranscoder fileTranscoder,
            MusicMirrorConfiguration configuration,
            INow now)
        {
            if (now == null) throw new ArgumentNullException(nameof(now), $"{nameof(now)} is null.");
            if (fileTranscoder == null) throw new ArgumentNullException(nameof(fileTranscoder));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _fileTranscoder = fileTranscoder;
            _configuration = configuration;
            _now = now;
            _synchronizations = new ConcurrentDictionary<string, DateTimeOffset>();
        }

        public IFileTranscoder FileTranscoder { get { return _fileTranscoder; } }

        public MusicMirrorConfiguration Configuration { get { return _configuration; } }

        public INow Now { get { return _now; } }

        public Task AddSynchronization(CancellationToken ct, FileInfo sourceFile)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));
            var now = _now.Now;
            _synchronizations.AddOrUpdate(sourceFile.FullName, now, (s, offset) => now);
            return Task.FromResult(true);
        }

        public Task DeleteSynchronization(CancellationToken ct, FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
            DateTimeOffset time;
            _synchronizations.TryRemove(fileInfo.FullName, out time);
            return Task.FromResult(true);
        }

        public Task<DateTimeOffset> GetLastSynchronization(CancellationToken ct, FileInfo sourceFile)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));
            DateTimeOffset time;
            if (!_synchronizations.TryGetValue(sourceFile.FullName, out time))
            {
                return Task.FromResult(DateTimeOffset.MinValue);
            }
            return Task.FromResult(time);
        }

        public Task<FileInfo> GetMirroredFilePath(CancellationToken ct, FileInfo sourceFile)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));
            var fileName = _fileTranscoder.GetTranscodedFileName(sourceFile.Name);
            var mirroredFile = new FileInfo(Path.Combine(sourceFile.GetDirectoryFromSourceFile(_configuration).FullName, fileName));
            return Task.FromResult(mirroredFile);
        }
    }
}