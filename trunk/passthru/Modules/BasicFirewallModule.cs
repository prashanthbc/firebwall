using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace PassThru
{
    /*
     * Basic Firewall Module
     * @summary
     */
    public class BasicFirewall : FirewallModule
    {
        public BasicFirewall(NetworkAdapter adapter)
            : base(adapter)
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
                    if (!string.IsNullOrEmpty(ipsplit[x]))
                        IP[x] = short.Parse(ipsplit[x]);
                }

                Port = int.Parse(split[3]);

                Log = bool.Parse(split[4]);

                Notify = bool.Parse(split[5]);

                Name = split[6];

                string[] incoming = split[7].Split(';');
                foreach (string i in incoming)
                {
                    if (!string.IsNullOrEmpty(i))
                        doNotNotifyIncoming.Add(int.Parse(i));
                }

                string[] outgoing = split[8].Split(';');
                foreach (string o in outgoing)
                {
                    if (!string.IsNullOrEmpty(o))
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
                if ((pkt.Outbound && (Direction & (1 << 1)) == (1 << 1)) || (!pkt.Outbound && (Direction & 0x1) == 0x1))
                {
                    if (pkt.ContainsLayer(protocol))
                    {
                        if (transport == Protocol.TCP && pkt.ContainsLayer(Protocol.TCP))
                        {
                            TCPPacket tcp = (TCPPacket)pkt;
                            if ((pkt.Outbound && (Direction & (1 << 1)) == (1 << 1)) && tcp.SynSet && !tcp.AckSet)
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
                            else if ((!pkt.Outbound && (Direction & 0x1) == 0x1) && tcp.SynSet && !tcp.AckSet)
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
                            if (pkt.Outbound)
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
                            else if (!pkt.Outbound)
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

        readonly object padlock = new object();
        public List<Rule> rules = new List<Rule>();

        public override ModuleError ModuleStart()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            folder = folder + Path.DirectorySeparatorChar + "firebwall";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            folder = folder + Path.DirectorySeparatorChar + "modules";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            folder = folder + Path.DirectorySeparatorChar + "configs";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

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
            return new RuleEditor(this) { Dock = System.Windows.Forms.DockStyle.Fill };
        }

        public override ModuleError ModuleStop()
        {
            lock (padlock)
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                folder = folder + Path.DirectorySeparatorChar + "firebwall";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                folder = folder + Path.DirectorySeparatorChar + "modules";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                folder = folder + Path.DirectorySeparatorChar + "configs";
                if (!Directory.Exists(folder))
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

        public override PacketMainReturn interiorMain(ref Packet in_packet)
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
                        PacketMainReturn pmr = new PacketMainReturn("Basic Firewall");
                        pmr.returnType = PacketMainReturnType.Drop;
                        if (r.Log && r.Message != null)
                        {
                            pmr.returnType |= PacketMainReturnType.Log;
                            pmr.logMessage = r.Message;
                            Location l = null;
                            if (in_packet.Outbound)
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
}
