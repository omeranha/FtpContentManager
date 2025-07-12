using FTPcontentManager.Src.Attributes;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Readers.Stfs.Data
{
	public class PackageSignature : BinaryModelBase, IPackageSignature
	{
		[BinaryData(0x100)]
		public virtual byte[] Signature { get; set; }

		public PackageSignature(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}