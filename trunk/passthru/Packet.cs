using System;
using System.Collections.Generic;
using System.Text;
using NdisapiSpace;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Net;

namespace PassThru
{
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

		public interface Packet {
			bool ContainsLayer(Protocol layer);

			byte[] Data();

			Protocol GetHighestLayer();

			uint Length();

			Packet MakeNextLayerPacket();

			bool Outbound();
		}

		public class EthPacket: System.Object, Packet {
			public EthPacket(INTERMEDIATE_BUFFER in_packet) {
				data = in_packet;
			}
			public INTERMEDIATE_BUFFER data;

			public virtual bool ContainsLayer(Protocol layer) {
				return (layer == Protocol.Ethernet);
			}

			public byte[] Data() {
				return data.m_IBuffer;
			}

			public virtual Protocol GetHighestLayer() {
				return Protocol.Ethernet;
			}

			public uint Length() {
				return data.m_Length;
			}

			public virtual Packet MakeNextLayerPacket() {
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

			public bool Outbound() {
				return (data.m_dwDeviceFlags == Ndisapi.PACKET_FLAG_ON_SEND);
			}

			public bool isARP() {
				return (data.m_IBuffer[0x0c] == 0x08 && data.m_IBuffer[0x0d] == 0x06);
			}

			public bool isEETH() {
				return (data.m_IBuffer[0x0c] == 0x98 && data.m_IBuffer[0x0d] == 0x09);
			}

			public bool isIP() {
				return (data.m_IBuffer[0x0c] == 0x08 && data.m_IBuffer[0x0d] == 0x00);
			}

			public PhysicalAddress FromMac {
				get {
					byte[] mac = new byte[6];
					Buffer.BlockCopy(data.m_IBuffer, 6, mac, 0, 6);
					return new PhysicalAddress(mac);
				}
			}

			public PhysicalAddress ToMac {
				get {
					byte[] mac = new byte[6];
					Buffer.BlockCopy(data.m_IBuffer, 0, mac, 0, 6);
					return new PhysicalAddress(mac);
				}
			}
		}

		public class EETHPacket: EthPacket {
			public EETHPacket(EthPacket eth): base(eth.data) {
			}

			public EETHPacket(INTERMEDIATE_BUFFER in_packet): base(in_packet) {
			}

			public override bool ContainsLayer(Protocol layer) {
				if (layer == Protocol.EEth)
						return true;
				else
						return base.ContainsLayer(layer);
			}

			public override Protocol GetHighestLayer() {
				return Protocol.EEth;
			}

			public override Packet MakeNextLayerPacket() {
				return this;
			}
		}

		public class ARPPacket: EthPacket {
			public ARPPacket(EthPacket eth): base(eth.data) {
				if (!isARP())
						throw new Exception("Not an ARP packet!");
			}

			public ARPPacket(INTERMEDIATE_BUFFER in_packet): base(in_packet) {
				if (!isARP())
						throw new Exception("Not an ARP packet!");
			}

			public override bool ContainsLayer(Protocol layer) {
				if (layer == Protocol.ARP)
						return true;
				else
						return base.ContainsLayer(layer);
			}

			public override Protocol GetHighestLayer() {
				return Protocol.ARP;
			}

			public override Packet MakeNextLayerPacket() {
				return this;
			}

			public bool isRequest() {
				return (data.m_IBuffer[0x14] == 0x00 && data.m_IBuffer[0x15] == 0x01);
			}

			public IPAddress ASenderIP {
				get {
					byte[] ip = new byte[4];
					Buffer.BlockCopy(data.m_IBuffer, 0x1c, ip, 0, 4);
					return new IPAddress(ip);
				}
			}

			public PhysicalAddress ASenderMac {
				get {
					byte[] ip = new byte[6];
					Buffer.BlockCopy(data.m_IBuffer, 0x16, ip, 0, 6);
					return new PhysicalAddress(ip);
				}
			}

			public IPAddress ATargetIP {
				get {
					byte[] ip = new byte[4];
					Buffer.BlockCopy(data.m_IBuffer, 0x26, ip, 0, 4);
					return new IPAddress(ip);
				}
			}

			public PhysicalAddress ATargetMac {
				get {
					byte[] ip = new byte[6];
					Buffer.BlockCopy(data.m_IBuffer, 0x20, ip, 0, 6);
					return new PhysicalAddress(ip);
				}
			}
		}

		public class IPPacket: EthPacket {
			public IPPacket(EthPacket eth): base(eth.data) {
				if (!isIP())
						throw new Exception("Not an IP packet!");
			}

			public IPPacket(INTERMEDIATE_BUFFER in_packet): base(in_packet) {
				if (!isIP())
						throw new Exception("Not an IP packet!");
			}

