using System;

namespace FTPcontentManager.Src.Constants
{
	/// <summary>
	/// Xbox executable (XEX) compression types
	/// </summary>
	public enum XexCompressionType
	{
		NotCompressed = 1,
		Compressed = 2,
		DeltaCompressed = 3
	}

	/// <summary>
	/// Xbox executable (XEX) encryption types
	/// </summary>
	public enum XexEncryptionType
	{
		NotEncrypted = 0,
		Encrypted = 1
	}

	/// <summary>
	/// Xbox executable (XEX) flags
	/// </summary>
	[Flags]
	public enum XexFlags
	{
		None = 0,
		TitleModule = 1 << 0,
		ExportsToTitle = 1 << 1,
		SystemDebugger = 1 << 2,
		DllModule = 1 << 3,
		ModulePatch = 1 << 4,
		PatchFull = 1 << 5,
		PatchDelta = 1 << 6,
		UserMode = 1 << 7
	}

	/// <summary>
	/// Xbox executable (XEX) optional header IDs
	/// </summary>
	public enum XexOptionalHeaderId : uint
	{
		ResourceInfo = 0x2FF,
		DecompressionInformation = 0x3FF,
		BaseReference = 0x405,
		DeltaPatchDescriptor = 0x5FF,
		BoundingPath = 0x80FF,
		DeviceId = 0x8105,
		OriginalBaseAddress = 0x10001,
		EntryPoint = 0x10100,
		ImageBaseAddress = 0x10201,
		ImportLibraries = 0x103FF,
		ChecksumTimestamp = 0x18002,
		EnabledForCallcap = 0x18102,
		EnabledForFastcap = 0x18200,
		OriginalPEName = 0x183FF,
		StaticLibraries = 0x200FF,
		TlsInfo = 0x20104,
		DefaultStackSize = 0x20200,
		DefaultFilesystemCacheSize = 0x20301,
		DefaultHeapSize = 0x20401,
		PageHeapSizeandFlags = 0x28002,
		SystemFlags = 0x30000,
		ExecutionId = 0x40006,
		ServiceIdList = 0x401FF,
		TitleWorkspaceSize = 0x40201,
		GameRatings = 0x40310,
		LanKey = 0x40404,
		Xbox360Logo = 0x405FF,
		MultidiscMediaIds = 0x406FF,
		AlternateTitleIds = 0x407FF,
		AdditionalTitleMemory = 0x40801,
		ExportsByName = 0xE10402,
	}

	/// <summary>
	/// Xbox executable (XEX) optional header types
	/// </summary>
	public enum XexOptionalHeaderType
	{
		SimpleData,
		DataSize,
		EntrySize
	}

	/// <summary>
	/// Xbox binary executable (XBE) allowed media types
	/// </summary>
	[Flags]
	public enum XbeAllowedMedia : uint
	{
		None = 0,
		HardDisk = 1 << 0,
		DvdX2 = 1 << 1,
		DvdCd = 1 << 2,
		Cd = 1 << 3,
		DvdX5 = 1 << 4,
		DvdX9 = 1 << 5,
		UnusedBit6 = 1 << 6,
		UnusedBit7 = 1 << 7,
		DongleDisk = 1 << 8,
		MediaBoard = 1 << 9,
		NonSecureHardDisk = 1 << 10,
		NonSecureMode = 1 << 11,
		Media = 1 << 12,
		SecureMode = 1 << 13
	}

	/// <summary>
	/// Xbox binary executable (XBE) game regions
	/// </summary>
	[Flags]
	public enum XbeGameRegion : uint
	{
		None = 0,
		NorthAmerica = 1 << 0,
		Japan = 1 << 1,
		RestOfWorld = 1 << 2,
		Manufacturing = 0xFFFFFFFF
	}

	/// <summary>
	/// Xbox binary executable (XBE) initialization flags
	/// </summary>
	[Flags]
	public enum XbeInitFlags : uint
	{
		None = 0,
		MountUtilityDrive = 1 << 0,
		FormatUtilityDrive = 1 << 1,
		Limit64Mb = 1 << 2,
		DontSetupHarddisk = 1 << 3
	}

	/// <summary>
	/// Xbox binary executable (XBE) section flags
	/// </summary>
	[Flags]
	public enum XbeSectionFlags : uint
	{
		None = 0,
		Writable = 1 << 0,
		Preload = 1 << 1,
		Executable = 1 << 2,
		InsertedFile = 1 << 3,
		HeadPageRO = 1 << 4,
		TailPageRO = 1 << 5
	}

	/// <summary>
	/// XPR format enumeration
	/// </summary>
	public enum XprFormat
	{
		Argb = 6,
		Dxt1 = 12,
		None = 0
	}
}
