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
			string witnessFileName,
			TagTypes tagTypes)
		{
			_witnessFilename = Guard.ForNullOrWhiteSpace(witnessFileName, nameof(witnessFileName));
			_tagTypes = tagTypes;
		}

		public Task<Tag> ReadTags(CancellationToken ct, Stream stream)
		{
			Guard.ForNull(stream, nameof(stream));
			return Task.Run(() =>
			{
				using (var fileAbstraction = new AlwaysOpenStreamFileAbstraction(_witnessFilename, stream))
				using (var file = TagLib.File.Create(fileAbstraction))
				{
					return file.GetTag(_tagTypes, false);
				}
			});
		}

		public Task WriteTags(CancellationToken ct, Stream stream, Tag tags)
		{
			Guard.ForNull(stream, nameof(stream));
			Guard.ForNull(tags, nameof(tags));
			return Task.Run(() =>
			{
				using (var fileAbstraction = new AlwaysOpenStreamFileAbstraction(_witnessFilename, stream))
				using (var file = TagLib.File.Create(fileAbstraction))
                {
					var fileTags = file.GetTag(_tagTypes, true);
					fileTags.Clear();
					tags.CopyTo(fileTags, true);
					fileTags.Pictures = tags.Pictures;
					file.Save();
				}
			});
		}
	}

	public class FlacTagLibReaderWriter : TagLibReaderWriterBase
	{
		public FlacTagLibReaderWriter() : base("file.flac", TagTypes.FlacMetadata)
		{
		}
	}

	public class MP3TagLibReaderWriter : TagLibReaderWriterBase
	{
		public MP3TagLibReaderWriter() : base("file.mp3", TagTypes.Id3v2)
		{
		}
	}
}
