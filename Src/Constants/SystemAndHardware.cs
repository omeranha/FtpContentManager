namespace FTPcontentManager.Src.Constants
{
	public enum ConsoleType
	{
		DevKit = 1,
		Retail = 2
	}

	public enum ConsoleTypeFlags
	{
		None,
		TestKit = 4,
		RecoveryGenerated = 8
	}

	public enum Button
	{
		Null,
		DpadUp,
		DpadDown,
		DpadLeft,
		DpadRight,
		X,
		Y,
		A,
		B,
		LeftTrigger,
		RightTrigger,
		LeftBumper,
		RightBumper
	}

	public enum UsbDeviceChange
	{
		Inserted = 0x8000,
		Removed = 0x8004
	}

	public enum EndianType
	{
		BigEndian = 0,
		LittleEndian = 1,
		SwapBytesBy4 = 4,
		SwapBytesBy8 = 8
	}

	public enum XprFormat : byte
	{
		Argb = 6,
		Dxt1 = 12,
		None = 0
	}
}
