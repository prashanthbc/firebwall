using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;

namespace PassThru
{
	[Flags]
	public enum PacketMainReturnType
	{
		Error,          //Reports an error in the packet processing
		Drop,           //Drops the packet
		Allow,          //Allows the packet to be passed on to the next module
		SendOutPacket,  //Requires out packet to be sent
		Log	            //Logs the packet
	}

	public class PacketMainReturn
    {
		/// <summary>
		/// Creates a PacketMainReturn with the basic unknown error message
		/// </summary>
		/// <param name="moduleName"></param>
		public PacketMainReturn(string moduleName) 
        {
			Module = moduleName;
			returnType = PacketMainReturnType.Error | PacketMainReturnType.Log;
			logMessage = "An error has occurred in " + moduleName + " with no other details.";
		}
        public PacketMainReturn(string moduleName, Exception e)
        {
            Module = moduleName;
            returnType = PacketMainReturnType.Error | PacketMainReturnType.Log;
            logMessage = "An error has occurred in " + moduleName + ". " + e.Message + "\r\n" + e.StackTrace;
        }
		public string Module = null;
		public byte[] SendPacket = null;
		public string logMessage = null;
		public PacketMainReturnType returnType;
	}

	public enum ModuleErrorType
	{
		Success,        //No error
		UnknownError    //I'm not sure what type of errors it'll run into yet
	}

	public class ModuleError
    {
		public byte[] errorBinary = null;
		public string errorMessage = null;
		public ModuleErrorType errorType;
		public string moduleName = null;
	}

	/// <summary>
	/// An abstract class for the firewall modules, making input and output uniform
	/// </summary>
	public abstract class FirewallModule
    {
		public FirewallModule(NetworkAdapter adapter) 
        {
			this.adapter = adapter;
		}
		public NetworkAdapter adapter = null;
		public System.Windows.Forms.UserControl uiControl = null;
		public string moduleName = null;

        public virtual System.Windows.Forms.UserControl GetControl()
        {
            return null;
        }

		/// <summary>
		/// Ran after the module is loaded, to prime it for processing if required
		/// </summary>
		/// <returns>Any error that occured during the starting of it</returns>
		public abstract ModuleError ModuleStart();

		/// <summary>
		/// Ran when the module is to be stopped, to clear up any uneeded resources
		/// </summary>
		/// <returns>Any error that occured during the stopping of it</returns>
		public abstract ModuleError ModuleStop();

		/// <summary>
		/// The wrapper function for processing packets
		/// </summary>
		/// <param name="in_packet">Packet to be processed</param>
		/// <returns>A PacketMainReturn object, either from the interiorMain or default error one</returns>
		public PacketMainReturn PacketMain(Packet in_packet) {
			try
			{
				PacketMainReturn pmr = interiorMain(in_packet);
				return pmr;
			}
			catch (Exception e)
			{
				return new PacketMainReturn(moduleName, e);
			}
		}

		/// <summary>
		/// The internal function for processing packets implemented by the module
		/// </summary>
		/// <param name="in_packet">Packet to be processed</param>
		/// <returns>PacketMainReturn object describing what to do with the packet and/or
		/// anything that is notable during the processing</returns>
		public abstract PacketMainReturn interiorMain(Packet in_packet);
	}

	public class Quad
    {
		public IPAddress dstIP = null;
		public int dstPort = -1;
		public IPAddress srcIP = null;
		public int srcPort = -1;

		public override bool Equals(object obj) 
        {
			Quad other = (Quad)obj;
			return (srcIP == other.srcIP && srcPort == other.srcPort && dstIP == other.dstIP && dstPort == other.dstPort) || (srcIP == other.dstIP && srcPort == other.dstPort && dstIP == other.srcIP && dstPort == other.srcPort);
		}

		public override int GetHashCode() 
        {
			return srcIP.GetHashCode() + dstIP.GetHashCode();
		}
	}

	public class BasicFirewall: FirewallModule 
    {
		public BasicFirewall(NetworkAdapter adapter):base(adapter) 
        {
            moduleName = "Basic Firewall";
		}

