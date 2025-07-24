using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.God
{
	public class HashTable : BinaryModelBase
	{
		[BinaryData(0xCC)]
		public virtual HashEntry[] Entries { get; set; }

		public HashTable(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}