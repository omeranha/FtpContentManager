using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FTPcontentManager.Src.Readers.Stfs.Data;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Readers.Stfs {
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