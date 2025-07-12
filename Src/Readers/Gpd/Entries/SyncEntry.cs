using FTPcontentManager.Src.Attributes;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Readers.Gpd.Entries
{
	public class SyncEntry : BinaryModelBase
	{
		public const int Size = 16;

		[BinaryData]
		public virtual ulong EntryId { get; set; }
		[BinaryData]
		public virtual ulong SyncId { get; set; }

		public SyncEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}