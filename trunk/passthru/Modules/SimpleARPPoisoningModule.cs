using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace PassThru
{
    /*
         * ARP Poisoning module.
         * @summary: //TODO 
         */
    public class SimpleAntiARPPoisoning : FirewallModule
    {
        public SimpleAntiARPPoisoning(NetworkAdapter adapter)
            : base(adapter)
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
}
