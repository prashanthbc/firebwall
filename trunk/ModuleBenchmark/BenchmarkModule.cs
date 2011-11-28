using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;

namespace ModuleBenchmark
{
    class BenchmarkModule
    {


        public BenchmarkModule()
        {

        }

        public virtual void Initiate()
        {

        }

        public virtual void InteriorMain(ref Packet in_packet)
        {

        }

        public virtual void Close()
        {

        }

        public TimeSpan Benchmark(List<Packet> packets)
        {            
            DateTime start = DateTime.Now;
            Initiate();
            for(int x = 0; x < packets.Count; x++)
            {
                Packet p = packets[x];
                InteriorMain(ref p);
            }
            Close();
            return DateTime.Now - start;
        }
    }

    class BasicFirewall : BenchmarkModule
    {
        public enum PacketStatus
        {
            UNDETERMINED,
            BLOCKED,
            ALLOWED
        }

        public enum Direction
        {
            IN = 1,
            OUT = 1 << 1
        }

        public interface Rule
        {
            PacketStatus GetStatus(Packet pkt);
            string ToFileString();
            string GetLogMessage();
        }

        public class UDPAllRule : Rule
        {
            PacketStatus ps;
            Direction direction;
            bool log = true;            

            public UDPAllRule(PacketStatus ps, Direction direction, bool log)
            {
                this.ps = ps;
                this.direction = direction;
                this.log = log;
            }

            public PacketStatus GetStatus(Packet pkt)
            {
                if (pkt.ContainsLayer(Protocol.UDP))
                {
                    if (pkt.Outbound && (direction & Direction.OUT) == Direction.OUT)
                    {
                        if (log)
                            message = " UDP packet from " + ((UDPPacket)pkt).SourceIP.ToString() + ":" + ((UDPPacket)pkt).SourcePort.ToString() + " to " + ((UDPPacket)pkt).DestIP.ToString() + ":" + ((UDPPacket)pkt).DestPort.ToString();
                        return ps;
                    }
                    else if (!pkt.Outbound && (direction & Direction.IN) == Direction.IN)
                    {
                        if (log)
                            message = " UDP packet from " + ((UDPPacket)pkt).SourceIP.ToString() + ":" + ((UDPPacket)pkt).SourcePort.ToString() + " to " + ((UDPPacket)pkt).DestIP.ToString() + ":" + ((UDPPacket)pkt).DestPort.ToString();
                        return ps;
                    }
                }
                return PacketStatus.UNDETERMINED;
            }

            string message = null;
            public string GetLogMessage()
            {
                if (!log) return null;
                if (ps == PacketStatus.ALLOWED)
                {
                    return "Allowed " + message;
                }
                return "Blocked " + message;
            }

            public string ToFileString()
            {
                return null;
            }
        }

        public class UDPPortRule : Rule
        {
            PacketStatus ps;
            Direction direction;
            int port;
            bool log = true;     

            public UDPPortRule(PacketStatus ps, int port, Direction direction, bool log)
            {
                this.ps = ps;
                this.direction = direction;
                this.port = port;
                this.log = log;
            }

            public PacketStatus GetStatus(Packet pkt)
            {
                if (pkt.ContainsLayer(Protocol.UDP))
                {
                    if (pkt.Outbound && (direction & Direction.OUT) == Direction.OUT)
                    {
                        if (((UDPPacket)pkt).DestPort == port)
                        {
                            if (log)
                                message = " UDP packet from " + ((UDPPacket)pkt).SourceIP.ToString() + ":" + ((UDPPacket)pkt).SourcePort.ToString() + " to " + ((UDPPacket)pkt).DestIP.ToString() + ":" + ((UDPPacket)pkt).DestPort.ToString();
                            return ps;
                        }
                    }
                    else if (!pkt.Outbound && (direction & Direction.IN) == Direction.IN)
                    {
                        if (((UDPPacket)pkt).DestPort == port)
                        {
                            if (log)
                                message = " UDP packet from " + ((UDPPacket)pkt).SourceIP.ToString() + ":" + ((UDPPacket)pkt).SourcePort.ToString() + " to " + ((UDPPacket)pkt).DestIP.ToString() + ":" + ((UDPPacket)pkt).DestPort.ToString();
                            return ps;
                        }
                    }
                }
                return PacketStatus.UNDETERMINED;
            }

