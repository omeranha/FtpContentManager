using FtpContentManager.Src.Constants;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Iso
{
	public class XisoFileEntry : INamed
	{
		public string Name { get; set; }
		public long Offset { get; set; }
		public string Path { get; set; }
		public uint? Size { get; set; }
		public XisoFlags Flags { get; set; }
		public XisoTableData TableData { get; set; }

		public bool IsDirectory
		{
			get { return Flags.HasFlag(XisoFlags.Directory); }
		}
	}
}