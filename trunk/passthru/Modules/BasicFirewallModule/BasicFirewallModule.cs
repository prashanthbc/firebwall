using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
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
            string String
            {
                get;
            }
        }

        public enum RuleType
        {
            UDPALL,
            UDPPORT,
            TCPIPPORT,
            TCPPORT,
            TCPALL,
            ALL
        }

        public class RuleFactory
        {
            public static Rule MakeRule(RuleType ruleType, PacketStatus ps, Direction dir, string args, bool log)
            {
                switch (ruleType)
                {
                    case RuleType.TCPALL:
                        return new TCPAllRule(ps, dir, log);
                    case RuleType.TCPIPPORT:
                        return new TCPIPPortRule(ps, IPAddress.Parse(args.Split(' ')[0]), int.Parse(args.Split(' ')[1]), dir, log);
                    case RuleType.TCPPORT:
                        return new TCPPortRule(ps, int.Parse(args), dir, log);
                    case RuleType.UDPALL:
                        return new UDPAllRule(ps, dir, log);
                    case RuleType.UDPPORT:
                        return new UDPPortRule(ps, int.Parse(args), dir, log);
                    case RuleType.ALL:
                        return new AllRule(ps, dir, log);
                }
                return null;
            }
        }

        [Serializable]
        public class AllRule : Rule
        {
            PacketStatus ps;
            Direction direction;
            bool log = true;

            public AllRule(PacketStatus ps, Direction direction, bool log)
            {
                this.ps = ps;
                this.direction = direction;
                this.log = log;
            }

            public PacketStatus GetStatus(Packet pkt)
            {
                if (pkt.Outbound && (direction & Direction.OUT) == Direction.OUT)
                {
                    if (log)
                        message = " packet";
                    return ps;
                }
                else if (!pkt.Outbound && (direction & Direction.IN) == Direction.IN)
                {
                    if (log)
                        message = " packet";
                    return ps;
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

            public string String
            {
                get { return ToString(); }
            }

            public override string ToString()
            {
                string ret = "";
                if (ps == PacketStatus.ALLOWED)
                {
                    ret = "Allows";
                }
                else
                {
                    ret = "Blocks";
                }
                ret += " all";
                if (direction == (Direction.IN | Direction.OUT))
                {
                    ret += " in and out";
                }
                else if(direction == Direction.OUT)
                {
                    ret += " out";
                }
                else if (direction == Direction.IN)
                {
                    ret += " in";
                }
                if (log)
                    ret += " and logs";
                return ret;
            }

            public string ToFileString()
            {
                return null;
            }
        }

        [Serializable]
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

            public string String
            {
                get { return ToString(); }
            }

            public override string ToString()
            {
                string ret = "";
                if (ps == PacketStatus.ALLOWED)
                {
                    ret = "Allows";
                }
                else
                {
                    ret = "Blocks";
                }
                ret += " all UDP ports";
                if (direction == (Direction.IN | Direction.OUT))
                {
                    ret += " in and out";
                }
                else if(direction == Direction.OUT)
                {
                    ret += " out";
                }
                else if (direction == Direction.IN)
                {
                    ret += " in";
                }
                if (log)
                    ret += " and logs";
                return ret;
            }

            public string ToFileString()
            {
                return null;
            }
        }

        [Serializable]
        public class UDPPortRule : Rule
        {
            PacketStatus ps;
            Direction direction;
            int port;
            bool log = true;

            public override string ToString()
            {
                string ret = "";
                if (ps == PacketStatus.ALLOWED)
                {
                    ret = "Allows";
                }
                else
                {
                    ret = "Blocks";
                }
                ret += " UDP port " + port.ToString();
                if (direction == (Direction.IN | Direction.OUT))
                {
                    ret += " in and out";
                }
                else if (direction == Direction.OUT)
                {
                    ret += " out";
                }
                else if (direction == Direction.IN)
                {
                    ret += " in";
                }
                if (log)
                    ret += " and logs";
                return ret;
            }

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

            public string String
            {
                get { return ToString(); }
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

        [Serializable]
        public class TCPIPPortRule : Rule
        {
            PacketStatus ps;
            Direction direction;
            int port;
            IPAddress ip;
            bool log = true;

            public override string ToString()
            {
                string ret = "";
                if (ps == PacketStatus.ALLOWED)
                {
                    ret = "Allows";
                }
                else
                {
                    ret = "Blocks";
                }
                ret += " TCP ip:port " + ip.ToString() + ":" + port.ToString();
                if (direction == (Direction.IN | Direction.OUT))
                {
                    ret += " in and out";
                }
                else if (direction == Direction.OUT)
                {
                    ret += " out";
                }
                else if (direction == Direction.IN)
                {
                    ret += " in";
                }
                if (log)
                    ret += " and logs";
                return ret;
            }

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
                if (pkt.ContainsLayer(Protocol.TCP) && ((TCPPacket)pkt).SynSet && !((TCPPacket)pkt).AckSet)
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

            public string String
            {
                get { return ToString(); }
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

        [Serializable]
        public class TCPPortRule : Rule
        {
            PacketStatus ps;
            Direction direction;
            int port;
            bool log = true;

            public override string ToString()
            {
                string ret = "";
                if (ps == PacketStatus.ALLOWED)
                {
                    ret = "Allows";
                }
                else
                {
                    ret = "Blocks";
                }
                ret += " TCP port " + port.ToString();
                if (direction == (Direction.IN | Direction.OUT))
                {
                    ret += " in and out";
                }
                else if (direction == Direction.OUT)
                {
                    ret += " out";
                }
                else if (direction == Direction.IN)
                {
                    ret += " in";
                }
                if (log)
                    ret += " and logs";
                return ret;
            }

            public TCPPortRule(PacketStatus ps, int port, Direction direction, bool log)
            {
                this.ps = ps;
                this.direction = direction;
                this.port = port;
                this.log = log;
            }

            public PacketStatus GetStatus(Packet pkt)
            {
                if (pkt.ContainsLayer(Protocol.TCP) && ((TCPPacket)pkt).SynSet && !((TCPPacket)pkt).AckSet)
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

            public string String
            {
                get { return ToString(); }
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

        [Serializable]
        public class TCPAllRule : Rule
        {
            PacketStatus ps;
            Direction direction;
            bool log = true;

            public string String
            {
                get { return ToString(); }
            }

            public override string ToString()
            {
                string ret = "";
                if (ps == PacketStatus.ALLOWED)
                {
                    ret = "Allows";
                }
                else
                {
                    ret = "Blocks";
                }
                ret += " all TCP ports";
                if (direction == (Direction.IN | Direction.OUT))
                {
                    ret += " in and out";
                }
                else if (direction == Direction.OUT)
                {
                    ret += " out";
                }
                else if (direction == Direction.IN)
                {
                    ret += " in";
                }
                if (log)
                    ret += " and logs";
                return ret;
            }

            public TCPAllRule(PacketStatus ps, Direction direction, bool log)
            {
                this.ps = ps;
                this.direction = direction;
                this.log = log;
            }

            public PacketStatus GetStatus(Packet pkt)
            {
                if (pkt.ContainsLayer(Protocol.TCP) && ((TCPPacket)pkt).SynSet && !((TCPPacket)pkt).AckSet)
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

        readonly object padlock = new object();
        public List<Rule> rules = new List<Rule>();

        public override ModuleError ModuleStart()
        {
            LoadConfig();
            lock (padlock)
            {
                if (PersistentData == null)
                {
                    rules.Add(new TCPAllRule(PacketStatus.BLOCKED, Direction.IN, true));
                }
                else
                {
                    rules = (List<Rule>)PersistentData;
                }
            }

            ModuleError me = new ModuleError();
            me.errorType = ModuleErrorType.Success;
            return me;
        }

        public override System.Windows.Forms.UserControl GetControl()
        {
            return new PassThru.Modules.BasicFirewallModule.BasicFirewallControl(this) { Dock = System.Windows.Forms.DockStyle.Fill };
        }

        public override ModuleError ModuleStop()
        {
            PersistentData = rules;
            SaveConfig();
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
                        PacketMainReturn pmr = new PacketMainReturn("Basic Firewall");
                        pmr.returnType = PacketMainReturnType.Drop;
                        if (r.GetLogMessage() != null)
                        {
                            pmr.returnType |= PacketMainReturnType.Log;
                            pmr.logMessage = r.GetLogMessage();
                            Location l = null;
                            if (in_packet.Outbound)
                                l = Program.ls.getLocation(((IPPacket)in_packet).DestIP);
                            else
                                l = Program.ls.getLocation(((IPPacket)in_packet).SourceIP);
                            if (l != null)
                                pmr.logMessage += "\r\nThis IP originates from " + l.city + ", " + l.regionName + ", " + l.countryName;
                        }
                        return pmr;
                    }
                    else if (status == PacketStatus.ALLOWED)
                    {
                        if (in_packet.ContainsLayer(Protocol.TCP) && ((TCPPacket)in_packet).SynSet && !((TCPPacket)in_packet).AckSet)
                        {
                            tcpConnections.Add(MakeQuad(in_packet));
                        }
                        PacketMainReturn pmr = new PacketMainReturn("Basic Firewall");
                        pmr.returnType = PacketMainReturnType.Allow;
                        return pmr;
                    }
                }
                if (in_packet.ContainsLayer(Protocol.TCP) && ((TCPPacket)in_packet).SynSet && !((TCPPacket)in_packet).AckSet)
                {
                    tcpConnections.Add(MakeQuad(in_packet));
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
