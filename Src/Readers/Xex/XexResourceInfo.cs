using FTPcontentManager.Src.Attributes;
using FTPcontentManager.Src.Constants;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Readers.Xex
{
	[XexHeader(XexOptionalHeaderId.ResourceInfo)]
	public class XexResourceInfo : BinaryModelBase
	{
		[BinaryData(8, "ascii", StringReadOptions.AutoTrim)]
		public virtual string Name { get; set; }

		[BinaryData]
		public virtual uint Offset { get; set; }

		[BinaryData]
		public virtual uint Size { get; set; }

		public XexResourceInfo(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}