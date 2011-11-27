using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModuleBenchmark
{
    class PcapReader
    {
        BinaryReader reader;

        public PcapReader(string file)
        {
            reader = new BinaryReader(new FileStream(file, FileMode.Open));
            reader.ReadUInt32();
            reader.ReadUInt16();
            reader.ReadUInt16();
            reader.ReadInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
        }

        public byte[] ReadPacket()
        {
            if (reader.BaseStream.Position >= reader.BaseStream.Length)
            {
                reader.Close();
                return null;
            }
            try
            {
                reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                uint length = reader.ReadUInt32();
                byte[] packet = new byte[length];
                reader.Read(packet, 0, (int)length);
                return packet;
            }
            catch
            {
                reader.Close();
                return null;
            }
        }
    }
}
