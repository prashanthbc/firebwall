using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PassThru
{
    /// <summary>
    /// Class for writing pcap files
    /// </summary>
	class PcapFileWriter
    {
        /// <summary>
        /// Initiates the pcap file
        /// </summary>
        /// <param name="path"></param>
		public PcapFileWriter(string path) 
        {
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

        /// <summary>
        /// Adds a packet to the file
        /// </summary>
        /// <param name="packet"></param>
		public void AddPacket(byte[] packet) 
        {
			DateTime now = DateTime.Now;
			file.Write((uint)(now.ToFileTime() - start.ToFileTime()));
			file.Write((uint)(now.Millisecond - start.Millisecond));
			file.Write((uint)packet.Length);
			file.Write((uint)packet.Length);
			file.Write(packet);
		}

        /// <summary>
        /// Closes the file handle
        /// </summary>
		public void Close() 
        {
			file.Close();
		}
	}
}
