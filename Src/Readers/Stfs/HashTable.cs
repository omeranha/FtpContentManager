using System;
using System.Collections.Generic;
using FTPcontentManager.Src.Attributes;
using FTPcontentManager.Src.Stfs.Data;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Stfs
{
    public class HashTable : BinaryModelBase
    {
        [BinaryData(0xAA)]
        public virtual HashEntry[] Entries { get; set; }

        [BinaryData]
        public virtual int AllocatedBlockCount { get; set; }

        public int Block { get; set; }

        public int EntryCount { get; set; }

        public List<HashTable> Tables { get; set; }

        public HashTable(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}