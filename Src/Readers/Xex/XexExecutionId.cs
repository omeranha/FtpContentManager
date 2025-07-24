using System;
using FtpContentManager.Src.Attributes;
using FtpContentManager.Src.Constants;
using FtpContentManager.Src.Extensions;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Xex
{
	[XexHeader(XexOptionalHeaderId.ExecutionId)]
	public class XexExecutionId : BinaryModelBase
	{
		[BinaryData(4, StringReadOptions.ID)]
		public virtual string MediaId { get; set; }

		[BinaryData(4)]
		public virtual Version Version { get; set; }

		[BinaryData(4)]
		public virtual Version BaseVersion { get; set; }

		[BinaryData(4, StringReadOptions.ID)]
		public virtual string TitleId { get; set; }

		[BinaryData]
		public virtual byte Platform { get; set; }

		[BinaryData]
		public virtual byte ExecutableType { get; set; }

		[BinaryData]
		public virtual byte DiscNumber { get; set; }

		[BinaryData]
		public virtual byte DiscCount { get; set; }

		public XexExecutionId(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}