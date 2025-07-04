using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using FTPcontentManager.Src.Attributes;
using FTPcontentManager.Src.Constants;
using FTPcontentManager.Src.Extensions;
using FTPcontentManager.Src.Stfs.Data;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Stfs
{
	public class Account : BinaryModelBase
	{
		[BinaryData]
		public virtual ReservedFlags ReservedFlags { get; set; }

		[BinaryData]
		public virtual uint LiveFlags { get; set; }

		[BinaryData(32, "utf-16BE", StringReadOptions.AutoTrim)]
		public virtual string GamerTag { get; set; }

		[BinaryData]
		public virtual ulong XUID { get; set; }

		[BinaryData]
		public virtual CachedUserFlags CachedUserFlags { get; set; }

		[BinaryData]
		public virtual XboxLiveServiceProvider ServiceProvider { get; set; }

		[BinaryData(4)]
		public virtual Button[] Passcode { get; set; }

		[BinaryData(20)]
		public virtual string OnlineDomain { get; set; }

		[BinaryData(24)]
		public virtual string KerberosRealm { get; set; }

		[BinaryData(16)]
		public virtual byte[] OnlineKey { get; set; }

		[BinaryData(114)]
		public virtual byte[] UserPassportMemberName { get; set; }

		[BinaryData(32)]
		public virtual byte[] UserPassportPassword { get; set; }

		[BinaryData(114)]
		public virtual byte[] OwnerPassportMemberName { get; set; }

		private static readonly byte[] RetailKey = new byte[] { 0xE1, 0xBC, 0x15, 0x9C, 0x73, 0xB1, 0xEA, 0xE9, 0xAB, 0x31, 0x70, 0xF3, 0xAD, 0x47, 0xEB, 0xF3 };
		private static readonly byte[] DevkitKey = new byte[] { 0xDA, 0xB6, 0x9A, 0xD9, 0x8E, 0x28, 0x76, 0x4F, 0x97, 0x7E, 0xE2, 0x48, 0x7E, 0x4F, 0x3F, 0x68 };

		public Account(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
		{
		}

		public static Account Decrypt(Stream inputStream, ConsoleType consoleType)
		{
			var key = consoleType == ConsoleType.Retail ? RetailKey : DevkitKey;
			var hmac = new HMACSHA1(key);
			var hash = inputStream.ReadBytes(16);

			var hmacResult = hmac.ComputeHash(hash);
			var rc4Key = new byte[16];
			Array.Copy(hmacResult, rc4Key, 16);

			var rest = inputStream.ReadBytes(388);
			var body = RC4Decrypt(rest, rc4Key);

			var compareBuffer = hmac.ComputeHash(body);
			if (!memcmp(hash, compareBuffer, 16))
				throw new InvalidDataException("Keys do not match");
			return ModelFactory.GetModel<Account>(body.Skip(8).ToArray());
		}

		private static byte[] RC4Decrypt(byte[] data, byte[] key)
		{
			byte[] s = new byte[256];
			for (int i = 0; i < 256; i++)
				s[i] = (byte)i;

			int j = 0;
			for (int i = 0; i < 256; i++) {
				j = (j + s[i] + key[i % key.Length]) & 0xFF;
				byte temp = s[i];
				s[i] = s[j];
				s[j] = temp;
			}

			byte[] result = new byte[data.Length];
			int iIndex = 0, jIndex = 0;
			for (int k = 0; k < data.Length; k++) {
				iIndex = (iIndex + 1) & 0xFF;
				jIndex = (jIndex + s[iIndex]) & 0xFF;
				byte temp = s[iIndex];
				s[iIndex] = s[jIndex];
				s[jIndex] = temp;
				byte rnd = s[(s[iIndex] + s[jIndex]) & 0xFF];
				result[k] = (byte)(data[k] ^ rnd);
			}
			return result;
		}

		private static bool memcmp(byte[] data1, byte[] data2, int length)
		{
			for (int i = 0; i < length; i++)
			{
				if (data1[i] != data2[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}
