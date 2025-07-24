using System;

namespace FtpContentManager.Src.Readers.Stfs.Events
{
	public class ContentParsedEventArgs : EventArgs
	{
		public object Content { get; private set; }

		public ContentParsedEventArgs(object content)
		{
			Content = content;
		}
	}
}