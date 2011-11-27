using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModuleBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            double bfms = 0;
            double benms = 0;
            int iterations = 100;
            List<Packet> packets = new List<Packet>();
            PcapReader pr = new PcapReader("input.pcap");
            while (true)
            {
                byte[] packetBuf = pr.ReadPacket();
                if (packetBuf == null)
                    break;
                INTERMEDIATE_BUFFER ib = new INTERMEDIATE_BUFFER() { m_IBuffer = packetBuf, m_Length = (uint)packetBuf.Length };
                if (new System.Net.NetworkInformation.PhysicalAddress(packetBuf).Equals(System.Net.NetworkInformation.PhysicalAddress.Parse("00c0ca36e1fe".ToUpper())))
                    ib.m_dwDeviceFlags = Ndisapi.PACKET_FLAG_ON_RECEIVE;
                else
                    ib.m_dwDeviceFlags = Ndisapi.PACKET_FLAG_ON_SEND;
                Packet pkt = new EthPacket(ref ib).MakeNextLayerPacket();
                packets.Add(pkt);
                //InteriorMain(ref pkt);
            }
            for (int x = 0; x < iterations; x++)
            {
                BasicFirewall bm = new BasicFirewall();
                bfms += bm.Benchmark(packets).TotalMilliseconds;
                OldBasicFirewall benm = new OldBasicFirewall();
                benms += benm.Benchmark(packets).TotalMilliseconds;
            }
            Console.WriteLine(((benms - bfms) / iterations).ToString() + " milliseconds in the module");
        }
    }
}
