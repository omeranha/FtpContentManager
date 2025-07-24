using System;

namespace FtpContentManager.Src.Constants
{
	public enum IsoType
	{
		GamePartitionOnly = 0,
		XGD1 = 0x30600,
		XGD2 = 0x1fb20,
		XGD3 = 0x4100
	}

	[Flags]
	public enum XisoFlags
	{
		Unknown = 0,
		ReadOnly = 1,
		Hidden = 2,
		System = 4,
		Directory = 0x10,
		Archive = 0x20,
		Device = 0x40,
		Normal = 0x80
	}

	public enum VolumeDescriptorType
	{
		STFS = 0,
		SVOD = 1
	}

	public enum BlockStatus
	{
		Unallocated = 0,
		PreviouslyAllocated = 0x40,
		Allocated = 0x80,
		NewlyAllocated = 0xC0
	}

	[Flags]
	public enum FileEntryFlags
	{
		IsDirectory = 0x80,
		BlocksAreConsecutive = 0x40,
		Default = 0
	}

	public enum StringReadOptions
	{
		Default,
		AutoTrim,
		NullTerminated,
		ID
	}
}
