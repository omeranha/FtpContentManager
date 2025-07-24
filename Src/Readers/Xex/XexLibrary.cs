using System;
using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Constants;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Xex
{
	[XexHeader(XexOptionalHeaderId.StaticLibraries)]
	public class XexLibrary : BinaryModelBase
	{

		[BinaryData(8, "ascii", StringReadOptions.AutoTrim)]
		public virtual string Name { get; set; }

		[BinaryData(8)]
		public virtual Version Version { get; set; }

		public XexLibrary(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}