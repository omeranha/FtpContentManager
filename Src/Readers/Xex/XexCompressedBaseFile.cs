using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Xex
{
	public class XexCompressedBaseFile : BinaryModelBase
	{
		[BinaryData]
		public virtual int CompressionWindow { get; set; }

		[BinaryData]
		public virtual int DataSize { get; set; }

		[BinaryData(0x14)]
		public virtual byte[] Hash { get; set; }

		public XexCompressedBaseFile(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}