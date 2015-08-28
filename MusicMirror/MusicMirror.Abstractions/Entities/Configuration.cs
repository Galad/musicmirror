using System;
using System.IO;

namespace MusicMirror.Entities
{
	public enum NonTranscodingFilesBehavior
	{
		Copy,
		SymbolicLink,
		Ignore
	}

	public class MusicMirrorConfiguration : IEquatable<MusicMirrorConfiguration>
	{
		#region Equality
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((MusicMirrorConfiguration)obj);
		}

		public bool Equals(MusicMirrorConfiguration other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(_sourcePath.FullName, other._sourcePath.FullName) && string.Equals(_targetPath.FullName, other._targetPath.FullName);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((_sourcePath != null ? _sourcePath.FullName.GetHashCode() : 0) * 397) ^ (_targetPath != null ? _targetPath.FullName.GetHashCode() : 0);
			}
		}

		public static bool operator ==(MusicMirrorConfiguration c1, MusicMirrorConfiguration c2)
		{
			if (ReferenceEquals(null, c1) && ReferenceEquals(null, c2)) return true;
			if (ReferenceEquals(null, c1)) return false;
			return c1.Equals(c2);
		}

		public static bool operator !=(MusicMirrorConfiguration c1, MusicMirrorConfiguration c2)
		{
			return !(c1 == c2);
		}

		#endregion

		private readonly DirectoryInfo _sourcePath;
		private readonly DirectoryInfo _targetPath;
		private readonly NonTranscodingFilesBehavior _nonTranscodingFilesBehavior;

		public MusicMirrorConfiguration(DirectoryInfo sourcePath, DirectoryInfo targetPath, NonTranscodingFilesBehavior nonTranscodingFilesBehavior)
		{
			_sourcePath = sourcePath;
			_targetPath = targetPath;
			_nonTranscodingFilesBehavior = nonTranscodingFilesBehavior;
		}

		public DirectoryInfo SourcePath
		{
			get { return _sourcePath; }
		}

		public DirectoryInfo TargetPath
		{
			get { return _targetPath; }
		}

		public bool HasValidDirectories
		{
			get { return SourcePath != null && SourcePath.Exists && TargetPath != null && TargetPath.Exists; }
		}
			
		public NonTranscodingFilesBehavior NonTranscodingFilesBehavior
		{
			get
			{
				return _nonTranscodingFilesBehavior;
			}
		}
	}
}