            string message = null;
            public string GetLogMessage()
            {
                if (!log)
                    return null;
                if (ps == PacketStatus.ALLOWED)
                {
                    return "Allowed " + message;
                }
                return "Blocked " + message;
            }

            public string ToFileString()
            {
                return null;
            }
        }

        public class TCPIPPortRule : Rule
        {
            PacketStatus ps;
            Direction direction;
            int port;
            IPAddress ip;
            bool log = true;     

            public TCPIPPortRule(PacketStatus ps, IPAddress ip, int port, Direction direction, bool log)
            {
                this.ps = ps;
                this.direction = direction;
                this.port = port;
                this.ip = ip;
                this.log = log;
            }

            public PacketStatus GetStatus(Packet pkt)
            {
                if (pkt.ContainsLayer(Protocol.TCP))
                {
                    if (pkt.Outbound && (direction & Direction.OUT) == Direction.OUT)
                    {
                        if (((TCPPacket)pkt).DestPort == port && ((TCPPacket)pkt).DestIP == ip)
                        {
                            if (log)
                                message = " TCP packet from " + ((TCPPacket)pkt).SourceIP.ToString() + ":" + ((TCPPacket)pkt).SourcePort.ToString() + " to " + ((TCPPacket)pkt).DestIP.ToString() + ":" + ((TCPPacket)pkt).DestPort.ToString();
                            return ps;
                        }
                    }
                    else if (!pkt.Outbound && (direction & Direction.IN) == Direction.IN)
                    {
                        if (((TCPPacket)pkt).DestPort == port && ((TCPPacket)pkt).DestIP == ip)
                        {
                            if (log)
                                message = " TCP packet from " + ((TCPPacket)pkt).SourceIP.ToString() + ":" + ((TCPPacket)pkt).SourcePort.ToString() + " to " + ((TCPPacket)pkt).DestIP.ToString() + ":" + ((TCPPacket)pkt).DestPort.ToString();
                            return ps;
                        }
                    }
                }
                return PacketStatus.UNDETERMINED;
            }

            string message = null;
            public string GetLogMessage()
            {
                if (!log)
                    return null;
                if (ps == PacketStatus.ALLOWED)
                {
                    return "Allowed " + message;
                }
                return "Blocked " + message;
            }

            public string ToFileString()
            {
                return null;
            }
        }

        public class TCPPortRule : Rule
        {
            PacketStatus ps;
            Direction direction;
            int port;
            bool log = true;     

            public TCPPortRule(PacketStatus ps, int port, Direction direction, bool log)
            {
                this.ps = ps;
                this.direction = direction;
                this.port = port;
                this.log = log;
            }

            public PacketStatus GetStatus(Packet pkt)
            {
                if (pkt.ContainsLayer(Protocol.TCP))
                {
                    if (pkt.Outbound && (direction & Direction.OUT) == Direction.OUT)
                    {
                        if (((TCPPacket)pkt).DestPort == port)
                        {
                            if (log)
                                message = " TCP packet from " + ((TCPPacket)pkt).SourceIP.ToString() + ":" + ((TCPPacket)pkt).SourcePort.ToString() + " to " + ((TCPPacket)pkt).DestIP.ToString() + ":" + ((TCPPacket)pkt).DestPort.ToString();
                            return ps;
                        }
                    }
                    else if (!pkt.Outbound && (direction & Direction.IN) == Direction.IN)
                    {
                        if (((TCPPacket)pkt).DestPort == port)
                        {
                            if (log)
                                message = " TCP packet from " + ((TCPPacket)pkt).SourceIP.ToString() + ":" + ((TCPPacket)pkt).SourcePort.ToString() + " to " + ((TCPPacket)pkt).DestIP.ToString() + ":" + ((TCPPacket)pkt).DestPort.ToString();
                            return ps;
                        }
                    }
                }
                return PacketStatus.UNDETERMINED;
            }

