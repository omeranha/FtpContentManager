using System;

namespace FTPcontentManager.Src.Constants
{
	public enum XexCompressionType
	{
		NotCompressed = 1,
		Compressed = 2,
		DeltaCompressed = 3
	}

	public enum XexEncryptionType
	{
		NotEncrypted = 0,
		Encrypted = 1
	}

	[Flags]
	public enum XexFlags
	{
		Unknown = 0,
		TitleModule = 1,
		ExportsToTitle = 2,
		SystemDebugger = 4,
		DllModule = 8,
		ModulePatch = 0x10,
		PatchFull = 0x20,
		PatchDelta = 0x40,
		UserCode = 0x80
	}

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

	public enum XexOptionalHeaderType
	{
		SimpleData,
		DataSize,
		EntrySize
	}
}
