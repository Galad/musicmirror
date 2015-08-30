using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Hanno.Services;


namespace MusicMirror
{
	public class ConfigurationObservable : IObservable<MusicMirrorConfiguration>
	{
		private readonly ISettingsService _settingsService;
		public const string SourcePathKey = "SourcePath";
		public const string TargetPathKey = "TargetPath";

		public ConfigurationObservable(ISettingsService settingsService)
		{
			if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
			_settingsService = settingsService;
		}

		public IDisposable Subscribe(IObserver<MusicMirrorConfiguration> observer)
		{
			return ObserveSettingsPath(SourcePathKey).CombineLatest(ObserveSettingsPath(TargetPathKey), (s1, s2) => new MusicMirrorConfiguration(s1, s2, NonTranscodingFilesBehavior.Copy))
			                                        .Subscribe(observer);
		}

		private IObservable<DirectoryInfo> ObserveSettingsPath(string settingsName)
		{
			return _settingsService.ObserveValue(settingsName, () => string.Empty)
			                       .DistinctUntilChanged()
			                       .Select(d => string.IsNullOrEmpty(d) ? null : new DirectoryInfo(d));
		} 
	}

	public class FilterValidDirectories : IObservable<MusicMirrorConfiguration>
	{
		private readonly IObservable<MusicMirrorConfiguration> _innerObservable;

		public FilterValidDirectories(IObservable<MusicMirrorConfiguration> innerObservable)
		{
			if (innerObservable == null) throw new ArgumentNullException(nameof(innerObservable));
			_innerObservable = innerObservable;
		}

		public IDisposable Subscribe(IObserver<MusicMirrorConfiguration> observer)
		{
			return _innerObservable
				.Where(c => c.HasValidDirectories)
				.DistinctUntilChanged()
				.Subscribe(observer);
		}
	}

	public class LimitNumberOfConfigurationChanges : IObservable<MusicMirrorConfiguration>
	{
		private readonly IObservable<MusicMirrorConfiguration> _innerObservable;
		private int _numberOfChanges;

		public LimitNumberOfConfigurationChanges(IObservable<MusicMirrorConfiguration> innerObservable, int numberOfChanges)
		{
			if (innerObservable == null) throw new ArgumentNullException(nameof(innerObservable));
			if (numberOfChanges <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfChanges));
			_innerObservable = innerObservable;
			_numberOfChanges = numberOfChanges;
		}

		public IDisposable Subscribe(IObserver<MusicMirrorConfiguration> observer)
		{
			return _innerObservable.Take(_numberOfChanges).Subscribe(observer);
		}
	}
}