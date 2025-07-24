using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Gpd
{
	public class XdbfFreeSpaceEntry : BinaryModelBase
	{
		[BinaryData]
		public virtual int AddressSpecifier { get; set; }

		[BinaryData]
		public virtual int Length { get; set; }

		public XdbfFreeSpaceEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}