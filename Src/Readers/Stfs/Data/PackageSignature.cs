using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Stfs.Data
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