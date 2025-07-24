using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FtpContentManager.Src.Readers.Stfs.Data;
using FtpContentManager.Src.Models;

namespace FtpContentManager.Src.Readers.Stfs {
	public abstract class SvodPackage : Package<SvodVolumeDescriptor> {
		protected SvodPackage(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset) {
		}

		protected override void Parse() {
		}

		public override void Rehash() {
			throw new NotImplementedException();
		}
	}
}