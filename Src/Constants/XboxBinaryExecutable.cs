namespace FtpContentManager.Src.Constants
{
	public enum XbeGameRegion : uint
	{
		Japan = 2,
		Manufacturing = 0x80000000,
		NorthAmerica = 1,
		RestOfWorld = 4
	}

	public enum XbeAllowedMedia : uint
	{
		Cd = 8,
		Dongle = 0x100,
		Dvd5Ro = 0x10,
		Dvd5Rw = 0x40,
		Dvd9Ro = 0x20,
		Dvd9Rw = 0x80,
		DvdCd = 4,
		DvdX2 = 2,
		HardDisk = 1,
		MediaBoard = 0x200,
		MediaMask = 0xffffff,
		NonSecureHd = 0x40000000,
		NonSecureMode = 0x80000000
	}

	public enum XbeInitFlags
	{
		DontSetupHardDisk = 4,
		FormatUtilityDrive = 2,
		Limit64Megabytes = 3,
		MountUtilityDrive = 1
	}

	public enum XbeSectionFlags : uint
	{
		Executable = 4,
		HeadPageReadOnly = 0x10,
		InsertedFile = 8,
		Preload = 2,
		TailPageReadOnly = 0x20,
		Writable = 1
	}
}
