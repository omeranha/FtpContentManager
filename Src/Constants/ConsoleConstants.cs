using System;

namespace FTPcontentManager.Src.Constants
{
	/// <summary>
	/// Console type enumeration for Xbox devices
	/// </summary>
	public enum ConsoleType
	{
		/// <summary>
		/// Development kit console
		/// </summary>
		DevKit = 1,
		
		/// <summary>
		/// Retail/consumer console
		/// </summary>
		Retail = 2
	}

	/// <summary>
	/// Console type flags for extended console identification
	/// </summary>
	[Flags]
	public enum ConsoleTypeFlags
	{
		None = 0,
		DevKit = 1 << 0,
		Retail = 1 << 1,
		TestKit = 1 << 2
	}

	/// <summary>
	/// Controller button enumeration
	/// </summary>
	public enum Button
	{
		Null,
		DpadUp,
		DpadDown,
		DpadLeft,
		DpadRight,
		X,
		Y,
		A,
		B,
		LeftTrigger,
		RightTrigger,
		LeftBumper,
		RightBumper
	}
}
