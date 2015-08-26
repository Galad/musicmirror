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
using log4net;

namespace MusicMirror
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			var viewModel = new AppComposer().Compose();			
			DataContext = viewModel;
			viewModel.Load(CancellationToken.None);
		}		
	}
}
