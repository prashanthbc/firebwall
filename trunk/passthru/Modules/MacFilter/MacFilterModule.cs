using System;
using System.Collections.Generic;
using System.Text;
using System.Net.NetworkInformation;
using FM;

namespace PassThru.Modules.MacFilter
{
    public class MacFilterModule : FirewallModule
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

        [Serializable]
        public class MacRule
        {
            PacketStatus ps;
            string mac;
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
                if (mac != null)
                    ret += " MAC " + mac.ToString();
                else
                    ret += " all MACs ";
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

            public MacRule(PacketStatus ps, Direction direction, bool log)
            {
                this.ps = ps;
                this.mac = null;
                this.direction = direction;
                this.log = log;
            }

            public MacRule(PacketStatus ps, PhysicalAddress mac, Direction direction, bool log)
            {
                this.ps = ps;
                this.mac = mac.ToString();
                this.direction = direction;
                this.log = log;
            }

            public PacketStatus GetStatus(Packet pkt)
            {
                if (pkt.Outbound && (direction & Direction.OUT) == Direction.OUT)
                {
                    if (mac == null || mac.Equals(((EthPacket)pkt).ToMac.ToString()))
                    {
                        if (log)
                            message = "packet from " + ((EthPacket)pkt).FromMac.ToString() + " to " + ((EthPacket)pkt).ToMac.ToString();
                        return ps;
                    }
                }
                else if (!pkt.Outbound && (direction & Direction.IN) == Direction.IN)
                {
                    if (mac == null || mac.Equals(((EthPacket)pkt).FromMac.ToString()))
                    {
                        if (log)
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

        public MacFilterModule(NetworkAdapter adapter)
            : base(adapter)
        {
            moduleName = "MacFilter";
        }

        readonly object padlock = new object();
        public List<MacRule> rules = new List<MacRule>();

        public override ModuleError ModuleStart()
        {
            LoadConfig();
            lock (padlock)
            {
                if (PersistentData != null)
                {
                    rules = (List<MacRule>)PersistentData;
                }
                else
                    rules = new List<MacRule>();
            }

            ModuleError me = new ModuleError();
            me.errorType = ModuleErrorType.Success;
            return me;
        }

        public override ModuleError ModuleStop()
        {
            PersistentData = rules;
            SaveConfig();
            ModuleError me = new ModuleError();
            me.errorType = ModuleErrorType.Success;
            return me;
        }

        public override System.Windows.Forms.UserControl GetControl()
        {
            return new MacFilterControl(this) { Dock = System.Windows.Forms.DockStyle.Fill };
        }

        public override PacketMainReturn interiorMain(ref Packet in_packet)
        {
            lock (padlock)
            {
                PacketStatus status = PacketStatus.UNDETERMINED;
                foreach (MacRule r in rules)
                {
                    status = r.GetStatus(in_packet);
                    if (status == PacketStatus.BLOCKED)
                    {
                        PacketMainReturn pmr = new PacketMainReturn(moduleName);
                        pmr.returnType = PacketMainReturnType.Drop;
                        if (r.GetLogMessage() != null)
                        {
                            pmr.returnType |= PacketMainReturnType.Log;
                            pmr.logMessage = r.GetLogMessage();
                        }
                        return pmr;
                    }
                    else if (status == PacketStatus.ALLOWED)
                    {
                        PacketMainReturn pmr = new PacketMainReturn(moduleName);
                        pmr.returnType = PacketMainReturnType.Allow;
                        return pmr;
                    }
                }
            }
            PacketMainReturn pa = new PacketMainReturn(moduleName);
            pa.returnType = PacketMainReturnType.Allow;
            return pa;
        }

        public void InstanceGetRuleUpdates(List<MacRule> r)
        {
            lock (padlock)
            {
                rules = new List<MacRule>(r);
            }
        }
    }
}
