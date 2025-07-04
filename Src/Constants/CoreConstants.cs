namespace FTPcontentManager.Src.Constants
{
	/// <summary>
	/// Core offset constants for Xbox file systems
	/// </summary>
	public static class Offsets
	{
		/// <summary>
		/// Data blocks per hash tree level for Xbox file system
		/// </summary>
		public static readonly uint[] DataBlocksPerHashTreeLevel = new uint[] { 1, 0xAA, 0x70E4 };

		/// <summary>
		/// Standard sector size in bytes
		/// </summary>
		public const uint SectorSize = 512;

		/// <summary>
		/// Xbox 360 block size in bytes
		/// </summary>
		public const uint BlockSize = 0x1000;

		/// <summary>
		/// Hash block size in bytes
		/// </summary>
		public const uint HashBlockSize = 0x1000;
	}
}
