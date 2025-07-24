using System;
using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Constants;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Xbe
{
	public class XbeCertificate : BinaryModelBase
	{
		[BinaryData(EndianType.LittleEndian)]
		public virtual uint Size { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual uint TimeDate { get; set; }

		[BinaryData(4, StringReadOptions.ID)]
		public virtual string TitleId { get; set; }

		[BinaryData(80, "unicode", StringReadOptions.AutoTrim)]
		public virtual string TitleName { get; set; }

		[BinaryData(0x40)]
		public virtual byte[] AltTitleIds { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual XbeAllowedMedia AllowedMedia { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual XbeGameRegion GameRegion { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual uint GameRatings { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual uint DiscNumber { get; set; }

		[BinaryData(4)]
		public virtual Version Version { get; set; }

		[BinaryData(0x10)]
		public virtual byte[] LanKey { get; set; }

		[BinaryData(0x10)]
		public virtual byte[] SignatureKey { get; set; }

		[BinaryData(0x100)]
		public virtual byte[] AltSignatureKeys { get; set; }

		public XbeCertificate(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}