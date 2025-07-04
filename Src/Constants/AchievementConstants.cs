using System;

namespace FTPcontentManager.Src.Constants
{
	/// <summary>
	/// Achievement lock flags
	/// </summary>
	[Flags]
	public enum AchievementLockFlags
	{
		UnlockedOnline = 0x10000,
		Unlocked = 0x20000,
		Visible = 0x8
	}

	/// <summary>
	/// Title entry flags for achievements
	/// </summary>
	[Flags]
	public enum TitleEntryFlags
	{
		None = 0,
		SyncAchievements = 1 << 0,
		SyncStats = 1 << 1,
		NeedsUpdate = 1 << 2,
		HasDlc = 1 << 3
	}
}
