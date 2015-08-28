using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror
{
	public class AudioFormat : IEquatable<AudioFormat>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "AudioFormat is immutable")]
		public static readonly AudioFormat MP3 = new AudioFormat("MP3", "MPEG-1/2 Audio Layer 3", ".mp3", LossKind.Lossy);
		public static readonly AudioFormat FLAC = new AudioFormat("FLAC", "Free Lossless Audio Codec", ".flac", LossKind.Lossy);
		public static readonly IEnumerable<AudioFormat> KnownFormats = new[] { MP3, FLAC }.AsEnumerable();
		
		private readonly IEnumerable<string> _allExtensions;
		private readonly string _defaultExtension;
		private readonly LossKind _lossKind;
		private readonly string _shortName;
		private readonly string _fullName;

		public AudioFormat(
			string shortName,
			string fullName,
			string extension,
			LossKind lossKind,
			params string[] aliasExtensions)
			: this(shortName, fullName, extension, lossKind, (IEnumerable<string>)aliasExtensions)
		{ }

		public AudioFormat(
			string shortName,
			string fullName,
			string extension,
			LossKind lossKind,
			IEnumerable<string> aliasExtensions)
		{
			if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException(nameof(extension));
			if (string.IsNullOrEmpty(shortName)) throw new ArgumentNullException(nameof(shortName));
			if (string.IsNullOrEmpty(fullName)) throw new ArgumentNullException(nameof(fullName));
			if (aliasExtensions == null) throw new ArgumentNullException(nameof(aliasExtensions));
			_shortName = shortName;
			_fullName = fullName;
			_defaultExtension = extension;
			_lossKind = lossKind;
			_allExtensions = Enumerable.Concat(new[] { extension }, aliasExtensions);
		}

		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			return Equals((AudioFormat)obj);
		}

		public bool Equals(AudioFormat other)
		{
			if (other == null) return false;
			if (ReferenceEquals(this, other)) return true;
			return ShortName.Equals(other.ShortName) &&
				   FullName.Equals(other.FullName) &&
				   DefaultExtension.Equals(other.DefaultExtension) &&
				   LossKind.Equals(other.LossKind) &&
				   AllExtensions.SequenceEqual(other.AllExtensions);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return AllExtensions.Aggregate(
						ShortName.GetHashCode() ^
						FullName.GetHashCode() ^
						DefaultExtension.GetHashCode() ^
						LossKind.GetHashCode(),
						(hashCode, s) => hashCode ^ s.GetHashCode());
			}
		}

		public override string ToString()
		{
			return ShortName;
		}

		public IEnumerable<string> AllExtensions
		{
			get
			{
				return _allExtensions;
			}
		}

		public string DefaultExtension
		{
			get
			{
				return _defaultExtension;
			}
		}

		public LossKind LossKind
		{
			get
			{
				return _lossKind;
			}
		}

		public string ShortName
		{
			get
			{
				return _shortName;
			}
		}

		public string FullName
		{
			get
			{
				return _fullName;
			}
		}

		public bool SupportExtension(string extension)
		{
			return _allExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
		}
	}

	public enum LossKind
	{
		Lossy,
		Lossless
	}
}
