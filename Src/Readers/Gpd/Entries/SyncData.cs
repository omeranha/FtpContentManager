using System;
using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Gpd.Entries
{
	public class SyncData : EntryBase
	{
		[BinaryData]
		public virtual ulong NextSyncId { get; set; }

		[BinaryData]
		public virtual ulong LastSyncId { get; set; }

		[BinaryData]
		public virtual DateTime LastSyncedTime { get; set; }

		public SyncData(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}