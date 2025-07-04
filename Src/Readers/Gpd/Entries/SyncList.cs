using System;
using System.Collections.Generic;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Gpd.Entries
{
    public class SyncList : List<SyncEntry>
    {
        public XdbfEntry Entry { get; set; }

        public BinaryContainer Binary { get; set; }

        public byte[] AllBytes
        {
            get { return Binary.ReadAll(); }
        }
    }

}