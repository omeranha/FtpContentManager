using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Constants;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Stfs.Data
{
	public class StfsVolumeDescriptor : BinaryModelBase, IVolumeDescriptor
	{
		[BinaryData(0x1)]
		public virtual int Size { get; set; }

		[BinaryData(0x1)]
		public virtual int Reserved { get; set; }

		[BinaryData(0x1)]
		public virtual int BlockSeparation { get; set; }

		[BinaryData(0x2, EndianType.LittleEndian)]
		public virtual ushort FileTableBlockCount { get; set; }

		[BinaryData(0x3, EndianType.LittleEndian)]
		public virtual int FileTableBlockNum { get; set; }

		[BinaryData(0x14)]
		public virtual byte[] TopHashTableHash { get; set; }

		[BinaryData]
		public virtual int AllocatedBlockCount { get; set; }

		[BinaryData]
		public virtual int UnallocatedBlockCount { get; set; }

		public StfsVolumeDescriptor(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}