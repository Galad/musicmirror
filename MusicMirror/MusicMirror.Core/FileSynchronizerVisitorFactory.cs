using System;
using Hanno.IO;
using MusicMirror.Synchronization;

namespace MusicMirror
{
	public class FileSynchronizerVisitorFactory : IFileSynchronizerVisitorFactory
	{
		private readonly IFileTranscoder _transcoder;

		public FileSynchronizerVisitorFactory(IFileTranscoder transcoder)
		{
			if (transcoder == null) throw new ArgumentNullException(nameof(transcoder));
			_transcoder = transcoder;
		}

		public IFileSynchronizerVisitor CreateVisitor(Configuration configuration)
		{
			var fileSynchronizerRepository = new SynchronizedFilesRepository(
				_transcoder,
				configuration);
			var fileOperations = new AsyncOperations(new FileOperations());
			var transcodingMirroredFileOperations = new SynchronizeFileService(
					fileOperations,
					configuration,
					new FileSynchronizer(
						fileSynchronizerRepository,
						_transcoder),
					fileSynchronizerRepository);
			var nonTranscodingFileService = new SynchronizeFileService(
				GetFileOperations(configuration.NonTranscodingFilesBehavior, fileOperations),
				configuration,
				GetSynchronizer(configuration, fileOperations, new AsyncDirectoryOperations(new DirectoryOperations())),
				fileSynchronizerRepository);
			return new FileSynchronizeVisitor(
				new ChooseFolderOperation(
					new DefaultRequireTranscoding(),
					nonTranscodingFileService,
					transcodingMirroredFileOperations));
		}

		private IAsyncFileOperations GetFileOperations(NonTranscodingFilesBehavior behavior, IAsyncFileOperations defaultFileOperations)
		{
			switch (behavior)
			{
			case NonTranscodingFilesBehavior.Copy:
				return defaultFileOperations;
			case NonTranscodingFilesBehavior.SymbolicLink:
				return new SymbolicLinkFileOperations(defaultFileOperations);
			default:
				throw new PlatformNotSupportedException();
			}
		}

		private IFileSynchronizer GetSynchronizer(Configuration configuration, IAsyncFileOperations fileOperations, IAsyncDirectoryOperations directoryOperations)
		{
			switch (configuration.NonTranscodingFilesBehavior)
			{
			case NonTranscodingFilesBehavior.Copy:
				return new CopyFileSynchronizer(configuration, directoryOperations);
			case NonTranscodingFilesBehavior.SymbolicLink:
				return new SymbolicLinkFileSynchronizer(configuration, fileOperations, directoryOperations);
			default:
				throw new NotSupportedException();
			}
		}
	}
}