using FTPcontentManager.Src.Attributes;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Readers.God
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