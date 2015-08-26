using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Hanno.Extensions;
using Hanno.Services;
using Hanno.ViewModels;

namespace MusicMirror.ViewModels
{
	public class ConfigurationPageViewModel : ViewModelBase
	{
		private readonly ISettingsService _settingsService;

		public ConfigurationPageViewModel(IViewModelServices services, ISettingsService settingsService) : base(services)
		{
			if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
			_settingsService = settingsService;
		}

		protected override void OnInitialized()
		{
			base.OnInitialized();			
		}

		public IObservableProperty<string> SourcePath
		{
			get
			{
				return this.GetObservableProperty(() => _settingsService.ObserveValue("SourcePath", () => string.Empty));
			}
		}

		public IObservableProperty<string> TargetPath
		{
			get
			{
				return this.GetObservableProperty(() => _settingsService.ObserveValue("TargetPath", () => string.Empty));
			}
		}

		public ICommand SaveCommand
		{
			get
			{
				return CommandBuilderProvider.Get("SaveCommand")
					.Execute(async ct => await SaveSettings(ct))
					.CanExecute(CanExecuteSaveCommand())
					.ToCommand();
			}
		}

		private async Task SaveSettings(CancellationToken ct)
		{
			await _settingsService.SetValue("SourcePath", SourcePath.Value, ct);
			await _settingsService.SetValue("TargetPath", TargetPath.Value, ct);
        }

		private IObservable<bool> CanExecuteSaveCommand()
		{
			return Observable.CombineLatest(SourcePath, TargetPath)
							 .Select(paths => paths.All(path => !string.IsNullOrEmpty(path)));
		}
	}
}
