using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hanno.Testing.Autofixture;
using MusicMirror.ViewModels;

namespace MusicMirror.Specs
{
	public class TestComposer : Composer
	{
		public TestComposer() : base(() => new TestSchedulers())
		{
		}
	}
	public class TestsContext : IDisposable
	{
		private readonly ConfigurationPageViewModel _viewModel;
		private readonly Composer _composer;

		public TestsContext()
		{
			var settingsPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Settings");
			if (Directory.Exists(settingsPath))
			{
				Directory.Delete(settingsPath, true);
			}
			_composer = new TestComposer();
			_viewModel = _composer.Compose();
            _viewModel.Load(CancellationToken.None);			
        }

		public ConfigurationPageViewModel ViewModel
		{
			get
			{
				return _viewModel;
			}
		}

		public void Dispose()
		{
			_composer.Dispose();
			_viewModel.Dispose();
		}
	}
}
