using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.God
{
	public class HashEntry : BinaryModelBase
	{
		[BinaryData(0x14)]
		public virtual byte[] BlockHash { get; set; }

		public HashEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}