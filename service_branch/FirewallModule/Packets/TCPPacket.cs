﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace FM
{
    /// <summary>
    /// TCP packet obj
    /// </summary>
    public unsafe class TCPPacket : IPPacket
    {
        public TCPPacket(INTERMEDIATE_BUFFER* in_packet)
            : base(in_packet)
        {
            if (!isTCP())
                throw new Exception("Not a TCP packet!");
            start = base.LayerStart() + base.LayerLength();
            length = (uint)((data->m_IBuffer[start + 12] >> 4) * 4);
        }

        public TCPPacket(IPPacket eth)
            : base(eth.data)
        {
            if (!isTCP())
                throw new Exception("Not a TCP packet!");
            start = base.LayerStart() + base.LayerLength();
            length = (uint)((data->m_IBuffer[start + 12] >> 4) * 4);
        }

        public byte[] GetApplicationLayer_safe()
        {
            if (data->m_IBuffer[start + 14] == 0xff && data->m_IBuffer[start + 15] == 0x37)
                return new byte[0];
            uint dataStart = start + length;
            uint dataEnd = this.Length();

            byte[] d = new byte[dataEnd - dataStart];
            for (uint i = dataStart; i < dataEnd; i++)
            {
                d[i - dataStart] = data->m_IBuffer[i];
            }
            return d;
        }

        // (g|s)et the sequence number
        public UInt32 SequenceNumber
        {
            get
            {
                return (UInt32)((data->m_IBuffer[start + 4] << 24) | (data->m_IBuffer[start + 5] << 16) | (data->m_IBuffer[start + 6] << 8) | data->m_IBuffer[start + 7]);
            }
            set
            {
                data->m_IBuffer[start + 4] = (byte)(value << 24);
                data->m_IBuffer[start + 5] = (byte)(value << 16);
                data->m_IBuffer[start + 6] = (byte)(value << 8);
                data->m_IBuffer[start + 7] = (byte)(value & 0xff);
            }
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

        // (g|s)et ACK flag
        public bool ACK
        {
            get
            {
                return ((data->m_IBuffer[start + 13] & 0x10) == 0x10);
            }
            set
            {
                data->m_IBuffer[start + 13] |= (1 << 5);
            }
        }

        // (g|s)et destination port
        public ushort DestPort
        {
            get
            {
                return (ushort)((data->m_IBuffer[start + 2] << 8) + data->m_IBuffer[start + 3]);
            }
            set
            {
                data->m_IBuffer[start + 2] = (byte)(value >> 8);
                data->m_IBuffer[start + 3] = (byte)(value & 0xff);
            }
        }

        // (g|s)et FIN flag
        public bool FIN
        {
            get
            {
                return ((data->m_IBuffer[start + 13] & 0x01) == 0x01);
            }
            set
            {
                data->m_IBuffer[start + 13] |= (1 << 1);
            }
        }

        // (g|s)et PSH flag
        public bool PSH
        {
            get
            {
                return ((data->m_IBuffer[start + 13] & 0x08) == 0x08);
            }
            set
            {
                data->m_IBuffer[start + 13] |= (1 << 4);
            }
        }

        // (g|s)et RST flag
        public bool RST
        {
            get
            {
                return ((data->m_IBuffer[start + 13] & 0x04) == 0x04);
            }
            set
            {
                data->m_IBuffer[start + 13] |= (1 << 3);
            }
        }

        // (g|s)et the source port
        public ushort SourcePort
        {
            get
            {
                return (ushort)((data->m_IBuffer[start] << 8) + data->m_IBuffer[start + 1]);
            }
            set
            {
                data->m_IBuffer[start] = (byte)(value << 8);
                data->m_IBuffer[start + 1] = (byte)(value & 0xff);
            }
        }

        // (g|s)et SYN flag
        public bool SYN
        {
            get
            {
                return ((data->m_IBuffer[start + 13] & 0x02) == 0x02);
            }
            set
            {
                data->m_IBuffer[start + 13] |= (1 << 2);
            }
        }

        // (g|s)et URG flag
        public bool URG
        {
            get
            {
                return ((data->m_IBuffer[start + 13] & 0x20) == 0x20);
            }
            set
            {
                data->m_IBuffer[start + 13] |= (1 << 6);
            }
        }

        // ECN-Echo flag
        public bool ECE
        {
            get
            {
                return ((data->m_IBuffer[start + 13] & 0x40) == 0x40);
            }
            set
            {
                data->m_IBuffer[start + 13] |= (1 << 7);
            }
        }

        // CWR flag
        public bool CWR
        {
            get
            {
                return ((data->m_IBuffer[start + 13] & 0x80) == 0x80);
            }
            set
            {
                // data->m_IBuffer[start + 13] |= (1 << 8);
            }
        }

        // ECN nonce added in RFC-3540
        public bool NONCE
        {
            get
            {
                return ((data->m_IBuffer[start + 13] & 0x100) == 0x100);
            }
            set
            {
                // data->m_IBuffer[start + 13] |= (1 << 9);
            }
        }

        // get/set the checksum of the packet
        public ushort Checksum
        {
            get
            {
                return (ushort)((data->m_IBuffer[start + 16] << 8) | (data->m_IBuffer[start + 17]));
            }
            set
            {
                data->m_IBuffer[start + 16] = (byte)(value >> 8);
                data->m_IBuffer[start + 17] = (byte)(value & 0xff);
            }
        }

        // get/set window size
        public ushort WindowSize
        {
            get
            {
                return (ushort)((data->m_IBuffer[start + 14] << 8) | (data->m_IBuffer[start + 15]));
            }
            set
            {
                data->m_IBuffer[start + 14] = (byte)(value >> 8);
                data->m_IBuffer[start + 15] = (byte)(value & 0xff);
            }
        }

        // get/set the acknowledgement number
        public UInt32 AckNumber
        {
            get
            {
                return (UInt32)((data->m_IBuffer[start + 8] << 24) | (data->m_IBuffer[start + 9] << 16) |
                                (data->m_IBuffer[start + 10] << 8) | (data->m_IBuffer[start + 11]));
            }
            set
            {
                data->m_IBuffer[start + 8] = (byte)(value << 24);
                data->m_IBuffer[start + 9] = (byte)(value << 16);
                data->m_IBuffer[start + 10] = (byte)(value << 8);
                data->m_IBuffer[start + 11] = (byte)(value & 0xff);
            }
        }

        public ushort GenerateChecksum
        {
            get
            {
                return GetChecksum();
            }
        }

        // adaptation from http://www.netfor2.com/tcpsum.htm
        private ushort GetChecksum()
        {
            UInt32 sum = 0;

            for (uint i = this.start; i < this.start + 16; i += 2)
            {
                sum += (UInt32)(data->m_IBuffer[i] << 8) | data->m_IBuffer[i + 1];
            }
            for (uint i = this.start + 18; i < TotalLength + base.LayerStart() - 1; i += 2)
            {
                sum += (UInt32)(data->m_IBuffer[i] << 8) | data->m_IBuffer[i + 1];                
            }
            if (((TotalLength + base.LayerStart()) % 2) == 1)
            {
                sum += (UInt32)(data->m_IBuffer[TotalLength + base.LayerStart() - 1] << 8);
            }

            // src addr
            byte[] srcB = SourceIP.GetAddressBytes();
            for (int i = 0; i < 4; i += 2)
            {
                sum += (UInt32)(((srcB[i] << 8) & 0xFF00) | (srcB[i + 1] & 0xFF));
            }

            // dst addr
            byte[] destB = DestIP.GetAddressBytes();
            for (int i = 0; i < 4; i += 2)
            {
                sum += (UInt32)(((destB[i] << 8) & 0xFF00) | (destB[i + 1] & 0xFF));
            }

            // proto
            sum += 0x0006;

            // length
            sum += ((UInt16)(TotalLength - (base.LayerLength())));
            
            // 1's compliment
            while ((sum >> 16) != 0)
                sum = ((sum & 0xFFFF) + (sum >> 16));

            sum = ~sum;
            //sum = 0xFFFF - sum;

            return (ushort)sum;
            //return (ushort)ntoh((UInt16)sum);
        }

        // network to host order
        private ushort ntoh(UInt16 i)
        {
            int x = IPAddress.NetworkToHostOrder(i);
            return (ushort)(x >> 16);
        }
    }
}
