using System;

namespace FTPcontentManager.Src.Constants
{
	public static class Offsets
	{
		public static readonly uint[] DataBlocksPerHashTreeLevel = new uint[] { 1, 0xAA, 0x70E4 };
	}

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

	public enum SettingEntryType
	{
		Context,
		Int32,
		Int64,
		Double,
		UnicodeString,
		Float,
		Binary,
		DateTime,
		Null = 0xFF
	}

	public enum SettingId
	{
		WebConnectionSpeed = 0x1004200b,
		WebEmailFormat = 0x10042000,
		WebFavoriteGame = 0x10042004,
		WebFavoriteGame1 = 0x10042005,
		WebFavoriteGame2 = 0x10042006,
		WebFavoriteGame3 = 0x10042007,
		WebFavoriteGame4 = 0x10042008,
		WebFavoriteGame5 = 0x10042009,
		WebFavoriteGenre = 0x10042003,
		WebFlags = 0x10042001,
		WebFlash = 0x1004200c,
		WebPlatformsOwned = 0x1004200a,
		WebSpam = 0x10042002,
		WebVideoPreference = 0x1004200d,
		CruxBgLargePublic = 0x406403fe,
		CruxBgSmallPublic = 0x406403fd,
		CruxBio = 0x43e803fa,
		CruxBkgdImage = 0x100403f3,
		CruxLastChangeTime = 0x700803f4,
		CruxMediaMotto = 0x410003f6,
		CruxMediaPicture = 0x406403e8,
		CruxMediaStyle1 = 0x100403ea,
		CruxMediaStyle2 = 0x100403eb,
		CruxMediaStyle3 = 0x100403ec,
		CruxOfflineId = 0x603403f2,
		CruxTopAlbum1 = 0x100403ed,
		CruxTopAlbum2 = 0x100403ee,
		CruxTopAlbum3 = 0x100403ef,
		CruxTopAlbum4 = 0x100403f0,
		CruxTopAlbum5 = 0x100403f1,
		CruxTopMediaid1 = 0x601003f7,
		CruxTopMediaid2 = 0x601003f8,
		CruxTopMediaid3 = 0x601003f9,
		CruxTopMusic = 0x60a803f5,
		FriendsappShowBuddies = 0x1004003e,
		GamerActionAutoAim = 0x10040022,
		GamerActionAutoCenter = 0x10040023,
		GamerActionMovementControl = 0x10040024,
		GamerControlSensitivity = 0x10040018,
		GamerDifficulty = 0x10040015,
		GamerPreferredColorFirst = 0x1004001d,
		GamerPreferredColorSecond = 0x1004001e,
		GamerPresenceUserState = 0x10040007,
		GamerRaceAcceleratorControl = 0x10040029,
		GamerRaceBrakeControl = 0x10040028,
		GamerRaceCameraLocation = 0x10040027,
		GamerRaceTransmission = 0x10040026,
		GamerTier = 0x1004003a,
		GamerType = 0x10040001,
		GamerYaxisInversion = 0x10040002,
		GamercardAchievementsEarned = 0x10040013,
		GamercardAvatarInfo1 = 0x63e80044,
		GamercardAvatarInfo2 = 0x63e80045,
		GamercardCred = 0x10040006,
		GamercardHasVision = 0x10040008,
		GamercardMotto = 0x402c0011,
		GamercardPartyInfo = 0x60800046,
		GamercardPersonalPicture = 0x40640010,
		GamercardPictureKey = 0x4064000f,
		GamercardRegion = 0x10040005,
		GamercardRep = 0x5004000b,
		GamercardServiceTypeFlags = 0x1004003f,
		GamercardTitleAchievementsEarned = 0x10040039,
		GamercardTitleCredEarned = 0x10040038,
		GamercardTitlesPlayed = 0x10040012,
		GamercardUserBio = 0x43e80043,
		GamercardUserLocation = 0x40520041,
		GamercardUserName = 0x41040040,
		GamercardUserUrl = 0x41900042,
		GamercardZone = 0x10040004,
		MessengerAutoSignin = 0x1004003c,
		MessengerSignupState = 0x1004003b,
		OptionControllerVibration = 0x10040003,
		OptionVoiceMuted = 0x1004000c,
		OptionVoiceThruSpeakers = 0x1004000d,
		OptionVoiceVolume = 0x1004000e,
		Permissions = 0x10040000,
		SaveWindowsLivePassword = 0x1004003d,
		TitleSpecific1 = 0x63e83fff,
		TitleSpecific2 = 0x63e83ffe,
		TitleSpecific3 = 0x63e83ffd,
		LastOnLive = 0x7008004F,
		YearsOnLive = 0x10040047,
		TitleInformation = 0x8000,
		AvatarImage = 0x8007
	}

	public enum SkeletonVersion
	{
		Nxe = 1,
		Natal,
		NxeAndNatal
	}

	[Flags]
	public enum ReservedFlags
	{
		Unknown = 1,
		PasscodeEnabled = 0x10000000,
		LiveEnabled = 0x20000000,
		Recovering = 0x40000000
	}

	public enum OnlineContentResumeState
	{
		FileHeadersNotReady = 0x46494C48,
		NewFolder = 0x666F6C64,
		NewFolderResumeAttempt1 = 0x666F6C31,
		NewFolderResumeAttempt2 = 0x666F6C32,
		NewFolderResumeAttemptUnknown = 0x666F6C3F,
		NewFolderResumeAttemptSpecific = 0x666F6C40
	}

	[Flags]
	public enum AchievementLockFlags
	{
		UnlockedOnline = 0x10000,   //Indicates the achievement was achieved online.
		Unlocked = 0x20000,		  //Indicates the achievement was achieved.
		Visible = 0x8
	}

	public enum TitleEntryFlags
	{
		NeedsToBeSynced = 0x1,
		ImageNeedsToBeDownloaded = 0x2,
		AvatarAwardNeedsToBeDownloaded = 0x10
	}

	[Flags]
	public enum TransferFlags
	{
		None = 0,
		DeepLinkSupported = 4,
		DisableNetworkStorage = 8,
		KinectEnabled = 0x10,
		MoveOnlyTransfer = 0x20,
		DeviceTransfer = 0x40,
		ProfileTransfer = 0x80
	}
}
