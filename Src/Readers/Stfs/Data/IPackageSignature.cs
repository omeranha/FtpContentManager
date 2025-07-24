using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Stfs.Data
{
	public interface IPackageSignature : IBinaryModel
	{
		byte[] Signature { get; set; }
	}
}