using System;
using FTPcontentManager.Src.Attributes;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Gpd.Entries
{
    public class SyncData : EntryBase
    {
        [BinaryData]
        public virtual ulong NextSyncId { get; set; }

        [BinaryData]
        public virtual ulong LastSyncId { get; set; }

        [BinaryData]
        public virtual DateTime LastSyncedTime { get; set; }

        public SyncData(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}