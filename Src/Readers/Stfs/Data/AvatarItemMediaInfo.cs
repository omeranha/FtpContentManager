using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Constants;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Stfs.Data
{
	public class AvatarItemMediaInfo : BinaryModelBase, IMediaInfo
	{
		[BinaryData(EndianType.LittleEndian)]
		public virtual AssetSubcategory Subcategory { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual int Colorizable { get; set; }

		[BinaryData(0x10)]
		public virtual byte[] GUID { get; set; }

		[BinaryData(0x1)]
		public virtual SkeletonVersion SkeletonVersion { get; set; }

		public AvatarItemMediaInfo(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}