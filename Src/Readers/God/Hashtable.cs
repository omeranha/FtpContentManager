using FTPcontentManager.Src.Attributes;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Readers.God
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