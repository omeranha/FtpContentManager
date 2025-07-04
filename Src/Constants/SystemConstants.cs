using System;

namespace FTPcontentManager.Src.Constants
{
	/// <summary>
	/// Endian type enumeration
	/// </summary>
	public enum EndianType
	{
		BigEndian = 0,
		LittleEndian = 1,
		SwapBytesBy4 = 4,
		SwapBytesBy8 = 8
	}

	/// <summary>
	/// String reading options
	/// </summary>
	[Flags]
	public enum StringReadOptions
	{
		Default,
		AutoTrim,
		NullTerminated,
		ID
	}

	/// <summary>
	/// USB device change enumeration
	/// </summary>
	public enum UsbDeviceChange
	{
		Added,
		Removed,
		Changed
	}

	/// <summary>
	/// Reserved flags enumeration
	/// </summary>
	[Flags]
	public enum ReservedFlags
	{
		None = 0,
		Reserved1 = 1 << 0,
		Reserved2 = 1 << 1,
		Reserved3 = 1 << 2,
		Reserved4 = 1 << 3
	}

	/// <summary>
	/// Skeleton version enumeration
	/// </summary>
	public enum SkeletonVersion
	{
		Version1,
		Version2,
		Version3
	}
}
