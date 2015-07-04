using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TagLib;

namespace MusicMirror.Transcoding
{
	public class TagLibReaderWriterBase : IAudioTagReader, IAudioTagWriter
	{
		private readonly TagTypes _tagTypes;
		private readonly string _witnessFilename;

		public TagLibReaderWriterBase(
			string witnessFilename,
			TagTypes tagTypes)
		{
			_witnessFilename = Guard.ForNullOrWhiteSpace(witnessFilename, nameof(witnessFilename));
			_tagTypes = tagTypes;
		}
		
		public Task<Tag> ReadTags(CancellationToken ct, Stream stream)
		{
			Guard.ForNull(stream, nameof(stream));
			return Task.Run(() =>
			{
				using (var fileAbstraction = new AlwaysOpenStreamFileAbstraction(_witnessFilename, stream))
				{
					var file = TagLib.File.Create(fileAbstraction);
					return file.GetTag(_tagTypes, false);
				}
			});
		}

		public Task WriteTags(CancellationToken ct, Stream stream, Tag sourceTags)
		{
			Guard.ForNull(stream, nameof(stream));
			Guard.ForNull(sourceTags, nameof(sourceTags));
			return Task.Run(() =>
			{
				using (var fileAbstraction = new AlwaysOpenStreamFileAbstraction(_witnessFilename, stream))
				{
					var file = TagLib.File.Create(fileAbstraction);
					var tags = file.GetTag(_tagTypes, true);
					tags.Clear();
					sourceTags.CopyTo(tags, true);
					tags.Pictures = sourceTags.Pictures;
					file.Save();
				}
			});
		}
	}

	public class FlacTagLibReaderWriter : TagLibReaderWriterBase
	{
		public FlacTagLibReaderWriter() :base("file.flac", TagTypes.FlacMetadata)
		{
		}
	}

	public class Mp3TagLibReaderWriter : TagLibReaderWriterBase
	{
		public Mp3TagLibReaderWriter() : base("file.mp3", TagTypes.Id3v2)
		{
		}
	}
}
