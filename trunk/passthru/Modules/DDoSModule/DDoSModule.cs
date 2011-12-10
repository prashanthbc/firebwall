using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using FM;

namespace PassThru
{
    /*
     * DDoS Protection Module object
     * Implements specific techniques to help mitigate DDoS.  This includes:
     * 
     * - SYN flood protection and flow control
     * - UDP echos (fraggle)
     * - ICMP echo replies (smurf)
     * 
     * @note: this module WILL NOT stop a DDoS; it will only assist in mitigating it.
     * Proper DDoS protection should be implemented at a higher level (ie. hardware or better yet ISP)
     */
    public class DDoSModule : FirewallModule
    {
        private Dictionary<IPAddress, int> ipcache = new Dictionary<IPAddress, int>();
        private TCPPacket TCPprevious_packet;
        private ICMPPacket ICMPprevious_packet;

        // constructor 
        public DDoSModule(NetworkAdapter adapter)
            : base(adapter)
        {
            moduleName = "DDoS Protection";
        }

        // return local user control
        public override System.Windows.Forms.UserControl GetControl()
        {
            return new DDoSDisplay(this) { Dock = System.Windows.Forms.DockStyle.Fill };
        }

        // Action for ModuleStart
        public override ModuleError ModuleStart()
        {
            LoadConfig();
            if (PersistentData == null)
                data = new DDoSData();
            else
                data = (DDoSData)PersistentData;

            ModuleError moduleError = new ModuleError();
            moduleError.errorType = ModuleErrorType.Success;
            return moduleError;
        }

        // Action for ModuleError
        public override ModuleError ModuleStop()
        {
            if (!data.Save)
                data = new DDoSData();

            PersistentData = data;
            SaveConfig();
            ModuleError moduleError = new ModuleError();
            moduleError.errorType = ModuleErrorType.Success;
            return moduleError;
        }

        [Serializable]
        public class DDoSData
        {
            private List<BlockedIP> blockcache = new List<BlockedIP>();
            public List<BlockedIP> BlockCache { get { return blockcache; } set { blockcache = value; } }

            public bool Save = true;
        }

        public DDoSData data;

