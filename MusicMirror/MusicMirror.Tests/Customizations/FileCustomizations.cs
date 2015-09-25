using System;
using System.Collections.Generic;
using System.IO;
using MusicMirror.Tests.Autofixture;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using Xunit.Extensions;
using Moq;
using Hanno.Testing.Autofixture;

namespace MusicMirror.Tests.Customizations
{
	#region Customization
	/// <summary>
	/// This customization register allow the <c>IFixture</c> to create anonymous object reflecting the 
	/// possible values in the synchronization process, such as :
	/// - TargetFilePath with .mp3 extensions
	/// - SourceFilePath with .flac extensions
	/// - Configuration object with meaningful folder paths
	/// - Enumerable of <c>AudioFormat</c>
	/// </summary>
	public sealed class FileCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customize<string>(c => c.FromFactory(() => GuidGenerator.Create().ToString().Substring(0, 8)));
			fixture.Register(() => new MusicMirrorConfiguration(new DirectoryInfo(@"C:\Music"), new DirectoryInfo(@"D:\OneDrive\Music"), NonTranscodingFilesBehavior.Copy));
			fixture.Freeze<MusicMirrorConfiguration>();
			fixture.Register(() => CreateFilePath(SourceFilePath.CreateFromPathWithoutExtension, fixture));
			fixture.Register(() => CreateFilePath(TargetFilePath.CreateFromPathWithoutExtension, fixture));
			fixture.Register<IEnumerable<AudioFormat>>(() => new[] { AudioFormat.MP3, AudioFormat.Flac });
			fixture.Register<Stream>(() => fixture.Create<MemoryStream>());										
		}

		private T CreateFilePath<T>(Func<MusicMirrorConfiguration, string[], T> createFilePath, IFixture fixture) where T : FilePathBase
		{
			var folderParts = fixture.Create<string[]>();
			var config = fixture.Create<MusicMirrorConfiguration>();
			return createFilePath(config, folderParts);
		}
	}

	public sealed class FileCompositeCustomization : CompositeCustomization
	{
		public FileCompositeCustomization()
			: base(new Hanno.Testing.Autofixture.HannoCustomization(), new FileCustomization(), new RxCustomization())
		{
		}
	}

	public sealed class FileAutoDataAttribute : AutoDataAttribute
	{
		public FileAutoDataAttribute()
			: base(new Fixture().Customize(new FileCompositeCustomization()))
		{
		}
	}

	public sealed class FileInlineAutoDataAttribute : CompositeDataAttribute
	{
		public FileInlineAutoDataAttribute(params object[] values)
			: base(new InlineDataAttribute(values), new FileAutoDataAttribute())
		{
		}
	}
	#endregion
}