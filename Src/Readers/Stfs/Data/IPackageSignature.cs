using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Stfs.Data
{
    public interface IPackageSignature : IBinaryModel
    {
        byte[] Signature { get; set; }
    }
}