		public enum PacketStatus
		{
			UNDETERMINED,
			BLOCKED,
			ALLOWED
		}

		public class Rule
        {
            public List<int> doNotNotifyIncoming = new List<int>();
            public List<int> doNotNotifyOutgoing = new List<int>();

			public Rule(string fromFile) 
            {
				//Direction:Transport:IPShorts:Port:Log:Notify:Name:doNotNotifyIncoming;:doNotNotifyOutgoing;:Set
				string[] split = fromFile.Split(':');

				Direction = (byte)(0 + int.Parse(split[0]));

				transport = split[1] == "TCP" ? Protocol.TCP : Protocol.UDP;

				string[] ipsplit = split[2].Split(';');
				IP = new short[ipsplit.Length - 1];
				for (int x = 0; x < ipsplit.Length; x++)
				{
					if(!string.IsNullOrEmpty(ipsplit[x]))
						IP[x] = short.Parse(ipsplit[x]);
				}

				Port = int.Parse(split[3]);

				Log = bool.Parse(split[4]);

				Notify = bool.Parse(split[5]);

				Name = split[6];

				string[] incoming = split[7].Split(';');
				foreach (string i in incoming)
				{
					if(!string.IsNullOrEmpty(i))
						doNotNotifyIncoming.Add(int.Parse(i));
				}

				string[] outgoing = split[8].Split(';');
				foreach (string o in outgoing)
				{
					if(!string.IsNullOrEmpty(o))
						doNotNotifyOutgoing.Add(int.Parse(o));
				}

				if (split[9] == "ALLOWED")
					Set = PacketStatus.ALLOWED;
				else
					Set = PacketStatus.BLOCKED;
			}

			public Rule(PacketStatus ps, int port, Protocol transport, bool log, bool notify, string name, byte dir) 
            {
				Set = ps;
				Port = port;
				this.transport = transport;
				protocol = transport;
				this.Log = log;
				this.Notify = notify;
				this.Name = name;
				this.Direction = dir;
				this.IP[0] = -1;
				this.IP[1] = -1;
				this.IP[2] = -1;
				this.IP[3] = -1;
			}
			public byte Direction = 0x00;
			public short[] IP = new short[4];
			public bool Log = false;
			public string Message = null;
			public string Name = null;
			public bool Notify = false;
			public int Port = 0;
			public PacketStatus Set = PacketStatus.BLOCKED;

			//1 for inbound 1 << 1 for outbound
			public Protocol protocol = Protocol.Ethernet;
			public Protocol transport = Protocol.TCP;

