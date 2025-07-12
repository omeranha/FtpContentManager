using FTPcontentManager.Src.Constants;

namespace FTPcontentManager.Src.Readers.Iso
{
	public class IsoTypeDescriptor
	{
		public IsoType Type { get; private set; }

		public int RootOffset
		{
			get { return (int)Type; }
		}

		public IsoTypeDescriptor(IsoType type)
		{
			Type = type;
		}

		public long SectorToOffset(uint sector)
		{
			//TODO: why multiply the rootoffset with the sector size?
			return (RootOffset + sector) * VolumeDescriptor.SectorSize;
		}

	}
}