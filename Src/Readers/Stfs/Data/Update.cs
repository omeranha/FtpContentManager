using System;
using FTPcontentManager.Src.Attributes;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Readers.Stfs.Data
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