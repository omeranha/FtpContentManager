using System.Linq;
using FTPcontentManager.Src.Constants;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Gpd
{
	public class DashboardFile : GpdFile
	{
		public byte[] GamerPicture { get; set; }
		public byte[] AvatarImage { get; set; }

		public int OptionControllerVibration
		{
			get { return Settings.Get<int>(SettingId.OptionControllerVibration); }
			set { Settings.Set(SettingId.OptionControllerVibration, value); }
		}

		public int OptionVoiceMuted
		{
			get { return Settings.Get<int>(SettingId.OptionVoiceMuted); }
			set { Settings.Set(SettingId.OptionVoiceMuted, value); }
		}

		public int OptionVoiceThruSpeakers
		{
			get { return Settings.Get<int>(SettingId.OptionVoiceThruSpeakers); }
			set { Settings.Set(SettingId.OptionVoiceThruSpeakers, value); }
		}

		public int OptionVoiceVolume
		{
			get { return Settings.Get<int>(SettingId.OptionVoiceVolume); }
			set { Settings.Set(SettingId.OptionVoiceVolume, value); }
		}

		public int GamercardCred
		{
			get { return Settings.Get<int>(SettingId.GamercardCred); }
			set { Settings.Set(SettingId.GamercardCred, value); }
		}

		public int GamercardTitlesPlayed
		{
			get { return Settings.Get<int>(SettingId.GamercardTitlesPlayed); }
			set { Settings.Set(SettingId.GamercardTitlesPlayed, value); }
		}

		public int GamercardAchievementsEarned
		{
			get { return Settings.Get<int>(SettingId.GamercardAchievementsEarned); }
			set { Settings.Set(SettingId.GamercardAchievementsEarned, value); }
		}

		public string GamercardPictureKey
		{
			get { return Settings.Get<string>(SettingId.GamercardPictureKey); }
			set { Settings.Set(SettingId.GamercardPictureKey, value); }
		}

		public byte[] GamercardAvatarInfo1
		{
			get { return Settings.Get<byte[]>(SettingId.GamercardAvatarInfo1); }
			set { Settings.Set(SettingId.GamercardAvatarInfo1, value); }
		}

		public int MessengerAutoSignin
		{
			get { return Settings.Get<int>(SettingId.MessengerAutoSignin); }
			set { Settings.Set(SettingId.OptionVoiceMuted, value); }
		}

		protected DashboardFile(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			const int titleInformation = (int)SettingId.TitleInformation;
			const int avatarImage = (int)SettingId.AvatarImage;
			if (HasEntry(EntryType.Image, titleInformation)) GamerPicture = Images.Get(titleInformation).ImageData;
			if (HasEntry(EntryType.Image, avatarImage)) AvatarImage = Images.Get(avatarImage).ImageData;
		}

		public override void Recalculate()
		{
			base.Recalculate();
			GamercardTitlesPlayed = TitlesPlayed.Count;
			GamercardAchievementsEarned = TitlesPlayed.Sum(g => g.AchievementsUnlocked);
			GamercardCred = TitlesPlayed.Sum(g => g.GamerscoreUnlocked);
		}
	}
}