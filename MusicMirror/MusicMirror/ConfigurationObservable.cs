using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Hanno.Services;
using MusicMirror.Entities;

namespace MusicMirror
{
	public class ConfigurationObservable : IObservable<Configuration>
	{
		private readonly ISettingsService _settingsService;
		public const string SourcePathKey = "SourcePath";
		public const string TargetPathKey = "TargetPath";

		public ConfigurationObservable(ISettingsService settingsService)
		{
			if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
			_settingsService = settingsService;
		}

		public IDisposable Subscribe(IObserver<Configuration> observer)
		{
			return ObserveSettingsPath(SourcePathKey).CombineLatest(ObserveSettingsPath(TargetPathKey), (s1, s2) => new Configuration(s1, s2, NonTranscodingFilesBehavior.Copy))
			                                        .Subscribe(observer);
		}

		private IObservable<DirectoryInfo> ObserveSettingsPath(string settingsName)
		{
			return _settingsService.ObserveValue(settingsName, () => string.Empty)
			                       .DistinctUntilChanged()
			                       .Select(d => string.IsNullOrEmpty(d) ? null : new DirectoryInfo(d));
		} 
	}
}