using Hanno.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MusicMirror.ViewModels;
using System.Threading;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using Xunit;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Diagnostics;

namespace MusicMirror.FunctionalTests.Utils
{
	internal sealed class TestContext : IDisposable
	{
		private readonly Composer _composer;
		private readonly string _rootFolder;
		private readonly string _sourceDirectory;
		private readonly string _targetDirectory;
		private readonly TestFilesRepository _testFilesRepository;
		private readonly ConfigurationPageViewModel _viewModel;

		public TestContext(Composer composer, TestFilesRepository testFilesRepository, string testFolder)
		{
			if (composer == null) throw new ArgumentNullException(nameof(composer));
			if (string.IsNullOrWhiteSpace(testFolder)) throw new ArgumentNullException(nameof(testFolder));
			_viewModel = composer.Compose();
			_composer = composer;
			_rootFolder = testFolder;
			_sourceDirectory = Path.Combine(_rootFolder, "Source");
			_targetDirectory = Path.Combine(_rootFolder, "Target");
			_testFilesRepository = testFilesRepository;
			if (!Directory.Exists(SourceDirectory)) { Directory.CreateDirectory(SourceDirectory); }
			if (!Directory.Exists(TargetDirectory)) { Directory.CreateDirectory(TargetDirectory); }            
        }

		public async Task Load(CancellationToken ct)
		{
			await _viewModel.Load(ct);
            ViewModel.SourcePath.OnNext(_sourceDirectory);
            ViewModel.TargetPath.OnNext(_targetDirectory);
            ViewModel.SaveCommand.Execute(null);
        }

		public void Dispose()
		{
			_composer.Dispose();
			try
			{
				Directory.Delete(_rootFolder, true);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		public string SourceDirectory => _sourceDirectory;
		public string TargetDirectory => _targetDirectory;

		public class TestFilesSetup
		{
			private readonly string _folder;
			private TestFilesRepository _testFilesRepository;

			public TestFilesSetup(string folder, TestFilesRepository _testFilesRepository)
			{
				this._testFilesRepository = _testFilesRepository;
				this._folder = folder;
			}

			public TestFilesSetup WithFile(string filename)
			{
				_testFilesRepository.CopyTestFileTo(filename, _folder);
				return this;
			}
		}

		public TestFilesSetup SourceDirectorySetup(string folder = "")
		{
			return new TestFilesSetup(Path.Combine(SourceDirectory, folder), _testFilesRepository);
		}

		public async Task ExecuteSynchronization(CancellationToken ct)
		{
			await ViewModel.Load(ct);
			var controller = _composer.Resolve<ITranscodingNotifications>();
            var synchronizationCompleteTask = controller.ObserveIsTranscodingRunning()
                                                        .Where(t => t)
                                                        .Take(1)
                                                        .Select(_ => controller.ObserveIsTranscodingRunning())
                                                        .Switch()
                                                        .Where(t => !t)
                                                        .Take(1)
                                                        .ObserveOn(ThreadPoolScheduler.Instance)                                                        
                                                        .ToTask(ct)
                                                        .ConfigureAwait(false);			
            ViewModel.EnableSynchronizationCommand.Execute(null);
			await synchronizationCompleteTask;
		}		

		public ISynchronizationController SynchronizationController { get { return _composer.Resolve<ISynchronizationController>(); } }
		public ITranscodingNotifications SynchronizationNotifications { get { return _composer.Resolve<ITranscodingNotifications>(); } }

		public ConfigurationPageViewModel ViewModel
		{
			get
			{
				return _viewModel;
			}
		}		
	}

	internal class TestFilesRepository
	{
		private readonly string _testFilesSource;

		public TestFilesRepository(string testFilesSource)
		{
			if (string.IsNullOrWhiteSpace(nameof(testFilesSource))) throw new ArgumentNullException(nameof(testFilesSource));
			_testFilesSource = testFilesSource;
		}

		public void CopyTestFileTo(string file, string copyTo)
		{				
			File.Copy(Path.Combine(_testFilesSource, file), Path.Combine(copyTo, Path.GetFileName(file)));
		}
	}

	public enum AudioFormatEnum
	{
		MP3,
		Flac
	}

	public class TestFilesConstants
	{
		public static class MP3
		{
			public const string Folder = "MP3";
			public const string NormalFile1 = "NormalFile1.mp3";
			public const string FileWithWrongDisplayedDuration = "FileWithWrongDisplayedDuration.mp3";
            public const string SourceNormalFile1 = Folder + "\\" + NormalFile1;
			public const string SourceFileWithWrongDisplayedDuration = Folder + "\\" + FileWithWrongDisplayedDuration;
		}

		public static class Flac
		{
			public const string Folder = "FLAC";
			public const string NormalFile1 = "NormalFile1.flac";
			public const string SourceNormalFile1 = Folder + "\\" + NormalFile1;
		}
	}
}