			public PacketStatus GetStatus(Packet pkt) 
            {
				if ((pkt.Outbound() && (Direction & (1 << 1)) == (1 << 1)) || (!pkt.Outbound() && (Direction & 0x1) == 0x1))
				{
					if (pkt.ContainsLayer(protocol))
					{
						if (transport == Protocol.TCP && pkt.ContainsLayer(Protocol.TCP))
						{
							TCPPacket tcp = (TCPPacket)pkt;
							if ((pkt.Outbound() && (Direction & (1 << 1)) == (1 << 1)) && tcp.SynSet && !tcp.AckSet)
							{
								if ((CheckPort(tcp.DestPort) == Set) && CheckIP(tcp.DestIP) == Set)
								{
									Message = null;
									if (!doNotNotifyOutgoing.Contains(tcp.DestPort))
									{
										if (Set == PacketStatus.BLOCKED)
											Message = "Blocked";
										else
											Message = "Allowed";
										Message += string.Format(" TCP connection: {0}:{1} -> {2}:{3}", tcp.SourceIP, tcp.SourcePort, tcp.DestIP, tcp.DestPort);
									}
									return Set;
								}
							}
							else if((!pkt.Outbound() && (Direction & 0x1) == 0x1) && tcp.SynSet && !tcp.AckSet)
							{
								if ((CheckPort(tcp.DestPort) == Set) && CheckIP(tcp.SourceIP) == Set)
								{
									Message = null;
									if (!doNotNotifyIncoming.Contains(tcp.DestPort))
									{
										if (Set == PacketStatus.BLOCKED)
											Message = "Blocked";
										else
											Message = "Allowed";
										Message += " TCP connection: " + tcp.SourceIP.ToString() + ":" + tcp.SourcePort.ToString() + " -> " + tcp.DestIP.ToString() + ":" + tcp.DestPort.ToString();
									}
									return Set;
								}
							}
						}
						else if (transport == Protocol.UDP && pkt.ContainsLayer(Protocol.UDP))
						{
							UDPPacket tcp = (UDPPacket)pkt;
							if (pkt.Outbound())
							{
								if (((CheckPort(tcp.DestPort) == Set && (Direction & 0x2) == 0x2) || (CheckPort(tcp.SourcePort) == Set && (Direction & 0x1) == 0x1)) && CheckIP(tcp.DestIP) == Set)
								{
									Message = null;
									if (!doNotNotifyOutgoing.Contains(tcp.DestPort))
									{
										if (Set == PacketStatus.BLOCKED)
											Message = "Blocked";
										else
											Message = "Allowed";
										Message += " UDP connection: " + tcp.SourceIP.ToString() + ":" + tcp.SourcePort.ToString() + " -> " + tcp.DestIP.ToString() + ":" + tcp.DestPort.ToString();
									}
									return Set;
								}
							}
							else if(!pkt.Outbound())
							{
								if (((CheckPort(tcp.DestPort) == Set && (Direction & 0x1) == 0x1) || (CheckPort(tcp.SourcePort) == Set && (Direction & 0x2) == 0x2)) && CheckIP(tcp.DestIP) == Set)
								{
									Message = null;
									if (!doNotNotifyIncoming.Contains(tcp.DestPort))
									{
										if (Set == PacketStatus.BLOCKED)
											Message = "Blocked";
										else
											Message = "Allowed";
										Message += " UDP connection: " + tcp.SourceIP.ToString() + ":" + tcp.SourcePort.ToString() + " -> " + tcp.DestIP.ToString() + ":" + tcp.DestPort.ToString();
									}
									return Set;
								}
							}
						}
					}
				}
				return PacketStatus.UNDETERMINED;
			}

			public string ToFileString() 
            {
				string ret = "";
				//Direction:Transport:IPShorts:Port:Log:Notify:Name:doNotNotifyIncoming;:doNotNotifyOutgoing;:Set
				ret += ((int)Direction).ToString() + ":";

				switch (transport)
				{
					case Protocol.TCP:
						ret += "TCP:";
						break;
					case Protocol.UDP:
						ret += "UDP:";
						break;
					default:
						ret += ":";
						break;
				}

				foreach (short a in IP)
					ret += a.ToString() + ";";
				ret += ":";

				ret += Port.ToString() + ":";

				ret += Log.ToString() + ":";

				ret += Notify.ToString() + ":";

				if (Name != null)
					ret += Name;
				ret += ":";

				foreach (int p in doNotNotifyIncoming)
				{
					ret += p.ToString() + ";";
				}
				ret += ":";

				foreach (int p in doNotNotifyOutgoing)
				{
					ret += p.ToString() + ";";
				}
				ret += ":";

				if (Set == PacketStatus.ALLOWED)
					ret += "ALLOWED";
				else
					ret += "BLOCKED";

				return ret;
			}

			public override string ToString() 
            {
				return Message;
			}

			PacketStatus CheckIP(IPAddress i) 
            {
				byte[] b = i.GetAddressBytes();

				if (IP[0] == -1 && IP[1] == -1 && IP[2] == -1 && IP[3] == -1)
					return Set;

				for (int x = 0; x < 4; x++)
				{
					if (IP[x] == -1)
						continue;
					if (IP[x] != b[x])
						return PacketStatus.UNDETERMINED;
				}

				return Set;
			}

			PacketStatus CheckPort(ushort p) 
            {
				if (Port == -1 || Port == p)
					return Set;
				return PacketStatus.UNDETERMINED;
			}
		}
		PcapFileWriter file;
		readonly object padlock = new object();
        public List<Rule> rules = new List<Rule>();

