using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PassThru
{
		class PcapFileWriter: System.Object {
			public PcapFileWriter(string path) {
				this.path = path;
				file = new BinaryWriter(new FileStream(path, FileMode.Append));
				file.Write(0xa1b2c3d4);
				file.Write((ushort)2);
				file.Write((ushort)4);
				file.Write((int)-5);
				file.Write((uint)0);
				file.Write((uint)65535);
				file.Write((uint)1);
				start = DateTime.Now;
			}
			public BinaryWriter file;
			public string path;
			public DateTime start;

			public void AddPacket(byte[] packet) {
				DateTime now = DateTime.Now;
				file.Write((uint)(now.ToFileTime() - start.ToFileTime()));
				file.Write((uint)(now.Millisecond - start.Millisecond));
				file.Write((uint)packet.Length);
				file.Write((uint)packet.Length);
				file.Write(packet);
			}

			public void Close() {
				file.Close();
			}
		}
}
