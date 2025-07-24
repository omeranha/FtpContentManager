using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Xex
{
	public class XexRawBaseFileBlock : BinaryModelBase
	{
		[BinaryData]
		public virtual int DataSize { get; set; }

		[BinaryData]
		public virtual int ZeroSize { get; set; }

		public XexRawBaseFileBlock(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}