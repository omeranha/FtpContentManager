using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Gpd.Entries
{
	public class ImageEntry : EntryBase
	{
		public byte[] ImageData 
		{
			get { return AllBytes; }
			set { Binary.WriteBytes(StartOffset, value, 0, value.Length); }
		}

		public ImageEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}