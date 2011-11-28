﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PassThru
{
    /*
     * ICMP filtering module.  
     * Allows filtering by both code and type.
     */
    public class ICMPFilterModule : FirewallModule
    {
        // rule table var and accessor
        private Dictionary<string, List<string>> ruletable = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> RuleTable
        {
            get { return ruletable; }
            set { ruletable = new Dictionary<string, List<string>>(value); }
        }
        
        // deny all var and accessor
        private bool denyAll;
        public bool DenyAll
        {
            get { return denyAll; }
            set { denyAll = value; }
        }

        // return local user control
        public override System.Windows.Forms.UserControl GetControl()
        {
            return new ICMPFilterDisplay(this) { Dock = System.Windows.Forms.DockStyle.Fill };
        }

        // constructor 
        public ICMPFilterModule(NetworkAdapter adapter)
            : base(adapter)
        {
            moduleName = "ICMP Filter";
        }

        // Action for ModuleStart
        public override ModuleError ModuleStart()
        {
            ModuleError moduleError = new ModuleError();
            moduleError.errorType = ModuleErrorType.Success;
            denyAll = false;
            return moduleError;
        }

        // Action for ModuleError
        public override ModuleError ModuleStop()
        {
            ModuleError moduleError = new ModuleError();
            moduleError.errorType = ModuleErrorType.Success;
            return moduleError;
        }

        // main routine
        public override PacketMainReturn interiorMain(ref Packet in_packet)
        {
            PacketMainReturn pmr;

            // if the packet is ICMP
            if (in_packet.GetHighestLayer() == Protocol.ICMP)
            {
                // check if the packet is allowed and deny all is false
                if (isAllowed(((ICMPPacket)in_packet).getType(), ((ICMPPacket)in_packet).getCode()) && 
                    !denyAll)
                {
                    pmr = new PacketMainReturn("ICMPFilter Module");
                    pmr.returnType = PacketMainReturnType.Allow;
                    return pmr;
                }
                // else, log and drop it
                else
                {
                    pmr = new PacketMainReturn("ICMPFilter Module");
                    pmr.returnType = PacketMainReturnType.Drop | PacketMainReturnType.Log;
                    pmr.logMessage = "ICMP from " + ((ICMPPacket)in_packet).SourceIP.ToString() + " for " +
                        ((ICMPPacket)in_packet).DestIP.ToString() + " was dropped.";
                    return pmr;
                }
            }

            pmr = new PacketMainReturn("ICMPFilter Module");
            pmr.returnType = PacketMainReturnType.Allow;
            return pmr;
        }

        /*
         * Method used to check whether an ICMP packet should be
         * allowed through.
         * 
         * @param type is the ICMP type
         * @param code is the ICMP code
         */
        private bool isAllowed(string type, string code)
        {
            bool isAllowed = true;

            // if the table contains the type, check if it
            // also contains the code
            if (ruletable.ContainsKey(type))
            {
                List<string> temp;
                ruletable.TryGetValue(type, out temp);
                // invert logic; if found, disallow, if not, allow
                isAllowed = !(temp.Contains(code));
            }
            return isAllowed;
        }
    }
}