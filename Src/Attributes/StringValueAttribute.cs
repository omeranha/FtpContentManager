using System;

namespace FtpContentManager.Src.Attributes {
	[AttributeUsage(AttributeTargets.All)]
	public class StringValueAttribute(string value) : Attribute {
		public string Value { get; private set; } = value;
	}
}
