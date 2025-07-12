using System.Collections.Generic;
using FTPcontentManager.Src.Attributes;
using FTPcontentManager.Src.Constants;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Readers.Xex
{
	[XexHeader(XexOptionalHeaderId.DecompressionInformation)]
	public class XexCompressionInfo : BinaryModelBase
	{
		[BinaryData]
		public virtual uint InfoSize { get; set; }

		[BinaryData(2)]
		public virtual XexEncryptionType EncryptionType { get; set; }

		[BinaryData(2)]
		public virtual XexCompressionType CompressionType { get; set; }

		private XexRawBaseFileBlock[] _rawBlocks;
		public XexRawBaseFileBlock[] RawBlocks
		{
			get
			{
				if (_rawBlocks == null)
				{
					_rawBlocks = new XexRawBaseFileBlock[(InfoSize - 8) / 8];
					for (var i = 0; i < _rawBlocks.Length; i++)
					{
						_rawBlocks[i] = ModelFactory.GetModel<XexRawBaseFileBlock>(Binary, StartOffset + 8 + i * 8);
					}
				}
				return _rawBlocks;
			}
		}

		private XexCompressedBaseFile _compressedBaseFile;
		public XexCompressedBaseFile CompressedBaseFile
		{
			get {
				return _compressedBaseFile ?? (_compressedBaseFile = ModelFactory.GetModel<XexCompressedBaseFile>(Binary, StartOffset + 8));
			}
		}

		public XexCompressionInfo(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}
	}
}