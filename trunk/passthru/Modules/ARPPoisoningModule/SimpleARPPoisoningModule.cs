﻿using System;
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
            LoadConfig();
            if (PersistentData == null)
            {
                data = new ArpData();
            }
            else
            {
                data = (ArpData)PersistentData;
            }
            ModuleError me = new ModuleError();
            me.errorType = ModuleErrorType.Success;
            return me;
        }

        public override ModuleError ModuleStop()
        {
            if (!data.Save)
                data.arpCache = new Dictionary<IPAddress, PhysicalAddress>();

            PersistentData = data;
            SaveConfig();
            ModuleError me = new ModuleError();
            me.errorType = ModuleErrorType.Success;
            return me;
        }

        List<int> requestedIPs = new List<int>();

        [Serializable]
        public class ArpData
        {
            public Dictionary<IPAddress, PhysicalAddress> arpCache = new Dictionary<IPAddress, PhysicalAddress>();
            public bool Save = true;
            public bool LogUnsolic = true;
            public bool LogAttacks = true;
        }

        public ArpData data;

        public event System.Threading.ThreadStart UpdatedArpCache;
        object padlock = new object();

        

        public Dictionary<IPAddress, PhysicalAddress> GetCache()
        {
            lock (padlock)
            {
                return new Dictionary<IPAddress, PhysicalAddress>(data.arpCache);
            }
        }

        public void UpdateCache(Dictionary<IPAddress, PhysicalAddress> cache)
        {
            lock (padlock)
            {
                data.arpCache = new Dictionary<IPAddress, PhysicalAddress>(cache);
            }
        }

        public override System.Windows.Forms.UserControl GetControl()
        {
            return new ArpPoisoningProtection(this) { Dock = System.Windows.Forms.DockStyle.Fill };
        }

        public override PacketMainReturn interiorMain(ref Packet in_packet)
        {
            if (in_packet.GetHighestLayer() == Protocol.ARP)
            {
                ARPPacket arpp = (ARPPacket)in_packet;
                if (arpp.isRequest && arpp.Outbound)
                {
                    int ip = arpp.ATargetIP.GetHashCode();
                    if (!requestedIPs.Contains(ip))
                        requestedIPs.Add(ip);
                }
                else if (!arpp.Outbound)
                {
                    int ip = arpp.ASenderIP.GetHashCode();
                    if (!arpp.isRequest)
                    {
                        if (requestedIPs.Contains(ip))
                        {
                            lock (padlock)
                            {
                                if (data.arpCache.ContainsKey(arpp.ASenderIP))
                                {
                                    if (!data.arpCache[arpp.ASenderIP].Equals(arpp.ASenderMac))
                                    {
                                        PacketMainReturn pmr = new PacketMainReturn("Simple ARP Poisoning Protection");
                                        pmr.returnType = PacketMainReturnType.Edited;                                        
                                        if (data.LogAttacks)
                                            pmr.returnType |= PacketMainReturnType.Log;                                        
                                        pmr.logMessage = "ARP Response from " + arpp.ASenderMac.ToString() + " for " + arpp.ASenderIP.ToString() + " does not match the ARP cache.";
                                        arpp.ATargetIP = arpp.ASenderIP;
                                        arpp.ATargetMac = data.arpCache[arpp.ATargetIP];
                                        arpp.ASenderMac = adapter.InterfaceInformation.GetPhysicalAddress();
                                        arpp.FromMac = arpp.ASenderMac;
                                        arpp.ToMac = arpp.ATargetMac;
                                        foreach (UnicastIPAddressInformation ipv4 in adapter.InterfaceInformation.GetIPProperties().UnicastAddresses)
                                        {
                                            if (ipv4.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                            {
                                                arpp.ASenderIP = ipv4.Address;
                                                break;
                                            }
                                        }                                        
                                        arpp.Outbound = true;
                                        in_packet = arpp;
                                        return pmr;
                                    }
                                    else
                                    {
                                        requestedIPs.Remove(ip);
                                    }
                                }
                                else
                                {
                                    data.arpCache[arpp.ASenderIP] = arpp.ASenderMac;
                                    if (UpdatedArpCache != null)
                                        UpdatedArpCache();
                                    requestedIPs.Remove(ip);
                                }
                            }
                        }
                        else
                        {
                            lock (padlock)
                            {
                                if (data.arpCache.ContainsKey(arpp.ASenderIP))
                                {
                                    if (!data.arpCache[arpp.ASenderIP].Equals(arpp.ASenderMac))
                                    {
                                        PacketMainReturn pmra = new PacketMainReturn("Simple ARP Poisoning Protection");
                                        pmra.returnType = PacketMainReturnType.Edited;
                                        if (data.LogAttacks)
                                            pmra.returnType |= PacketMainReturnType.Log;                                        
                                        pmra.logMessage = "ARP Response from " + arpp.ASenderMac.ToString() + " for " + arpp.ASenderIP.ToString() + " does not match the ARP cache.";
                                        arpp.ATargetIP = arpp.ASenderIP;
                                        arpp.ATargetMac = data.arpCache[arpp.ATargetIP];
                                        arpp.ASenderMac = adapter.InterfaceInformation.GetPhysicalAddress();
                                        arpp.FromMac = arpp.ASenderMac;
                                        arpp.ToMac = arpp.ATargetMac;
                                        foreach (UnicastIPAddressInformation ipv4 in adapter.InterfaceInformation.GetIPProperties().UnicastAddresses)
                                        {
                                            if (ipv4.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                            {
                                                arpp.ASenderIP = ipv4.Address;
                                                break;
                                            }
                                        }
                                        arpp.Outbound = true;
                                        in_packet = arpp;
                                        return pmra;
                                    }
                                }
                            }
                            PacketMainReturn pmr = new PacketMainReturn("Simple ARP Poisoning Protection");
                            pmr.returnType = PacketMainReturnType.Drop;
                            if (data.LogUnsolic)
                                pmr.returnType |= PacketMainReturnType.Log;                            
                            pmr.logMessage = "Unsolicited ARP Response from " + arpp.ASenderMac.ToString() + " for " + arpp.ASenderIP.ToString();
                            return pmr;
                        }
                    }
                    else
                    {
                        lock (padlock)
                        {
                            if (data.arpCache.ContainsKey(arpp.ASenderIP))
                            {
                                if (!data.arpCache[arpp.ASenderIP].Equals(arpp.ASenderMac))
                                {
                                    PacketMainReturn pmr = new PacketMainReturn("Simple ARP Poisoning Protection");
                                    pmr.returnType = PacketMainReturnType.Drop;
                                    if (data.LogAttacks)
                                        pmr.returnType |= PacketMainReturnType.Log; 
                                    pmr.logMessage = "ARP Request from " + arpp.ASenderMac.ToString() + " for " + arpp.ASenderIP.ToString() + " does not match the ARP cache.";
                                    return pmr;
                                }
                            }
                        }
                    }
                    return new PacketMainReturn("Simple ARP Poisoning Protection"){returnType = PacketMainReturnType.Allow};
                }
                PacketMainReturn p = new PacketMainReturn("Simple ARP Poisoning Protection");
                p.returnType = PacketMainReturnType.Allow;
                return p;
            }
            PacketMainReturn pm = new PacketMainReturn("Simple ARP Poisoning Protection");
            pm.returnType = PacketMainReturnType.Allow;
            return pm;
        }
    }
}
