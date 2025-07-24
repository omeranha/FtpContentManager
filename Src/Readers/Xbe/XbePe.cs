using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Constants;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Xbe
{
	public class XbePe : BinaryModelBase
	{
		[BinaryData(EndianType.LittleEndian)]
		public virtual uint StackCommit { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual uint HeapReserve { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual uint HeapCommit { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual uint BaseAddress { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual uint SizeOfImage { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual uint Checksum { get; set; }

		public XbePe(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}