		public override ModuleError ModuleStart() 
        {
			//RuleUpdater.Instance.GetRuleUpdates +=new RuleUpdater.GR(InstanceGetRuleUpdates);
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            folder = folder + Path.DirectorySeparatorChar + "firebwall";
            if(!Directory.Exists(folder))
                Directory.CreateDirectory(folder);            
			string f = folder + Path.DirectorySeparatorChar + "blocked-" + this.adapter.InterfaceInformation.Name + "-" + PcapCreator.Instance.GetNewDate() + ".pcap";
			file = new PcapFileWriter(f);

			lock (padlock)
			{
                if (File.Exists(folder + Path.DirectorySeparatorChar + this.adapter.InterfaceInformation.Name + "BasicFirewallRules.conf"))
                {
                    string[] lines = File.ReadAllLines(folder + Path.DirectorySeparatorChar + this.adapter.InterfaceInformation.Name + "BasicFirewallRules.conf");
                    foreach (string line in lines)
                    {
                        rules.Add(new Rule(line));
                    }
                }
                else
                {
                    rules.Add(new Rule("1:TCP:-1;-1;-1;-1;:-1:True:True:No Incoming TCP Connections:::BLOCKED"));
                }
			}

			ModuleError me = new ModuleError();
			me.errorType = ModuleErrorType.Success;
			return me;
		}

        public override System.Windows.Forms.UserControl GetControl()
        {
            return new RuleEditor(this){Dock = System.Windows.Forms.DockStyle.Fill};
        }

		public override ModuleError ModuleStop() 
        {
			file.Close();
			lock (padlock)
			{
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                folder = folder + Path.DirectorySeparatorChar + "firebwall";
                if(!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                File.Create(folder + Path.DirectorySeparatorChar + this.adapter.InterfaceInformation.Name + "BasicFirewallRules.conf").Close();
				foreach (Rule r in rules)
				{
                    File.AppendAllText(folder + Path.DirectorySeparatorChar + this.adapter.InterfaceInformation.Name + "BasicFirewallRules.conf", r.ToFileString() + "\r\n");
				}
			}
			ModuleError me = new ModuleError();
			me.errorType = ModuleErrorType.Success;
			return me;
		}

        List<Quad> tcpConnections = new List<Quad>();

		public override PacketMainReturn interiorMain(Packet in_packet) 
        {
			if (in_packet.ContainsLayer(Protocol.TCP) && !((TCPPacket)in_packet).SynSet)
			{
				PacketMainReturn p = new PacketMainReturn("Basic Firewall");
				p.returnType = PacketMainReturnType.Allow;
				return p;
			}
			lock (padlock)
			{
				PacketStatus status = PacketStatus.UNDETERMINED;
				foreach (Rule r in rules)
				{
					status = r.GetStatus(in_packet);
					if (status == PacketStatus.BLOCKED)
					{
						byte[] data = new byte[in_packet.Length()];
						Buffer.BlockCopy(in_packet.Data(), 0, data, 0, data.Length);
						file.AddPacket(data);
						PacketMainReturn pmr = new PacketMainReturn("Basic Firewall");
						pmr.returnType = PacketMainReturnType.Drop;
						if (r.Log && r.Message != null)
						{
							pmr.returnType |= PacketMainReturnType.Log;
							pmr.logMessage = r.Message;
							Location l = null;
							if (in_packet.Outbound())
								l = Program.ls.getLocation(((IPPacket)in_packet).DestIP);
							else
								l = Program.ls.getLocation(((IPPacket)in_packet).SourceIP);
							if (l != null)
								pmr.logMessage += "\r\nThis IP originates from " + l.city + ", " + l.regionName + ", " + l.countryName;
						}
                        //if (r.Notify && r.Message != null)
                        //{
                        //    string m = r.Message;
                        //    Location l = null;
                        //    if (in_packet.Outbound())
                        //        l = Program.ls.getLocation(((IPPacket)in_packet).DestIP);
                        //    else
                        //        l = Program.ls.getLocation(((IPPacket)in_packet).SourceIP);
                        //    if (l != null)
                        //        m += "\r\nThis IP originates from " + l.city + ", " + l.regionName + ", " + l.countryName;
                        //    Program.tray.AddLine(m);
                        //}
						return pmr;
					}
					else if (status == PacketStatus.ALLOWED)
					{
						if (in_packet.ContainsLayer(Protocol.TCP) && ((TCPPacket)in_packet).SynSet && ((TCPPacket)in_packet).AckSet)
						{
							tcpConnections.Add(MakeQuad(in_packet));
						}
						PacketMainReturn pmr = new PacketMainReturn("Basic Firewall");
						pmr.returnType = PacketMainReturnType.Allow;
						return pmr;
					}
				}
			}
			PacketMainReturn pa = new PacketMainReturn("Basic Firewall");
			pa.returnType = PacketMainReturnType.Allow;
			return pa;
		}

		public void InstanceGetRuleUpdates(List<Rule> r) 
        {
			lock (padlock)
			{
				rules = new List<Rule>(r);
			}
		}

		Quad MakeQuad(Packet in_packet) 
        {
			if (in_packet.ContainsLayer(Protocol.TCP))
			{
				TCPPacket tcp = (TCPPacket)in_packet;
				Quad q = new Quad
							 {
								 dstIP = tcp.DestIP,
								 dstPort = tcp.DestPort,
								 srcPort = tcp.SourcePort,
								 srcIP = tcp.SourceIP
							 };
				return q;
			}
			return null;
		}
	}

