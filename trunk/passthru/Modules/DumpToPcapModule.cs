using System;
using System.Collections.Generic;
using System.Text;

namespace PassThru
{
    /*
     * Dump data to pcap module
     * @summary
     */
    public class DumpToPcapModule : FirewallModule
    {
        public DumpToPcapModule(NetworkAdapter adapter)
            : base(adapter)
        {
        }
        PcapFileWriter file;

        public override ModuleError ModuleStart()
        {
            string f = "pcaptest-" + PcapCreator.Instance.GetNewDate() + ".pcap";
            file = new PcapFileWriter(f);
            ModuleError me = new ModuleError();
            me.errorType = ModuleErrorType.Success;
            return me;
        }

        public override ModuleError ModuleStop()
        {
            file.Close();
            ModuleError me = new ModuleError();
            me.errorType = ModuleErrorType.Success;
            return me;
        }

        public override PacketMainReturn interiorMain(ref Packet in_packet)
        {
            byte[] buffer = new byte[in_packet.Length()];
            Buffer.BlockCopy(in_packet.Data(), 0, buffer, 0, (int)in_packet.Length());
            file.AddPacket(buffer);
            PacketMainReturn pmr = new PacketMainReturn("Dump to Pcap");
            pmr.returnType = PacketMainReturnType.Allow;
            return pmr;
        }
    }
}
