using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Readers.Stfs.Data
{
	public interface IPackageSignature : IBinaryModel
	{
		byte[] Signature { get; set; }
	}
}