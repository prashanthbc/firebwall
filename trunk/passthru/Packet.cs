using System;
using System.Collections.Generic;
using System.Text;
using NdisapiSpace;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Net;

namespace PassThru
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
	public abstract unsafe class Packet 
    {
		public abstract bool ContainsLayer(Protocol layer);

        public abstract byte* Data();

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

        // time a packet is captured.
        // should be logged with DateTime.UtcNow
        private DateTime packetTime;
        public DateTime PacketTime
        {
            get { return packetTime; }
            set { packetTime = value; }
        }
	}

    /*
        Ethernet frame packet
        */
	public unsafe class EthPacket: Packet 
    {
		public EthPacket(INTERMEDIATE_BUFFER* in_packet) 
        {
			data = in_packet;
		}
		public INTERMEDIATE_BUFFER* data;

		public override bool ContainsLayer(Protocol layer) 
        {
			return (layer == Protocol.Ethernet);
		}

		public override byte* Data() 
        {
			return data->m_IBuffer;
		}

		public override Protocol GetHighestLayer() 
        {
			return Protocol.Ethernet;
		}

		public override uint Length() 
        {
			return data->m_Length;
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
                return new EETHPacket(data).MakeNextLayerPacket();
            }
            else if (isIP())
            {
                return new IPPacket(data).MakeNextLayerPacket();
            }
            else if (isARP())
                return new ARPPacket(data);
            else
                return this;
		}

		public override bool Outbound 
        {
            get
            {
                return (data->m_dwDeviceFlags == Ndisapi.PACKET_FLAG_ON_SEND);
            }
            set
            {
                if (value)
                {
                    data->m_dwDeviceFlags = Ndisapi.PACKET_FLAG_ON_SEND;
                }
                else
                {
                    data->m_dwDeviceFlags = Ndisapi.PACKET_FLAG_ON_RECEIVE;
                }
            }
		}

        // check Type for 0x0806
		public bool isARP() 
        {
            return (data->m_IBuffer[0x0c] == 0x08 && data->m_IBuffer[0x0d] == 0x06);
		}

		public bool isEETH() 
        {
			return (data->m_IBuffer[0x0c] == 0x98 && data->m_IBuffer[0x0d] == 0x09);
		}

		public bool isIP() 
        {
			return (data->m_IBuffer[0x0c] == 0x08 && data->m_IBuffer[0x0d] == 0x00);
		}

		public PhysicalAddress FromMac 
        {
			get 
            {
				byte[] mac = new byte[6];
                for (int x = 0; x < 6; x++)
                    mac[x] = data->m_IBuffer[x + 6];
				return new PhysicalAddress(mac);
			}
            set
            {
                byte[] mac = value.GetAddressBytes();
                for (int x = 0; x < 6; x++)
                    data->m_IBuffer[6 + x] = mac[x];
            }
		}

		public PhysicalAddress ToMac 
        {
			get 
            {
				byte[] mac = new byte[6];
                for (int x = 0; x < 6; x++)
                    mac[x] = data->m_IBuffer[x];
				return new PhysicalAddress(mac);
			}
            set
            {
                byte[] mac = value.GetAddressBytes();
                for (int x = 0; x < 6; x++)
                    data->m_IBuffer[x] = mac[x];
            }
		}
	}
                
    /// <summary>
    /// Encrypted ethernet packet
    /// (Not yet implemented)
    /// </summary>
	public unsafe class EETHPacket: EthPacket 
    {
		public EETHPacket(EthPacket eth): base(eth.data) 
        {
		}

        public EETHPacket(INTERMEDIATE_BUFFER* in_packet)
            : base(in_packet) 
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
    public unsafe class ICMPPacket : IPPacket
    {
        // accepts intermediate buff, checks if ICMP
        public ICMPPacket(INTERMEDIATE_BUFFER* in_packet)
            : base(in_packet) 
        {
			if (!isICMP())
				throw new Exception("Not an ICMP packet!");
            start = base.LayerStart() + base.LayerLength();
		}

        // accepts IPPacket, checks if ICMP
        public ICMPPacket(IPPacket eth)
            : base(eth.data)
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
            return (data->m_IBuffer[start + 1]).ToString();
        }

        // return ICMP type
        public string getType()
        {
            return (data->m_IBuffer[start]).ToString();
        }
    }

    /*
        ARP object
        */
	public unsafe class ARPPacket: EthPacket 
    {
        public ARPPacket(EthPacket eth)
            : base(eth.data) 
        {
			if (!isARP())
				throw new Exception("Not an ARP packet!");
            start = base.LayerStart() + base.LayerLength();
            length = this.Length() - base.LayerLength();
		}

        public ARPPacket(INTERMEDIATE_BUFFER* in_packet)
            : base(in_packet) 
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
                return (data->m_IBuffer[start + 6] == 0x00 && data->m_IBuffer[start + 7] == 0x01);
            }
            set
            {
                if (value)
                {
                    data->m_IBuffer[start + 6] = 0x00;
                    data->m_IBuffer[start + 7] = 0x01;
                }
                else
                {
                    data->m_IBuffer[start + 6] = 0x00;
                    data->m_IBuffer[start + 7] = 0x02;
                }
            }
		}

		public IPAddress ASenderIP 
        {
			get 
            {
				byte[] ip = new byte[4];
                for (int x = 0; x < 4; x++)
                    ip[x] = data->m_IBuffer[start + 0xe + x];
				return new IPAddress(ip);
			}
            set
            {
                byte[] ip = value.GetAddressBytes();
                for (int x = 0; x < 4; x++)
                    data->m_IBuffer[start + 0xe + x] = ip[x];
            }
		}

		public PhysicalAddress ASenderMac 
        {
			get 
            {
				byte[] ip = new byte[6];
                for (int x = 0; x < 6; x++)
                    ip[x] = data->m_IBuffer[start + 0x8 + x];
				return new PhysicalAddress(ip);
			}
            set
            {
                byte[] ip = value.GetAddressBytes();
                for (int x = 0; x < 6; x++)
                    data->m_IBuffer[start + 0x8 + x] = ip[x];
            }
		}

		public IPAddress ATargetIP 
        {
			get 
            {
				byte[] ip = new byte[4];
                for (int x = 0; x < 4; x++)
                    ip[x] = data->m_IBuffer[start + 0x18 + x];
				return new IPAddress(ip);
			}
            set
            {
                byte[] ip = value.GetAddressBytes();
                for (int x = 0; x < 4; x++)
                    data->m_IBuffer[start + 0x18 + x] = ip[x];
            }
		}

		public PhysicalAddress ATargetMac 
        {
			get
            {
				byte[] ip = new byte[6];
                for (int x = 0; x < 6; x++)
                    ip[x] = data->m_IBuffer[start + 0x12 + x];
				return new PhysicalAddress(ip);
			}
            set
            {
                byte[] ip = value.GetAddressBytes();
                for (int x = 0; x < 6; x++)
                    data->m_IBuffer[start + 0x12 + x] = ip[x];
            }
		}
	}

    /*
        IP Packet object
        */
	public unsafe class IPPacket: EthPacket 
    {
        public IPPacket(EthPacket eth)
            : base(eth.data) 
        {
			if (!isIP())
				throw new Exception("Not an IP packet!");
            start = base.LayerStart() + base.LayerLength();
            length = (uint)((data->m_IBuffer[start] & 0xf) * 4);
		}

        public IPPacket(INTERMEDIATE_BUFFER* in_packet)
            : base(in_packet) 
        {
			if (!isIP())
				throw new Exception("Not an IP packet!");
            start = base.LayerStart() + base.LayerLength();
            length = (uint)((data->m_IBuffer[start] & 0xf) * 4);
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
                return new TCPPacket(data).MakeNextLayerPacket();
            }
            else if (isUDP())
                return new UDPPacket(data).MakeNextLayerPacket();
            else if (isICMP())
                return new ICMPPacket(data).MakeNextLayerPacket();
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
            return (data->m_IBuffer[start + 0x9] == 0x06);
		}

		public bool isUDP() 
        {
            return (data->m_IBuffer[start + 0x9] == 0x11);
		}

        public bool isICMP()
        {
            return (data->m_IBuffer[start + 0x9] == 0x01);
        }

		public IPAddress DestIP 
        {
			get 
            {
				if (IPVersion == 0x4)
				{
					byte[] ip = new byte[4];
                    for (int x = 0; x < 4; x++)
                    {
                        ip[x] = data->m_IBuffer[start + 0x10 + x];
                    }
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
				return (byte)(data->m_IBuffer[start] >> 4);
			}
		}

		public IPAddress SourceIP 
        {
			get 
            {
				if (IPVersion == 0x4)
				{
					byte[] ip = new byte[4];
                    for (int x = 0; x < 4; x++)
                    {
                        ip[x] = data->m_IBuffer[start + 0xc + x];
                    }
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
	public unsafe class UDPPacket: IPPacket 
    {
        public UDPPacket(INTERMEDIATE_BUFFER* in_packet)
            : base(in_packet) 
        {
			if (!isUDP())
				throw new Exception("Not a UDP packet!");
            start = base.LayerStart() + base.LayerLength();
		}

        public UDPPacket(IPPacket eth)
            : base(eth.data) 
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
                return new DNSPacket(data).MakeNextLayerPacket();
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

        // check if the UDP packet has an empty header.
        // This is usually the case with port scans.
        public bool isEmpty()
        {
            return ((data->m_IBuffer[start+8] << 8) == 0x00);
        }
		
        // check if the packet is a UDP DNS packet
        public bool isDNS() 
        {
			return (SourcePort == 53 || DestPort == 53);
		}

		public ushort DestPort 
        {
			get 
            {
                return (ushort)((data->m_IBuffer[start + 2] << 8) + data->m_IBuffer[start + 3]);
			}
		}

		public ushort SourcePort 
        {
			get 
            {
                return (ushort)((data->m_IBuffer[start] << 8) + data->m_IBuffer[start + 1]);
			}
		}
	}

        /*
            DNS Packet object
         */
		public unsafe class DNSPacket: UDPPacket 
        {
            uint start = 0;
            uint length = 0;

            public DNSPacket(INTERMEDIATE_BUFFER* in_packet)
                : base(in_packet)
            {
				if (!isDNS())
					throw new Exception("Not a DNS packet!");
                start = base.LayerStart() + base.LayerLength();
                length = Length() - start;
			}

            public DNSPacket(UDPPacket eth)
                : base(eth.data)
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
                    return (data->m_IBuffer[start] & 0x80) == 0x80;
                }
                set
                {
                    if (value)
                    {
                        data->m_IBuffer[start] = (byte)(data->m_IBuffer[start] | 0x80);
                    }
                    else
                    {
                        if ((data->m_IBuffer[start] & 0x80) == 0x80)
                            data->m_IBuffer[start] -= 0x80;
                    }
                }
            }

            public ushort QuestionCount
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

            public override uint LayerStart()
            {
                return start;
            }

            public override uint LayerLength()
            {
                return length;
            }

			public override bool ContainsLayer(Protocol layer) {
				if (layer == Protocol.DNS)
						return true;
				else
						return base.ContainsLayer(layer);
			}

			public override Protocol GetHighestLayer() {
				return Protocol.DNS;
			}

			public override Packet MakeNextLayerPacket() {
				return this;
			}
		}

        /*
            TCP Packet object
         */
		public unsafe class TCPPacket: IPPacket 
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
                    return ((data->m_IBuffer[start + 13] & 0x10) == 0x10);
				}
			}

			public ushort DestPort 
            {
				get 
                {
                    return (ushort)((data->m_IBuffer[start + 2] << 8) + data->m_IBuffer[start + 3]);
				}
			}

			public bool FinSet 
            {
				get 
                {
                    return ((data->m_IBuffer[start + 13] & 0x01) == 0x01);
				}
			}

			public bool PshSet 
            {
				get 
                {
                    return ((data->m_IBuffer[start + 13] & 0x08) == 0x08);
				}
			}

			public bool RstSet 
            {
				get 
                {
                    return ((data->m_IBuffer[start + 13] & 0x04) == 0x04);
				}
			}

			public ushort SourcePort 
            {
				get 
                {
                    return (ushort)((data->m_IBuffer[start] << 8) + data->m_IBuffer[start + 1]);
				}
			}

			public bool SynSet 
            {
				get 
                {
                    return ((data->m_IBuffer[start + 13] & 0x02) == 0x02);
				}
			}
		}
}