	public class EthEncryption: FirewallModule 
    {
		public EthEncryption(NetworkAdapter adapter): base(adapter) 
        {
		}

		public override ModuleError ModuleStart() 
        {
			throw new NotImplementedException();
		}

		public override ModuleError ModuleStop() 
        {
			throw new NotImplementedException();
		}

		public override PacketMainReturn interiorMain(Packet in_packet)
        {
			throw new NotImplementedException();
		}
	}

	public class SimpleAntiARPPoisoning: FirewallModule 
    {
		public SimpleAntiARPPoisoning(NetworkAdapter adapter): base(adapter) 
        {
            this.moduleName = "Arp Poisoning Protection";
		}

		public override ModuleError ModuleStart() 
        {
			ModuleError me = new ModuleError();
			me.errorType = ModuleErrorType.Success;
			return me;
		}

		public override ModuleError ModuleStop() 
        {
			ModuleError me = new ModuleError();
			me.errorType = ModuleErrorType.Success;
			return me;
		}

        List<int> requestedIPs = new List<int>();
        Dictionary<IPAddress, PhysicalAddress> arpCache = new Dictionary<IPAddress, PhysicalAddress>();

        public event System.Threading.ThreadStart UpdatedArpCache;
        object padlock = new object();

        public Dictionary<IPAddress, PhysicalAddress> GetCache()
        {
            lock (padlock)
            {
                return new Dictionary<IPAddress, PhysicalAddress>(arpCache);
            }
        }

        public void UpdateCache(Dictionary<IPAddress, PhysicalAddress> cache)
        {
            lock (padlock)
            {
                arpCache = new Dictionary<IPAddress, PhysicalAddress>(cache);
            }
        }

        public override System.Windows.Forms.UserControl GetControl()
        {
            return new ArpPoisoningProtection(this) { Dock = System.Windows.Forms.DockStyle.Fill };
        }

