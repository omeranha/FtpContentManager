namespace FTPcontentManager.Src.Constants
{
	/// <summary>
	/// Online content resume state enumeration
	/// </summary>
	public enum OnlineContentResumeState
	{
		FileHeaderNotReady,
		FileHeaderReady,
		NewlyCreated,
		FileContentReady
	}

	/// <summary>
	/// Xbox Live service provider enumeration
	/// </summary>
	public enum XboxLiveServiceProvider
	{
		PartnerNet,
		XboxLive,
		XboxLiveDevNet
	}
}
