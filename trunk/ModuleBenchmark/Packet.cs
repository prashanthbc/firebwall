using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace ModuleBenchmark
{
    /// <summary>
    /// Different protocols that are supported or that will be
    /// </summary>
    public enum Protocol
    {
        EEth,
        Ethernet,
        IP,
        ARP,
        TCP,
        UDP,
        ICMP,
        DNS,
        DHCP
    }

    /// <summary>
    /// Interface for a network Packet
    /// Packets are upward processing!
    /// </summary>
    public abstract class Packet
    {
        public abstract bool ContainsLayer(Protocol layer);

        public abstract byte[] Data();

        public abstract Protocol GetHighestLayer();

        public abstract uint Length();

        public abstract uint LayerStart();

        public abstract uint LayerLength();

        public abstract Packet MakeNextLayerPacket();

        public abstract bool Outbound
        {
            get;
            set;
        }
    }

    public class Ndisapi
    {
        public static uint PACKET_FLAG_ON_SEND = 1;
        public static uint PACKET_FLAG_ON_RECEIVE = 1 << 1;
    }

    public class INTERMEDIATE_BUFFER
    {
        public byte[] m_IBuffer
        {
            get;
            set;
        }

        public uint m_Length
        {
            get;
            set;
        }

        public uint m_dwDeviceFlags
        {
            get;
            set;
        }
    }

    /*
        Ethernet frame packet
        */
    public class EthPacket : Packet
    {
        public EthPacket(ref INTERMEDIATE_BUFFER in_packet)
        {
            data = in_packet;
        }
        public INTERMEDIATE_BUFFER data;

        public override bool ContainsLayer(Protocol layer)
        {
            return (layer == Protocol.Ethernet);
        }

        public override byte[] Data()
        {
            return data.m_IBuffer;
        }

        public override Protocol GetHighestLayer()
        {
            return Protocol.Ethernet;
        }

        public override uint Length()
        {
            return data.m_Length;
        }

        public override uint LayerStart()
        {
            return 0;
        }

        public override uint LayerLength()
        {
            return 14;
        }

        public override Packet MakeNextLayerPacket()
        {
            if (isEETH())
            {
                return new EETHPacket(ref data).MakeNextLayerPacket();
            }
            else if (isIP())
            {
                return new IPPacket(ref data).MakeNextLayerPacket();
            }
            else if (isARP())
                return new ARPPacket(ref data);
            else
                return this;
        }

        public override bool Outbound
        {
            get
            {
                return (data.m_dwDeviceFlags == Ndisapi.PACKET_FLAG_ON_SEND);
            }
            set
            {
                if (value)
                {
                    data.m_dwDeviceFlags = Ndisapi.PACKET_FLAG_ON_SEND;
                }
                else
                {
                    data.m_dwDeviceFlags = Ndisapi.PACKET_FLAG_ON_RECEIVE;
                }
            }
        }

        // check Type for 0x0806
        public bool isARP()
        {
            return (data.m_IBuffer[0x0c] == 0x08 && data.m_IBuffer[0x0d] == 0x06);
        }

        public bool isEETH()
        {
            return (data.m_IBuffer[0x0c] == 0x98 && data.m_IBuffer[0x0d] == 0x09);
        }

        public bool isIP()
        {
            return (data.m_IBuffer[0x0c] == 0x08 && data.m_IBuffer[0x0d] == 0x00);
        }

        public PhysicalAddress FromMac
        {
            get
            {
                byte[] mac = new byte[6];
                Buffer.BlockCopy(data.m_IBuffer, 6, mac, 0, 6);
                return new PhysicalAddress(mac);
            }
            set
            {
                byte[] mac = value.GetAddressBytes();
                Buffer.BlockCopy(mac, 0, data.m_IBuffer, 6, 6);
            }
        }

        public PhysicalAddress ToMac
        {
            get
            {
                byte[] mac = new byte[6];
                Buffer.BlockCopy(data.m_IBuffer, 0, mac, 0, 6);
                return new PhysicalAddress(mac);
            }
            set
            {
                byte[] mac = value.GetAddressBytes();
                Buffer.BlockCopy(mac, 0, data.m_IBuffer, 0, 6);
            }
        }
    }

    /// <summary>
    /// Encrypted ethernet packet
    /// (Not yet implemented)
    /// </summary>
    public class EETHPacket : EthPacket
    {
        public EETHPacket(ref EthPacket eth)
            : base(ref eth.data)
        {
        }

        public EETHPacket(ref INTERMEDIATE_BUFFER in_packet)
            : base(ref in_packet)
        {
        }

        public override bool ContainsLayer(Protocol layer)
        {
            if (layer == Protocol.EEth)
                return true;
            else
                return base.ContainsLayer(layer);
        }

        public override Protocol GetHighestLayer()
        {
            return Protocol.EEth;
        }

        public override Packet MakeNextLayerPacket()
        {
            return this;
        }
    }

    /*
        ICMPPacket object
        */
    public class ICMPPacket : IPPacket
    {
        // accepts intermediate buff, checks if ICMP
        public ICMPPacket(ref INTERMEDIATE_BUFFER in_packet)
            : base(ref in_packet)
        {
            if (!isICMP())
                throw new Exception("Not an ICMP packet!");
            start = base.LayerStart() + base.LayerLength();
        }

        // accepts IPPacket, checks if ICMP
        public ICMPPacket(ref IPPacket eth)
            : base(ref eth.data)
        {
            if (!isICMP())
                throw new Exception("Not an ICMP packet!");
            start = base.LayerStart() + base.LayerLength();
        }

        // return if ICMP; else return to base layer to find layer
        public override bool ContainsLayer(Protocol layer)
        {
            if (layer == Protocol.ICMP)
                return true;
            else
                return base.ContainsLayer(layer);
        }

        // return ICMP (highest)
        public override Protocol GetHighestLayer()
        {
            return Protocol.ICMP;
        }

        // return this (highest)
        public override Packet MakeNextLayerPacket()
        {
            return this;
        }

        uint start = 0;

        public override uint LayerStart()
        {
            return start;
        }

        public override uint LayerLength()
        {
            return 8;
        }

        // return ICMP code
        public string getCode()
        {
            return (data.m_IBuffer[start + 1]).ToString();
        }

        // return ICMP type
        public string getType()
        {
            return (data.m_IBuffer[start]).ToString();
        }
    }

    /*
        ARP object
        */
    public class ARPPacket : EthPacket
    {
        public ARPPacket(ref EthPacket eth)
            : base(ref eth.data)
        {
            if (!isARP())
                throw new Exception("Not an ARP packet!");
            start = base.LayerStart() + base.LayerLength();
            length = this.Length() - base.LayerLength();
        }

        public ARPPacket(ref INTERMEDIATE_BUFFER in_packet)
            : base(ref in_packet)
        {
            if (!isARP())
                throw new Exception("Not an ARP packet!");
            start = base.LayerStart() + base.LayerLength();
            length = this.Length() - base.LayerLength();
        }

        public override bool ContainsLayer(Protocol layer)
        {
            if (layer == Protocol.ARP)
                return true;
            else
                return base.ContainsLayer(layer);
        }

        public override Protocol GetHighestLayer()
        {
            return Protocol.ARP;
        }

        public override Packet MakeNextLayerPacket()
        {
            return this;
        }

        uint start = 0;

        public override uint LayerStart()
        {
            return start;
        }

        uint length = 0;

        public override uint LayerLength()
        {
            return length;
        }

        public bool isRequest
        {
            get
            {
                return (data.m_IBuffer[start + 6] == 0x00 && data.m_IBuffer[start + 7] == 0x01);
            }
            set
            {
                if (value)
                {
                    data.m_IBuffer[start + 6] = 0x00;
                    data.m_IBuffer[start + 7] = 0x01;
                }
                else
                {
                    data.m_IBuffer[start + 6] = 0x00;
                    data.m_IBuffer[start + 7] = 0x02;
                }
            }
        }

        public IPAddress ASenderIP
        {
            get
            {
                byte[] ip = new byte[4];
                Buffer.BlockCopy(data.m_IBuffer, (int)start + 0xe, ip, 0, 4);
                return new IPAddress(ip);
            }
            set
            {
                byte[] ip = value.GetAddressBytes();
                Buffer.BlockCopy(ip, 0, data.m_IBuffer, (int)start + 0xe, 4);
            }
        }

        public PhysicalAddress ASenderMac
        {
            get
            {
                byte[] ip = new byte[6];
                Buffer.BlockCopy(data.m_IBuffer, (int)start + 0x8, ip, 0, 6);
                return new PhysicalAddress(ip);
            }
            set
            {
                byte[] ip = value.GetAddressBytes();
                Buffer.BlockCopy(ip, 0, data.m_IBuffer, (int)start + 0x8, 6);
            }
        }

        public IPAddress ATargetIP
        {
            get
            {
                byte[] ip = new byte[4];
                Buffer.BlockCopy(data.m_IBuffer, (int)start + 0x18, ip, 0, 4);
                return new IPAddress(ip);
            }
            set
            {
                byte[] ip = value.GetAddressBytes();
                Buffer.BlockCopy(ip, 0, data.m_IBuffer, (int)start + 0x18, 4);
            }
        }

        public PhysicalAddress ATargetMac
        {
            get
            {
                byte[] ip = new byte[6];
                Buffer.BlockCopy(data.m_IBuffer, (int)start + 0x12, ip, 0, 6);
                return new PhysicalAddress(ip);
            }
            set
            {
                byte[] ip = value.GetAddressBytes();
                Buffer.BlockCopy(ip, 0, data.m_IBuffer, (int)start + 0x12, 6);
            }
        }
    }

    /*
        IP Packet object
        */
    public class IPPacket : EthPacket
    {
        public IPPacket(ref EthPacket eth)
            : base(ref eth.data)
        {
            if (!isIP())
                throw new Exception("Not an IP packet!");
            start = base.LayerStart() + base.LayerLength();
            length = (uint)((data.m_IBuffer[start] & 0xf) * 4);
        }

        public IPPacket(ref INTERMEDIATE_BUFFER in_packet)
            : base(ref in_packet)
        {
            if (!isIP())
                throw new Exception("Not an IP packet!");
            start = base.LayerStart() + base.LayerLength();
            length = (uint)((data.m_IBuffer[start] & 0xf) * 4);
        }

        public override bool ContainsLayer(Protocol layer)
        {
            if (layer == Protocol.IP)
                return true;
            else
                return base.ContainsLayer(layer);
        }

        public override Protocol GetHighestLayer()
        {
            return Protocol.IP;
        }

        public override Packet MakeNextLayerPacket()
        {
            if (isTCP())
            {
                return new TCPPacket(ref data).MakeNextLayerPacket();
            }
            else if (isUDP())
                return new UDPPacket(ref data).MakeNextLayerPacket();
            else if (isICMP())
                return new ICMPPacket(ref data).MakeNextLayerPacket();
            else
                return this;
        }

        uint start = 0;

        public override uint LayerStart()
        {
            return start;
        }

        uint length = 0;

        public override uint LayerLength()
        {
            return length;
        }

        public bool isTCP()
        {
            return (data.m_IBuffer[start + 0x9] == 0x06);
        }

        public bool isUDP()
        {
            return (data.m_IBuffer[start + 0x9] == 0x11);
        }

        public bool isICMP()
        {
            return (data.m_IBuffer[start + 0x9] == 0x01);
        }

        public IPAddress DestIP
        {
            get
            {
                if (IPVersion == 0x4)
                {
                    byte[] ip = new byte[4];
                    Buffer.BlockCopy(data.m_IBuffer, (int)start + 0x10, ip, 0, 4);
                    return new IPAddress(ip);
                }
                else
                    return null;
            }
        }

        public byte IPVersion
        {
            get
            {
                return (byte)(data.m_IBuffer[start] >> 4);
            }
        }

        public IPAddress SourceIP
        {
            get
            {
                if (IPVersion == 0x4)
                {
                    byte[] ip = new byte[4];
                    Buffer.BlockCopy(data.m_IBuffer, (int)start + 0xc, ip, 0, 4);
                    return new IPAddress(ip);
                }
                else
                    return null;
            }
        }
    }

    /*
        UDP Packet object
        */
    public class UDPPacket : IPPacket
    {
        public UDPPacket(ref INTERMEDIATE_BUFFER in_packet)
            : base(ref in_packet)
        {
            if (!isUDP())
                throw new Exception("Not a UDP packet!");
            start = base.LayerStart() + base.LayerLength();
        }

        public UDPPacket(ref IPPacket eth)
            : base(ref eth.data)
        {
            if (!isUDP())
                throw new Exception("Not a UDP packet!");
            start = base.LayerStart() + base.LayerLength();
        }

        public override bool ContainsLayer(Protocol layer)
        {
            if (layer == Protocol.UDP)
                return true;
            else
                return base.ContainsLayer(layer);
        }

        public override Protocol GetHighestLayer()
        {
            return Protocol.UDP;
        }

        public override Packet MakeNextLayerPacket()
        {
            if (isDNS())
            {
                return new DNSPacket(ref data).MakeNextLayerPacket();
            }
            return this;
        }

        uint start = 0;

        public override uint LayerStart()
        {
            return start;
        }

        public override uint LayerLength()
        {
            return 8;
        }

        public bool isDNS()
        {
            return (SourcePort == 53 || DestPort == 53);
        }

        public ushort DestPort
        {
            get
            {
                return (ushort)((data.m_IBuffer[start + 2] << 8) + data.m_IBuffer[start + 3]);
            }
        }

        public ushort SourcePort
        {
            get
            {
                return (ushort)((data.m_IBuffer[start] << 8) + data.m_IBuffer[start + 1]);
            }
        }
    }

    /*
        DNS Packet object
     */
    public class DNSPacket : UDPPacket
    {
        uint start = 0;
        uint length = 0;

        public DNSPacket(ref INTERMEDIATE_BUFFER in_packet)
            : base(ref in_packet)
        {
            if (!isDNS())
                throw new Exception("Not a DNS packet!");
            start = base.LayerStart() + base.LayerLength();
            length = Length() - start;
        }

        public DNSPacket(ref UDPPacket eth)
            : base(ref eth.data)
        {
            if (!isDNS())
                throw new Exception("Not a DNS packet!");
            start = base.LayerStart() + base.LayerLength();
            length = Length() - start;
        }

        public bool Response
        {
            get
            {
                return (data.m_IBuffer[start] & 0x80) == 0x80;
            }
            set
            {
                if (value)
                {
                    data.m_IBuffer[start] = (byte)(data.m_IBuffer[start] | 0x80);
                }
                else
                {
                    if ((data.m_IBuffer[start] & 0x80) == 0x80)
                        data.m_IBuffer[start] -= 0x80;
                }
            }
        }

        public ushort QuestionCount
        {
            get
            {
                return (ushort)((data.m_IBuffer[start + 2] << 8) + data.m_IBuffer[start + 3]);
            }
            set
            {
                data.m_IBuffer[start + 2] = (byte)(value >> 8);
                data.m_IBuffer[start + 3] = (byte)(value & 0xff);
            }
        }

        public override uint LayerStart()
        {
            return start;
        }

        public override uint LayerLength()
        {
            return length;
        }

        public override bool ContainsLayer(Protocol layer)
        {
            if (layer == Protocol.DNS)
                return true;
            else
                return base.ContainsLayer(layer);
        }

        public override Protocol GetHighestLayer()
        {
            return Protocol.DNS;
        }

        public override Packet MakeNextLayerPacket()
        {
            return this;
        }
    }

    /*
        TCP Packet object
     */
    public class TCPPacket : IPPacket
    {
        public TCPPacket(ref INTERMEDIATE_BUFFER in_packet)
            : base(ref in_packet)
        {
            if (!isTCP())
                throw new Exception("Not a TCP packet!");
            start = base.LayerStart() + base.LayerLength();
            length = (uint)((data.m_IBuffer[start + 12] >> 4) * 4);
        }

        public TCPPacket(ref IPPacket eth)
            : base(ref eth.data)
        {
            if (!isTCP())
                throw new Exception("Not a TCP packet!");
            start = base.LayerStart() + base.LayerLength();
            length = (uint)((data.m_IBuffer[start + 12] >> 4) * 4);
        }

        public override bool ContainsLayer(Protocol layer)
        {
            if (layer == Protocol.TCP)
                return true;
            else
                return base.ContainsLayer(layer);
        }

        public override Protocol GetHighestLayer()
        {
            return Protocol.TCP;
        }

        public override Packet MakeNextLayerPacket()
        {
            return this;
        }

        uint start = 0;

        public override uint LayerStart()
        {
            return start;
        }

        uint length = 0;

        public override uint LayerLength()
        {
            return length;
        }

        public bool AckSet
        {
            get
            {
                return ((data.m_IBuffer[start + 13] & 0x10) == 0x10);
            }
        }

        public ushort DestPort
        {
            get
            {
                return (ushort)((data.m_IBuffer[start + 2] << 8) + data.m_IBuffer[start + 3]);
            }
        }

        public bool FinSet
        {
            get
            {
                return ((data.m_IBuffer[start + 13] & 0x01) == 0x01);
            }
        }

        public bool PshSet
        {
            get
            {
                return ((data.m_IBuffer[start + 13] & 0x08) == 0x08);
            }
        }

        public bool RstSet
        {
            get
            {
                return ((data.m_IBuffer[start + 13] & 0x04) == 0x04);
            }
        }

        public ushort SourcePort
        {
            get
            {
                return (ushort)((data.m_IBuffer[start] << 8) + data.m_IBuffer[start + 1]);
            }
        }

        public bool SynSet
        {
            get
            {
                return ((data.m_IBuffer[start + 13] & 0x02) == 0x02);
            }
        }
    }
}
