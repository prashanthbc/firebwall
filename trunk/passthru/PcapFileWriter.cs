﻿using System;
using System.Collections.Generic;
using System.Threading;
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
			file.Write((int)0);
			file.Write((uint)0);
			file.Write((uint)65535);
			file.Write((uint)1);
            referenceTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			start = DateTime.UtcNow;
            t = new Thread(new ThreadStart(WriteLoop));
            t.Start();
		}

        void WriteLoop()
        {
            KeyValuePair<DateTime, byte[]> pkt = new KeyValuePair<DateTime,byte[]>();
            while (true)
            {
                bool toWrite = false;
                lock (padlock)
                {
                    if (packetQueue.Count != 0)
                    {
                        pkt = packetQueue.Dequeue();
                        toWrite = true;
                    }
                }
                if (toWrite)
                {
                    file.Write((uint)(pkt.Key - referenceTime).TotalSeconds);
                    file.Write((uint)(pkt.Key.Millisecond));
                    file.Write((uint)pkt.Value.Length);
                    file.Write((uint)pkt.Value.Length);
                    file.Write(pkt.Value);
                }
                Thread.Sleep(100);
            }
        }

        Thread t;
		public BinaryWriter file;
		public string path;
		public DateTime start;
        DateTime referenceTime;

        object padlock = new object();
        Queue<KeyValuePair<DateTime, byte[]>> packetQueue = new Queue<KeyValuePair<DateTime, byte[]>>();

        /// <summary>
        /// Adds a packet to the file
        /// </summary>
        /// <param name="packet"></param>
		public unsafe void AddPacket(byte* packet, int length) 
        {
            byte[] pkt = new byte[length];
            for (int x = 0; x < length; x++)
                pkt[x] = packet[x];
            lock (padlock)
            {                
                packetQueue.Enqueue(new KeyValuePair<DateTime, byte[]>(DateTime.UtcNow, pkt));
            }
		}

        /// <summary>
        /// Closes the file handle
        /// </summary>
		public void Close() 
        {
            t.Abort();
			file.Close();
		}
	}
}
