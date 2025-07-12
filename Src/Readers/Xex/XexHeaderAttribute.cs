using System;
using FTPcontentManager.Src.Constants;

namespace FTPcontentManager.Src.Readers.Xex
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