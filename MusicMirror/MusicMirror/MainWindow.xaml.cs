using System;
using System.Diagnostics;
using System.Threading;
using Hanno.CqrsInfrastructure;
using Hanno.Diagnostics;
using Hanno.IO;
using Hanno.Navigation;
using Hanno.Rx;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Windows;
using Hanno.MVVM.Services;
using Hanno.Serialization;
using Hanno.Services;
using Hanno.Storage;
using Hanno.Unity;
using Hanno.Validation;
using Hanno.ViewModels;
using Microsoft.Practices.Unity;
using MusicMirror.Synchronization;
using MusicMirror.ViewModels;
using MusicMirror.Transcoding;
using MusicMirror.Logging;
using System.Reactive;

namespace MusicMirror
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public MainWindow()
		{
			InitializeComponent();
			var composer = new AppComposer();
            var viewModel = composer.Compose();
			composer.Resolve<IObservable<Unit>>("SynchronizeFilesWhenFileChanged")
					.Subscribe(_ => { }, e => e.DebugWriteline(), () => Debug.WriteLine("SynchronizeFilesWhenFileChanged complete"));
			DataContext = viewModel;
			viewModel.Load(CancellationToken.None);
		}		
	}
}
