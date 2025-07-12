using System;
using FTPcontentManager.Src.Attributes;
using FTPcontentManager.Src.Constants;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Readers.Stfs.Data
{
	public class VideoMediaInfo : BinaryModelBase, IMediaInfo
	{
		[BinaryData(0x10)]
		public byte[] SeriesId { get; set; }

		[BinaryData(0x10)]
		public byte[] SeasonId { get; set; }

		[BinaryData]
		public ushort SeasonNumber { get; set; }

		[BinaryData]
		public ushort EpisodeNumber { get; set; }

		public VideoMediaInfo(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}