            string message = null;
            public string GetLogMessage()
            {
                if (!log)
                    return null;
                if (ps == PacketStatus.ALLOWED)
                {
                    return "Allowed " + message;
                }
                return "Blocked " + message;
            }

            public string ToFileString()
            {
                return null;
            }
        }

        public class TCPAllRule : Rule
        {
            PacketStatus ps;
            Direction direction;
            bool log = true;     

            public TCPAllRule(PacketStatus ps, Direction direction, bool log)
            {
                this.ps = ps;
                this.direction = direction;
                this.log = log;
            }

            public PacketStatus GetStatus(Packet pkt)
            {
                if (pkt.ContainsLayer(Protocol.TCP))
                {
                    if (pkt.Outbound && (direction & Direction.OUT) == Direction.OUT)
                    {
                        if (log)
                            message = " TCP packet from " + ((TCPPacket)pkt).SourceIP.ToString() + ":" + ((TCPPacket)pkt).SourcePort.ToString() + " to " + ((TCPPacket)pkt).DestIP.ToString() + ":" + ((TCPPacket)pkt).DestPort.ToString();
                        return ps;
                    }
                    else if (!pkt.Outbound && (direction & Direction.IN) == Direction.IN)
                    {
                        if (log)
                            message = " TCP packet from " + ((TCPPacket)pkt).SourceIP.ToString() + ":" + ((TCPPacket)pkt).SourcePort.ToString() + " to " + ((TCPPacket)pkt).DestIP.ToString() + ":" + ((TCPPacket)pkt).DestPort.ToString();
                        return ps;
                    }
                }
                return PacketStatus.UNDETERMINED;
            }

            string message = null;
            public string GetLogMessage()
            {
                if (!log)
                    return null;
                if (ps == PacketStatus.ALLOWED)
                {
                    return "Allowed " + message;
                }
                return "Blocked " + message;
            }

            public string ToFileString()
            {
                return null;
            }
        }

        public class MacRule : Rule
        {
            PacketStatus ps;
            PhysicalAddress mac;
            Direction direction;
            bool log = true;     

            public MacRule(PacketStatus ps, PhysicalAddress mac, Direction direction, bool log)
            {
                this.ps = ps;
                this.mac = mac;
                this.direction = direction;
                this.log = log;
            }

            public PacketStatus GetStatus(Packet pkt)
            {
                if (pkt.Outbound && (direction & Direction.OUT) == Direction.OUT)
                {
                    if (mac.Equals(((EthPacket)pkt).ToMac) || mac == null)
                    {
                        if (log)
                            message = "packet from " + ((EthPacket)pkt).FromMac.ToString() + " to " + ((EthPacket)pkt).ToMac.ToString();
                        return ps;
                    }
                }
                else if (!pkt.Outbound && (direction & Direction.IN) == Direction.IN)
                {
                    if (mac.Equals(((EthPacket)pkt).FromMac) || mac == null)
                    {
                        if(log)
                            message = "packet from " + ((EthPacket)pkt).FromMac.ToString() + " to " + ((EthPacket)pkt).ToMac.ToString();
                        return ps;
                    }
                }
                return PacketStatus.UNDETERMINED;
            }

            string message = null;
            public string GetLogMessage()
            {
                if (!log)
                    return null;
                if (ps == PacketStatus.ALLOWED)
                {
                    return "Allowed " + message;
                }
                return "Blocked " + message;
            }

            public string ToFileString()
            {
                return null;
            }
        }

        readonly object padlock = new object();
        public List<Rule> rules = new List<Rule>();

        public BasicFirewall()
        {
        }

