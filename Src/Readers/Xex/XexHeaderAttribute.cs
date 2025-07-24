using System;
using FtpContentManager.Src.Constants;

namespace FtpContentManager.Src.Readers.Xex
{
	public class XexHeaderAttribute : Attribute
	{
		public XexOptionalHeaderId Id { get; private set; }

		public XexHeaderAttribute(XexOptionalHeaderId id)
		{
			Id = id;
		}
	}
}