namespace FtpContentManager.Src.Constants
{
	public enum ContentType
	{
		Unknown,
		SavedGame		   = 0x00000001,
		DownloadableContent = 0x00000002,
		Publisher		   = 0x00000003,

		Xbox360Title		= 0x00001000,
		IptvPauseBuffer	 = 0x00002000,
		InstalledGame	   = 0x00004000,
		XboxOriginalGame	= 0x00005000,
		GameOnDemand		= 0x00007000,
		AvatarAssetPack	 = 0x00008000,
		AvatarItem		  = 0x00009000,

		Profile			 = 0x00010000,
		GamerPicture		= 0x00020000,
		Theme			   = 0x00030000,
		CacheFile		   = 0x00040000,
		StorageDownload	 = 0x00050000,
		XboxSavedGame	   = 0x00060000,
		XboxDownload		= 0x00070000,
		GameDemo			= 0x00080000,
		Video			   = 0x00090000,
		XboxLiveArcadeGame  = 0x000D0000,
		GamerTitle		  = 0x000A0000,
		TitleUpdate		 = 0x000B0000,
		GameTrailer		 = 0x000C0000,
		XNA				 = 0x000E0000,
		LicenseStore		= 0x000F0000,

		Movie			   = 0x00100000,
		Television		  = 0x00200000,
		MusicVideo		  = 0x00300000,
		GameVideo		   = 0x00400000,
		PodcastVideo		= 0x00500000,
		ViralVideo		  = 0x00600000,
		CommunityGame	   = 0x02000000,
	}

	public enum MediaTypecs : uint
	{
		HardDisk = 0x00000001,
		DvdX2 = 0x00000002,
		DvdCd = 0x00000004,
		Dvd5 = 0x00000008,
		Dvd9 = 0x00000010,
		SystemFlash = 0x00000020,
		MemoryUnit = 0x00000080,
		MassStorageDevice = 0x00000100,
		SmbFileSystem = 0x00000200,
		DirectFromRam = 0x00000400,
		InsecurePackage = 0x01000000,
		SaveGamePackage = 0x02000000,
		OfflineSignedPackage = 0x04000000,
		LiveSignedPackage = 0x08000000,
		XboxPlatformPackage = 0x10000000
	}

	public enum LicenseType
	{
		Unused = 0x0000,
		Unrestricted = 0xFFFF,
		ConsoleProfileLicense = 0x0009,
		WindowsProfileLicense = 0x0003,
		ConsoleLicense = 0xF000,
		MediaFlags = 0xE000,
		KeyVaultPrivileges = 0xD000,
		HyperVisorFlags = 0xC000,
		UserPrivileges = 0xB000
	}

	public enum Magic
	{
		CON = 0x434F4E20,
		LIVE = 0x4C495645,
		PIRS = 0x50495253
	}

	public enum InstallerType
	{
		None = 0,
		SystemUpdate = 0x53555044,
		TitleUpdate = 0x54555044,
		SystemUpdateProgressCache = 0x50245355,
		TitleUpdateProgressCache = 0x50245455,
		TitleContentProgressCache = 0x50245443
	}

	public enum SubscriptionTeir
	{
		NoSubscription = 0,
		Silver = 3,
		Gold = 6,
		FamilyGold = 9
	}

	public enum XboxLiveServiceProvider
	{
		LiveDisabled = 0,
		ProductionNet = 0x50524F44,	// PROD
		PartnerNet = 0x50415254		// PART
	}
}