        // main routine
        public override PacketMainReturn interiorMain(ref Packet in_packet)
        {
            PacketMainReturn pmr;

            // check it the packet is, or contains, IP
            if (in_packet.ContainsLayer(Protocol.IP))
            {
                // create a temp IPPacket obj and
                // check the IP address
                IPPacket temp = (IPPacket)in_packet;
                if (!(isIPAllowed(temp.SourceIP)))
                {
                    pmr = new PacketMainReturn("DDoS Module");
                    pmr.returnType = PacketMainReturnType.Drop;
                    return pmr;
                }
            }

            // simple sanity check to dump the ipcache if it gets too large.
            // this does not effect the blockcache of banned IPs
            if ((ipcache.Count) > 200)
                ipcache.Clear();

            // TCP incoming packets
            if (in_packet.GetHighestLayer() == Protocol.TCP)
            {
                TCPPacket packet = ((TCPPacket)in_packet);
                packet.PacketTime = DateTime.UtcNow;

                // if it's inbound and the SYN flag is set
                if (!packet.Outbound && packet.SynSet && !packet.AckSet)
                {
                    // first packet init
                    if (TCPprevious_packet == null)
                        TCPprevious_packet = packet;

                    // if the IP hasn't been logged yet 
                    if (!(ipcache.ContainsKey(packet.SourceIP)))
                        ipcache.Add(packet.SourceIP, 1);
                    // if the ipcache contains the ip
                    else if (ipcache.ContainsKey(packet.SourceIP))
                    {
                        // increment the packet count if they're coming in fast
                        if ( (packet.PacketTime.Millisecond - TCPprevious_packet.PacketTime.Millisecond) <= 1)
                            ipcache[packet.SourceIP] = (ipcache[packet.SourceIP])+1;

                        // check if this packet = previous, if the packet count is > 50, 
                        // and if the time between sent packets is less than a second
                        if (packet.SourceIP.Equals(TCPprevious_packet.SourceIP) &&
                            ((ipcache[packet.SourceIP]) > 50) &&
                            (packet.PacketTime.Millisecond - TCPprevious_packet.PacketTime.Millisecond) <= 1)
                        {
                            pmr = new PacketMainReturn("DDoS Module");
                            pmr.returnType = PacketMainReturnType.Drop | PacketMainReturnType.Log;
                            pmr.logMessage = "DoS attempt detected from IP " + packet.SourceIP + " (likely spoofed). "
                                        + " Packets from this IP will be dropped.  You can unblock this IP from the module interface.";
                            data.BlockCache.Add(new BlockedIP(packet.SourceIP, DateTime.UtcNow, "DoS Attempt"));
                            return pmr;
                        }

                        // to prevent IP packets for gradually building up, reset packet count when the packets arrive 
                        // later than a typical DoS attempt.  
                        else if (packet.SourceIP.Equals(TCPprevious_packet.SourceIP) &&
                                  (packet.PacketTime.Millisecond - TCPprevious_packet.PacketTime.Millisecond) >= 5)
                        {
                            ipcache[packet.SourceIP] = 1;
                        }
                    }
                    TCPprevious_packet = packet;
                }
            }

            // fraggle attack mitigation
            if (in_packet.GetHighestLayer() == Protocol.UDP)
            {
                UDPPacket packet = ((UDPPacket)in_packet);
                packet.PacketTime = DateTime.UtcNow;

                // if it's inbound
                if (!(packet.Outbound))
                {
                    // add IP to cache or increment packet count
                    if (!(ipcache.ContainsKey(packet.SourceIP)))
                        ipcache.Add(packet.SourceIP, 1);
                    else
                        ipcache[packet.SourceIP] = (ipcache[packet.SourceIP])+1;

                    // if the packet header is empty, headed towards port (7,13,19,17), and count > 50,
                    // then it's probably a fraggle attack
                    if (packet.isEmpty() && packet.DestPort.Equals(7) || packet.DestPort.Equals(13) ||
                         packet.DestPort.Equals(19) || packet.DestPort.Equals(17) &&
                         (ipcache[packet.SourceIP]) > 50 )
                    {
                        pmr = new PacketMainReturn("DDoS Module");
                        pmr.returnType = PacketMainReturnType.Drop | PacketMainReturnType.Log;
                        pmr.logMessage = "Potential fraggle attack from " + packet.SourceIP + " (likely spoofed). "
                            + " Packets from this IP will be dropped.  You can unblock this IP from the module interface.";
                        data.BlockCache.Add(new BlockedIP(packet.SourceIP, DateTime.UtcNow, "Fraggle Attempt"));
                        return pmr;
                    }
                }
            }
                
            // smurf attack mitigation
            if (in_packet.GetHighestLayer() == Protocol.ICMP)
            {
                ICMPPacket packet = ((ICMPPacket)in_packet);
                packet.PacketTime = DateTime.UtcNow;

                if (!(packet.Outbound))
                {
                    // init the previous packet
                    if (ICMPprevious_packet == null)
                        ICMPprevious_packet = packet;

                    // add IP to cache or increment packet count
                    if (!(ipcache.ContainsKey(packet.SourceIP)))
                        ipcache.Add(packet.SourceIP, 1);
                    // if the packet is 5ms after the previous and it's the same packet, clear up the cache
                    else if ((packet.PacketTime.Millisecond - ICMPprevious_packet.PacketTime.Millisecond) >= 5 &&
                                packet.Equals(ICMPprevious_packet))
                        ipcache[packet.SourceIP] = 1;
                    // if the packet is coming in quickly, add it to the packet count
                    else if ( (packet.PacketTime.Millisecond - ICMPprevious_packet.PacketTime.Millisecond) <= 1)
                        ipcache[packet.SourceIP] = (ipcache[packet.SourceIP]) + 1;

                    // if the packet is an echo reply and the IP source
                    // is the same as localhost and the time between packets is <= 1 and
                    // there are over 50 accumulated packets, it's probably a smurf attack
                    if ( packet.getType().Equals("0") &&
                         packet.getCode().Equals("0") &&
                         packet.SourceIP.Equals(getLocalIP()) &&
                         (packet.PacketTime.Millisecond - ICMPprevious_packet.PacketTime.Millisecond) <= 1 &&
                         ipcache[packet.SourceIP] > 50)
                    {
                        pmr = new PacketMainReturn("DDoS Module");
                        pmr.returnType = PacketMainReturnType.Drop | PacketMainReturnType.Log;
                        pmr.logMessage = "Potential Smurf attack from " + packet.SourceIP + " (likely spoofed). "
                            + " Packets from this IP will be dropped.  You can unblock this IP from the module interface.";
                        data.BlockCache.Add(new BlockedIP(packet.SourceIP, DateTime.UtcNow, "Smurf Attempt"));
                        return pmr;
                    }
                    ICMPprevious_packet = packet;
                }
            }

            return null;
        }

        /// <summary>
        /// Determine whether a given IP address is blocked
        /// </summary>
        /// <param name="i">IP address to resolve</param>
        /// <returns>bool</returns>
        private bool isIPAllowed(IPAddress i)
        {
            bool isAllowed = true;
            foreach ( BlockedIP l in data.BlockCache )
            {
                if (l.Blockedip.Equals(i))
                {
                    isAllowed = false;
                    break;
                }
            }
            return isAllowed;
        }

        /// <summary>
        /// Returns local IP address
        /// </summary>
        /// <returns>IPAddress</returns>
        private IPAddress getLocalIP()
        {
            IPAddress local = null;
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    local = ip;
                }
            }
            return local;
        }
    }

    // object used for storing information regarding a blocked IP
    [Serializable]
    public class BlockedIP
    {
        private IPAddress blockedip;
        public IPAddress Blockedip
        {
            get { return blockedip; }
            set { blockedip = value; }
        }

        private DateTime dateblocked;
        public DateTime DateBlocked
        {
            get { return dateblocked; }
            set { dateblocked = value; }
        }

        private string reason;
        public string Reason
        {
            get { return reason; }
            set { reason = value; }
        }

        // constructor with vars
        public BlockedIP(IPAddress b, DateTime d, string s)
        {
            this.blockedip = b;
            this.dateblocked = d;
            this.reason = s;
        }

        // def init
        public BlockedIP()
        {
            blockedip = null;
            dateblocked = DateTime.Now;
            reason = "";
        }
    }
}
