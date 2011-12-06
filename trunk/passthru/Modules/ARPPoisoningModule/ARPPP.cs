using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using FM;

namespace PassThru
{
    public class ARPPP : FirewallModule
    {
        public ARPPP()
            : base()
        {
            this.moduleName = "Arp Poisoning Protection";
        }

        public ARPPP(INetworkAdapter adapter)
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
                data.arpCache = new SerializableDictionary<IPAddress, byte[]>();

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
            public SerializableDictionary<IPAddress, byte[]> arpCache = new SerializableDictionary<IPAddress, byte[]>();
            public bool Save = true;
            public bool LogUnsolic = true;
            public bool LogAttacks = true;
        }

        public ArpData data;

        public event System.Threading.ThreadStart UpdatedArpCache;
        object padlock = new object();



        public SerializableDictionary<IPAddress, byte[]> GetCache()
        {
            lock (padlock)
            {
                return new SerializableDictionary<IPAddress, byte[]>(data.arpCache);
            }
        }

        public void UpdateCache(SerializableDictionary<IPAddress, byte[]> cache)
        {
            lock (padlock)
            {
                data.arpCache = new SerializableDictionary<IPAddress, byte[]>(cache);
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
                                    if (!Compare(data.arpCache[arpp.ASenderIP], arpp.ASenderMac.GetAddressBytes()))
                                    {
                                        PacketMainReturn pmr = new PacketMainReturn("Simple ARP Poisoning Protection");
                                        pmr.returnType = PacketMainReturnType.Edited;                                        
                                        if (data.LogAttacks)
                                            pmr.returnType |= PacketMainReturnType.Log;
                                        switch (LanguageConfig.GetCurrentLanguage())
                                        {
                                            case LanguageConfig.Language.NONE:
                                            case LanguageConfig.Language.ENGLISH:
                                                pmr.logMessage = "ARP Response from " + arpp.ASenderMac.ToString() + " for " + arpp.ASenderIP.ToString() + " does not match the ARP cache.";
                                                break;
                                            case LanguageConfig.Language.CHINESE:
                                                pmr.logMessage = arpp.ASenderMac.ToString() + "为" + arpp.ASenderIP.ToString() + "的ARP响应不匹配的ARP缓存。";
                                                break;
                                            case LanguageConfig.Language.GERMAN:
                                                pmr.logMessage = "ARP Response von " + arpp.ASenderMac.ToString() + " für " + arpp.ASenderIP.ToString() + " nicht mit dem ARP-Cache.";
                                                break;
                                            case LanguageConfig.Language.RUSSIAN:
                                                pmr.logMessage = "ARP-ответ от " + arpp.ASenderMac.ToString() + " для " + arpp.ASenderIP.ToString() + " не соответствует кэш ARP.";
                                                break;
                                            case LanguageConfig.Language.SPANISH:
                                                pmr.logMessage = "Respuesta de ARP de " + arpp.ASenderMac.ToString() + " para " + arpp.ASenderIP.ToString() + " no coincide con la caché ARP.";
                                                break;
                                        }                                        
                                        arpp.ATargetIP = arpp.ASenderIP;
                                        arpp.ATargetMac = new PhysicalAddress(data.arpCache[arpp.ATargetIP]);
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
                                    data.arpCache[arpp.ASenderIP] = arpp.ASenderMac.GetAddressBytes();
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
                                    if (!Compare(data.arpCache[arpp.ASenderIP], arpp.ASenderMac.GetAddressBytes()))
                                    {
                                        PacketMainReturn pmra = new PacketMainReturn("Simple ARP Poisoning Protection");
                                        pmra.returnType = PacketMainReturnType.Edited;
                                        if (data.LogAttacks)
                                            pmra.returnType |= PacketMainReturnType.Log;
                                        switch (LanguageConfig.GetCurrentLanguage())
                                        {
                                            case LanguageConfig.Language.NONE:
                                            case LanguageConfig.Language.ENGLISH:
                                                pmra.logMessage = "ARP Response from " + arpp.ASenderMac.ToString() + " for " + arpp.ASenderIP.ToString() + " does not match the ARP cache.";
                                                break;
                                            case LanguageConfig.Language.CHINESE:
                                                pmra.logMessage = arpp.ASenderMac.ToString() + "为" + arpp.ASenderIP.ToString() + "的ARP响应不匹配的ARP缓存。";
                                                break;
                                            case LanguageConfig.Language.GERMAN:
                                                pmra.logMessage = "ARP Response von " + arpp.ASenderMac.ToString() + " für " + arpp.ASenderIP.ToString() + " nicht mit dem ARP-Cache.";
                                                break;
                                            case LanguageConfig.Language.RUSSIAN:
                                                pmra.logMessage = "ARP-ответ от " + arpp.ASenderMac.ToString() + " для " + arpp.ASenderIP.ToString() + " не соответствует кэш ARP.";
                                                break;
                                            case LanguageConfig.Language.SPANISH:
                                                pmra.logMessage = "Respuesta de ARP de " + arpp.ASenderMac.ToString() + " para " + arpp.ASenderIP.ToString() + " no coincide con la caché ARP.";
                                                break;
                                        }     
                                        arpp.ATargetIP = arpp.ASenderIP;
                                        arpp.ATargetMac = new PhysicalAddress(data.arpCache[arpp.ATargetIP]);
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
                            switch (LanguageConfig.GetCurrentLanguage())
                            {
                                case LanguageConfig.Language.NONE:
                                case LanguageConfig.Language.ENGLISH:
                                    pmr.logMessage = "Unsolicited ARP Response from " + arpp.ASenderMac.ToString() + " for " + arpp.ASenderIP.ToString();
                                    break;
                                case LanguageConfig.Language.CHINESE:
                                    pmr.logMessage = "未经请求的ARP应答为" + arpp.ASenderIP.ToString() + "从" + arpp.ASenderMac.ToString();
                                    break;
                                case LanguageConfig.Language.GERMAN:
                                    pmr.logMessage = "Initiativbewerbung ARP Response von " + arpp.ASenderMac.ToString() + " für " + arpp.ASenderIP.ToString();
                                    break;
                                case LanguageConfig.Language.RUSSIAN:
                                    pmr.logMessage = "Незапрошенные ответ ARP от " + arpp.ASenderMac.ToString() + " для " + arpp.ASenderIP.ToString();
                                    break;
                                case LanguageConfig.Language.SPANISH:
                                    pmr.logMessage = "Respuesta ARP no solicitados de " + arpp.ASenderMac.ToString() + " para " + arpp.ASenderIP.ToString();
                                    break;
                            }                                 
                            return pmr;
                        }
                    }
                    else
                    {
                        lock (padlock)
                        {
                            if (data.arpCache.ContainsKey(arpp.ASenderIP))
                            {
                                if (!Compare(data.arpCache[arpp.ASenderIP], arpp.ASenderMac.GetAddressBytes()))
                                {
                                    PacketMainReturn pmr = new PacketMainReturn("Simple ARP Poisoning Protection");
                                    pmr.returnType = PacketMainReturnType.Drop;
                                    if (data.LogAttacks)
                                        pmr.returnType |= PacketMainReturnType.Log;
                                    switch (LanguageConfig.GetCurrentLanguage())
                                    {
                                        case LanguageConfig.Language.NONE:
                                        case LanguageConfig.Language.ENGLISH:
                                            pmr.logMessage = "ARP Response from " + arpp.ASenderMac.ToString() + " for " + arpp.ASenderIP.ToString() + " does not match the ARP cache.";
                                            break;
                                        case LanguageConfig.Language.CHINESE:
                                            pmr.logMessage = arpp.ASenderMac.ToString() + "为" + arpp.ASenderIP.ToString() + "的ARP响应不匹配的ARP缓存。";
                                            break;
                                        case LanguageConfig.Language.GERMAN:
                                            pmr.logMessage = "ARP Response von " + arpp.ASenderMac.ToString() + " für " + arpp.ASenderIP.ToString() + " nicht mit dem ARP-Cache.";
                                            break;
                                        case LanguageConfig.Language.RUSSIAN:
                                            pmr.logMessage = "ARP-ответ от " + arpp.ASenderMac.ToString() + " для " + arpp.ASenderIP.ToString() + " не соответствует кэш ARP.";
                                            break;
                                        case LanguageConfig.Language.SPANISH:
                                            pmr.logMessage = "Respuesta de ARP de " + arpp.ASenderMac.ToString() + " para " + arpp.ASenderIP.ToString() + " no coincide con la caché ARP.";
                                            break;
                                    }     
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

        bool Compare(byte[] a, byte[] b)
        {
            for (int x = 0; x < 6; x++)
            {
                if (a[x] != b[x])
                    return false;
            }
            return true;
        }
    }
}
