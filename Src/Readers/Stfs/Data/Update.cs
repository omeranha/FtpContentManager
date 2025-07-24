using System;
using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Stfs.Data
{
	public class Update : BinaryModelBase, IInstallerInformation
	{
		[BinaryData(4)]
		public virtual Version BaseVersion { get; set; }

		[BinaryData(4)]
		public virtual Version Version { get; set; }

		public Update(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}