        public override void Initiate()
        {
            //rules.Add(new Rule("3:UDP:-1;-1;-1;-1;:-1:True:True:No UDP:::BLOCKED"));
            //rules.Add(new Rule("1:TCP:-1;-1;-1;-1;:42552:False:False::::ALLOWED"));            
            rules.Add(new UDPPortRule(PacketStatus.ALLOWED, 42552, Direction.IN, false));
            rules.Add(new UDPAllRule(PacketStatus.BLOCKED, Direction.IN | Direction.OUT, false));
            rules.Add(new TCPAllRule(PacketStatus.BLOCKED, Direction.IN, false));
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
                return (srcIP == other.srcIP && srcPort == other.srcPort &&
                        dstIP == other.dstIP && dstPort == other.dstPort) ||
                        (srcIP == other.dstIP && srcPort == other.dstPort &&
                        dstIP == other.srcIP && dstPort == other.srcPort);
            }

            public override int GetHashCode()
            {
                return srcIP.GetHashCode() + dstIP.GetHashCode();
            }
        }

        List<Quad> tcpConnections = new List<Quad>();

        public override void InteriorMain(ref Packet in_packet)
        {
            if (in_packet.ContainsLayer(Protocol.TCP) && !((TCPPacket)in_packet).SynSet)
            {
                return;
            }
            lock (padlock)
            {
                PacketStatus status = PacketStatus.UNDETERMINED;
                foreach (Rule r in rules)
                {
                    status = r.GetStatus(in_packet);
                    if (status == PacketStatus.BLOCKED)
                    {
                        return;
                    }
                    else if (status == PacketStatus.ALLOWED)
                    {
                        if (in_packet.ContainsLayer(Protocol.TCP) && ((TCPPacket)in_packet).SynSet && ((TCPPacket)in_packet).AckSet)
                        {
                            tcpConnections.Add(MakeQuad(in_packet));
                        }
                        return;
                    }
                }
            }
            return;
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

        public override void Close()
        {
            base.Close();
        }
    }

    class OldBasicFirewall : BenchmarkModule
    {
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

        public OldBasicFirewall()
        {
        }

        public override void Initiate()
        {            
            rules.Add(new Rule("1:UDP:-1;-1;-1;-1;:42552:False:False::::ALLOWED"));
            rules.Add(new Rule("3:UDP:-1;-1;-1;-1;:-1:True:True:No UDP:::BLOCKED"));
            rules.Add(new Rule("3:TCP:-1;-1;-1;-1;:-1:True:True:No Incoming TCP Connections:::BLOCKED"));
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
                return (srcIP == other.srcIP && srcPort == other.srcPort &&
                        dstIP == other.dstIP && dstPort == other.dstPort) ||
                        (srcIP == other.dstIP && srcPort == other.dstPort &&
                        dstIP == other.srcIP && dstPort == other.srcPort);
            }

            public override int GetHashCode()
            {
                return srcIP.GetHashCode() + dstIP.GetHashCode();
            }
        }

        List<Quad> tcpConnections = new List<Quad>();

        public override void InteriorMain(ref Packet in_packet)
        {
            if (in_packet.ContainsLayer(Protocol.TCP) && !((TCPPacket)in_packet).SynSet)
            {
                return;
            }
            lock (padlock)
            {
                PacketStatus status = PacketStatus.UNDETERMINED;
                foreach (Rule r in rules)
                {
                    status = r.GetStatus(in_packet);
                    if (status == PacketStatus.BLOCKED)
                    {
                        return;
                    }
                    else if (status == PacketStatus.ALLOWED)
                    {
                        if (in_packet.ContainsLayer(Protocol.TCP) && ((TCPPacket)in_packet).SynSet && ((TCPPacket)in_packet).AckSet)
                        {
                            tcpConnections.Add(MakeQuad(in_packet));
                        }
                        return;
                    }
                }
            }
            return;
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

        public override void Close()
        {
            base.Close();
        }
    }
}
