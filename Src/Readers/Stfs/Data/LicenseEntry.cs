using System;
using System.IO;
using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Constants;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Stfs.Data
{
	public class LicenseEntry : BinaryModelBase
	{
		[BinaryData]
		public virtual ulong Data { get; set; }

		public LicenseType Type
		{
			get
			{
				var type = (int)(Data >> 48);
				if (!Enum.IsDefined(typeof(LicenseType), type))
					throw new InvalidDataException("STFS: Invalid license type " + type);
				return (LicenseType) type;
			}
		}

		[BinaryData]
		public virtual uint Bits { get; set; }

		[BinaryData]
		public virtual uint Flags { get; set; } //TODO?

		public LicenseEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}