			public override bool ContainsLayer(Protocol layer) {
				if (layer == Protocol.IP)
						return true;
				else
						return base.ContainsLayer(layer);
			}

			public override Protocol GetHighestLayer() {
				return Protocol.IP;
			}

			public override Packet MakeNextLayerPacket() {
				if (isTCP())
				{
						return new TCPPacket(data).MakeNextLayerPacket();
				}
				else if (isUDP())
						return new UDPPacket(data).MakeNextLayerPacket();
				else
						return this;
			}

			public bool isTCP() {
				return (data.m_IBuffer[0x17] == 0x06);
			}

			public bool isUDP() {
				return (data.m_IBuffer[0x17] == 0x11);
			}

			public IPAddress DestIP {
				get {
					if (IPVersion == 0x4)
					{
							byte[] ip = new byte[4];
							Buffer.BlockCopy(data.m_IBuffer, 0x1e, ip, 0, 4);
							return new IPAddress(ip);
					}
					else
							return null;
				}
			}

			public byte IPVersion {
				get {
					return (byte)(data.m_IBuffer[0x0e] >> 4);
				}
			}

			public IPAddress SourceIP {
				get {
					if (IPVersion == 0x4)
					{
							byte[] ip = new byte[4];
							Buffer.BlockCopy(data.m_IBuffer, 0x1a, ip, 0, 4);
							return new IPAddress(ip);
					}
					else
							return null;
				}
			}
		}

		public class UDPPacket: IPPacket {
			public UDPPacket(INTERMEDIATE_BUFFER in_packet): base(in_packet) {
				if (!isUDP())
						throw new Exception("Not a UDP packet!");
			}

			public UDPPacket(IPPacket eth): base(eth.data) {
				if (!isUDP())
						throw new Exception("Not a UDP packet!");
			}

			public override bool ContainsLayer(Protocol layer) {
				if (layer == Protocol.UDP)
						return true;
				else
						return base.ContainsLayer(layer);
			}

			public override Protocol GetHighestLayer() {
				return Protocol.UDP;
			}

			public override Packet MakeNextLayerPacket() {
				if (isDNS())
				{
						return new DNSPacket(data).MakeNextLayerPacket();
				}
				return this;
			}

			public bool isDNS() {
				return (SourcePort == 53 || DestPort == 53);
			}

			public ushort DestPort {
				get {
					return (ushort)((data.m_IBuffer[0x24] << 8) + data.m_IBuffer[0x25]);
				}
			}

			public ushort SourcePort {
				get {
					return (ushort)((data.m_IBuffer[0x22] << 8) + data.m_IBuffer[0x23]);
				}
			}
		}

		public class DNSPacket: UDPPacket {
			public DNSPacket(INTERMEDIATE_BUFFER in_packet): base(in_packet) {
				if (!isDNS())
						throw new Exception("Not a DNS packet!");
			}

			public DNSPacket(UDPPacket eth): base(eth.data) {
				if (!isDNS())
						throw new Exception("Not a DNS packet!");
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

		public class TCPPacket: IPPacket {
			public TCPPacket(INTERMEDIATE_BUFFER in_packet): base(in_packet) {
				if (!isTCP())
						throw new Exception("Not a TCP packet!");
			}

			public TCPPacket(IPPacket eth): base(eth.data) {
				if (!isTCP())
						throw new Exception("Not a TCP packet!");
			}

			public override bool ContainsLayer(Protocol layer) {
				if (layer == Protocol.TCP)
						return true;
				else
						return base.ContainsLayer(layer);
			}

			public override Protocol GetHighestLayer() {
				return Protocol.TCP;
			}

			public override Packet MakeNextLayerPacket() {
				return this;
			}

			public bool AckSet {
				get {
					return ((data.m_IBuffer[0x2f] & 0x10) == 0x10);
				}
			}

			public ushort DestPort {
				get {
					return (ushort)((data.m_IBuffer[0x24] << 8) + data.m_IBuffer[0x25]);
				}
			}

			public bool FinSet {
				get {
					return ((data.m_IBuffer[0x2f] & 0x01) == 0x01);
				}
			}

			public bool PshSet {
				get {
					return ((data.m_IBuffer[0x2f] & 0x08) == 0x08);
				}
			}

			public bool RstSet {
				get {
					return ((data.m_IBuffer[0x2f] & 0x04) == 0x04);
				}
			}

			public ushort SourcePort {
				get {
					return (ushort)((data.m_IBuffer[0x22] << 8) + data.m_IBuffer[0x23]);
				}
			}

			public bool SynSet {
				get {
					return ((data.m_IBuffer[0x2f] & 0x02) == 0x02);
				}
			}
		}
}
