using System;
using Hanno.IO;
using MusicMirror.Synchronization;

using System.Threading;
using System.Threading.Tasks;
using Hanno;

namespace MusicMirror
{
	public sealed class EmptyFileSynchronizerVisitorFactory : IFileSynchronizerVisitorFactory
	{
		public IFileSynchronizerVisitor CreateVisitor(MusicMirrorConfiguration configuration)
		{
			return new EmptyFileSynchronizerVisitor();
		}

		private class EmptyFileSynchronizerVisitor : IFileSynchronizerVisitor
		{
			public Task Visit(CancellationToken ct, FileModifiedNotification notification)
			{
				return Task.FromResult(true);				
			}

			public Task Visit(CancellationToken ct, FileInitNotification notification)
			{
				return Task.FromResult(true);
			}

			public Task Visit(CancellationToken ct, FileRenamedNotification notification)
			{
				return Task.FromResult(true);
			}

			public Task Visit(CancellationToken ct, FileDeletedNotification notification)
			{
				return Task.FromResult(true);
			}

			public Task Visit(CancellationToken ct, FileAddedNotification notification)
			{
				return Task.FromResult(true);
			}
		}
	}

	public sealed class FileSynchronizerVisitorFactory : IFileSynchronizerVisitorFactory
	{
		private readonly IFileTranscoder _transcoder;

		public FileSynchronizerVisitorFactory(IFileTranscoder transcoder)
		{
			if (transcoder == null) throw new ArgumentNullException(nameof(transcoder));
			_transcoder = transcoder;
		}

		public IFileSynchronizerVisitor CreateVisitor(MusicMirrorConfiguration configuration)
		{
			var fileSynchronizerRepository = new SynchronizedFilesRepository(
				_transcoder,
				configuration,
                new SystemUtcNow());
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

		private static IAsyncFileOperations GetFileOperations(NonTranscodingFilesBehavior behavior, IAsyncFileOperations defaultFileOperations)
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

		private static IFileSynchronizer GetSynchronizer(MusicMirrorConfiguration configuration, IAsyncFileOperations fileOperations, IAsyncDirectoryOperations directoryOperations)
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