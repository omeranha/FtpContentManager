using System;

namespace FTPcontentManager.Src.Constants
{
	/// <summary>
	/// File entry flags for file system operations
	/// </summary>
	[Flags]
	public enum FileEntryFlags
	{
		/// <summary>
		/// Default flags (no special flags)
		/// </summary>
		Default = 0,
		
		/// <summary>
		/// Blocks are stored consecutively
		/// </summary>
		BlocksAreConsecutive = 0x40,
		
		/// <summary>
		/// Entry represents a directory
		/// </summary>
		IsDirectory = 0x80
	}

	/// <summary>
	/// Block status enumeration
	/// </summary>
	public enum BlockStatus
	{
		Unallocated = 0,
		PreviouslyAllocated = 0x40,
		Allocated = 0x80,
		NewlyAllocated = 0xC0
	}

	/// <summary>
	/// Entry type enumeration
	/// </summary>
	public enum EntryType
	{
		Unknown = 0,
		Achievement = 1,
		Image = 2,
		Setting = 3,
		Title = 4,
		String = 5,
		AvatarAward = 6,
		MysteriousSeven = 7
	}

	/// <summary>
	/// Volume descriptor type for ISO files
	/// </summary>
	public enum VolumeDescriptorType
	{
		Boot = 0,
		Primary = 1,
		Supplementary = 2,
		VolumePartition = 3,
		Terminator = 255
	}

	/// <summary>
	/// ISO type enumeration
	/// </summary>
	public enum IsoType
	{
		Iso9660,
		Xiso,
		Udf
	}

	/// <summary>
	/// Transfer flags for file operations
	/// </summary>
	[Flags]
	public enum TransferFlags
	{
		None = 0,
		Silent = 1 << 0,
		NoProgressCallback = 1 << 1,
		SkipExisting = 1 << 2,
		MoveToTrash = 1 << 3
	}

	/// <summary>
	/// XISO flags for Xbox ISO files
	/// </summary>
	[Flags]
	public enum XisoFlags
	{
		None = 0,
		HasSecuritySector = 1 << 0,
		MediaCheck = 1 << 1,
		VideoCheck = 1 << 2
	}
}
