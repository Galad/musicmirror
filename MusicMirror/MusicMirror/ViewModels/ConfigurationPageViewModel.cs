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
using System.Reactive.Disposables;

namespace MusicMirror.ViewModels
{
	public sealed class ConfigurationPageViewModel : ViewModelBase
	{
		private readonly ISettingsService _settingsService;
		private readonly ISynchronizationController _synchronizationController;		

		public ConfigurationPageViewModel(
			IViewModelServices services,
			ISettingsService settingsService,
			ISynchronizationController synchronizationController) : base(services)
		{
			if (synchronizationController == null) throw new ArgumentNullException(nameof(synchronizationController));
			if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
			_settingsService = settingsService;
			_synchronizationController = synchronizationController;			
		}

		protected override void OnInitialized()
		{
			base.OnInitialized();
			SourcePath = this.GetObservableProperty(() => _settingsService.ObserveValue(SettingsConstants.SourcePath, () => string.Empty), "SourcePath");
			TargetPath = this.GetObservableProperty(() => _settingsService.ObserveValue(SettingsConstants.TargetPath, () => string.Empty), "TargetPath");
			IsSynchronizationEnabled = this.GetObservableProperty(() => _synchronizationController.ObserveSynchronizationIsEnabled(), "IsSynchronizationEnabled");
		}

		public IObservableProperty<string> SourcePath { get; private set; }
		public IObservableProperty<string> TargetPath { get; private set; }
		public IObservableProperty<bool> IsSynchronizationEnabled { get; private set; }

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

		public ICommand EnableSynchronizationCommand
		{
			get
			{
				return CommandBuilderProvider.Get("EnableSynchronizationCommand")
											 .Execute(() => _synchronizationController.Enable())
											 .ToCommand();
			}
		}

		public ICommand DisableSynchronizationCommand
		{
			get
			{
				return CommandBuilderProvider.Get("DisableSynchronizationCommand")
											 .Execute(() => _synchronizationController.Disable())
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
