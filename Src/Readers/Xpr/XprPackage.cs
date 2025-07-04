using System;
using FTPcontentManager.Src.Attributes;
using FTPcontentManager.Src.Constants;
using FTPcontentManager.Src.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FTPcontentManager.Src.Xpr
{
	public class XprPackage : BinaryModelBase
	{
		[BinaryData(4, "ascii")]
		public virtual string Magic { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual int FileSize { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual int HeaderSize { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual int TextureCommon { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual int TextureData { get; set; }

		[BinaryData(EndianType.LittleEndian)]
		public virtual int TextureLock { get; set; }

		[BinaryData]
		public virtual byte TextureMisc1 { get; set; }

		[BinaryData(1)]
		public virtual XprFormat TextureFormat { get; set; }

		[BinaryData(1)]
		public virtual byte TextureRes1 { get; set; }

		[BinaryData(1)]
		public virtual byte TextureRes2 { get; set; }

		public byte[] Image
		{
			get { return Binary.ReadBytes(HeaderSize, FileSize - HeaderSize); }
		}

		public int Width
		{
			get { return (int) Math.Pow(2.0, TextureRes2); }
		}

		public int Height
		{
			get { return (int)Math.Pow(2.0, TextureRes2); }
		}

		public bool IsValid
		{
			get { return Magic == "XPR0"; }
		}

		public XprPackage(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}

		public Image<Rgba32> DecompressImage() {
			switch (TextureFormat) {
				case XprFormat.Dxt1:
					return DecompressDxt1();
				case XprFormat.Argb:
					return DecompressArgb();
				default:
					return new Image<Rgba32>(Width, Height);
			}
		}

		private Image<Rgba32> DecompressDxt1() {
			var image = Image;
			var width = Width;
			var height = Height;
			var img = new Image<Rgba32>(width, height);

			// ... implement DXT1 decompression, setting pixels using img[x, y] = new Rgba32(r, g, b, a);
			// ImageSharp does not have SetPixel, use img[x, y] = value;
			// See ImageSharp documentation for pixel manipulation
			return img;
		}

		private Image<Rgba32> DecompressArgb() {
			var image = Image;
			var width = Width;
			var height = Height;
			var img = new Image<Rgba32>(width, height);
			var index = 0;
			for (var i = 0; i < height; i++) {
				for (var j = 0; j < width; j++) {
					img[j, i] = new Rgba32(image[index + 2], image[index + 1], image[index], image[index + 3]);
					index += 4;
				}
			}
			return img;
		}
	}
}