		public override PacketMainReturn interiorMain(Packet in_packet) 
        {
			if (in_packet.GetHighestLayer() == Protocol.ARP)
			{
				if (((ARPPacket)in_packet).isRequest() && in_packet.Outbound())
				{
					int ip = ((ARPPacket)in_packet).ATargetIP.GetHashCode();
					if (!requestedIPs.Contains(ip))
						requestedIPs.Add(ip);
				}
				else if (!in_packet.Outbound() && !((ARPPacket)in_packet).isRequest())
				{
					int ip = ((ARPPacket)in_packet).ASenderIP.GetHashCode();
					if (requestedIPs.Contains(ip))
					{
                        lock (padlock)
                        {
                            if (arpCache.ContainsKey(((ARPPacket)in_packet).ASenderIP))
                            {
                                if (!arpCache[((ARPPacket)in_packet).ASenderIP].Equals(((ARPPacket)in_packet).ASenderMac))
                                {
                                    PacketMainReturn pmr = new PacketMainReturn("Simple ARP Poisoning Protection");
                                    pmr.returnType = PacketMainReturnType.Drop | PacketMainReturnType.Log;
                                    pmr.logMessage = "ARP Response from " + ((ARPPacket)in_packet).ASenderMac.ToString() + " for " + ((ARPPacket)in_packet).ASenderIP.ToString() + " does not match the ARP cache.";
                                    return pmr;
                                }
                                else
                                {
                                    requestedIPs.Remove(ip);
                                }
                            }
                            else
                            {
                                arpCache[((ARPPacket)in_packet).ASenderIP] = ((ARPPacket)in_packet).ASenderMac;
                                if (UpdatedArpCache != null)
                                    UpdatedArpCache();
                                requestedIPs.Remove(ip);
                            }
                        }
					}
					else
					{
						PacketMainReturn pmr = new PacketMainReturn("Simple ARP Poisoning Protection");
						pmr.returnType = PacketMainReturnType.Drop | PacketMainReturnType.Log;
						pmr.logMessage = "Unsolicited ARP Response from " + ((ARPPacket)in_packet).ASenderMac.ToString() + " for " + ((ARPPacket)in_packet).ASenderIP.ToString();
						return pmr;
					}
				}
			}
			PacketMainReturn p = new PacketMainReturn("Simple ARP Poisoning Protection");
			p.returnType = PacketMainReturnType.Allow;
			return p;
		}
	}

	public class DumpToPcapModule: FirewallModule 
    {
		public DumpToPcapModule(NetworkAdapter adapter): base(adapter) {
		}
		PcapFileWriter file;

		public override ModuleError ModuleStart() {
			string f = "pcaptest-" + PcapCreator.Instance.GetNewDate() + ".pcap";
			file = new PcapFileWriter(f);
			ModuleError me = new ModuleError();
			me.errorType = ModuleErrorType.Success;
			return me;
		}

		public override ModuleError ModuleStop() {
			file.Close();
			ModuleError me = new ModuleError();
			me.errorType = ModuleErrorType.Success;
			return me;
		}

		public override PacketMainReturn interiorMain(Packet in_packet) {
			byte[] buffer = new byte[in_packet.Length()];
			Buffer.BlockCopy(in_packet.Data(), 0, buffer, 0, (int)in_packet.Length());
			file.AddPacket(buffer);
			PacketMainReturn pmr = new PacketMainReturn("Dump to Pcap");
			pmr.returnType = PacketMainReturnType.Allow;
			return pmr;
		}
	}

	/*class TestFirewallModule : FirewallModule
	{
		public override PacketMainReturn interiorMain(Packet in_packet)
		{
			if (!in_packet.Outbound() && in_packet.Length() > 0x2f)
			{
				if (in_packet.GetHighestLayer() == Protocol.TCP)
				{
					TCPPacket tcp = (TCPPacket)in_packet;
					if (tcp.SynSet && !tcp.AckSet)
					{
						PacketMainReturn pmr = new PacketMainReturn("Block Incoming Connections");
						pmr.logMessage = "Blocked Incoming TCP Connection from " + tcp.SourceIP.ToString() + " to port " + tcp.DestPort.ToString();
						pmr.returnType = PacketMainReturnType.Drop | PacketMainReturnType.Log;
						return pmr;
					}
				}
			}

			PacketMainReturn pmrNull = new PacketMainReturn("Block Incoming Connections");
			pmrNull.returnType = PacketMainReturnType.Allow;
			return pmrNull;

		}

		public override ModuleError ModuleStart()
		{
			ModuleError me = new ModuleError();
			me.errorType = ModuleErrorType.Success;
			return me;
		}

		public override ModuleError ModuleStop()
		{
			ModuleError me = new ModuleError();
			me.errorType = ModuleErrorType.Success;
			return me;
		}
	}*/
}
