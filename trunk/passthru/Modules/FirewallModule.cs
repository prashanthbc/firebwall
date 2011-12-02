using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PassThru
{
	[Flags]
	public enum PacketMainReturnType
	{
		Error = 1,          //Reports an error in the packet processing
		Drop = 1 << 1,           //Drops the packet
		Allow = 1 << 2,          //Allows the packet to be passed on to the next module
        Edited = 1 << 3,
		Log = 1 << 4	        //Logs the packet
	}

	public class PacketMainReturn
    {
		/// <summary>
		/// Creates a PacketMainReturn with the basic unknown error message
		/// </summary>
		/// <param name="moduleName"></param>
		public PacketMainReturn(string moduleName) 
        {
			Module = moduleName;
			returnType = PacketMainReturnType.Error | PacketMainReturnType.Log;
			logMessage = "An error has occurred in " + moduleName + " with no other details.";
		}
        public PacketMainReturn(string moduleName, Exception e)
        {
            Module = moduleName;
            returnType = PacketMainReturnType.Error | PacketMainReturnType.Log;
            logMessage = "An error has occurred in " + moduleName + ". " + e.Message + "\r\n" + e.StackTrace;
        }
		public string Module = null;
		public Packet SendPacket = null;
		public string logMessage = null;
		public PacketMainReturnType returnType;        
	}

	public enum ModuleErrorType
	{
		Success,        //No error
		UnknownError    //I'm not sure what type of errors it'll run into yet
	}

	public class ModuleError
    {
		public byte[] errorBinary = null;
		public string errorMessage = null;
		public ModuleErrorType errorType;
		public string moduleName = null;
	}

	/// <summary>
	/// An abstract class for the firewall modules, making input and output uniform
	/// </summary>
	public abstract class FirewallModule
    {
		public FirewallModule(NetworkAdapter adapter) 
        {
			this.adapter = adapter;
		}
		public NetworkAdapter adapter = null;
		public System.Windows.Forms.UserControl uiControl = null;
		public string moduleName = null;
        public object PersistentData = null;
        public bool Enabled = true;

        public void SaveConfig()
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
            string file = folder + Path.DirectorySeparatorChar + adapter.InterfaceInformation.Name + moduleName + ".cfg";
            Stream stream = File.Open(file, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, PersistentData);
            stream.Close();
        }

        public void LoadConfig()
        {
            try
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
                string file = folder + Path.DirectorySeparatorChar + adapter.InterfaceInformation.Name + moduleName + ".cfg";
                Stream stream = File.Open(file, FileMode.Open);
                BinaryFormatter bFormatter = new BinaryFormatter();
                PersistentData = bFormatter.Deserialize(stream);
                stream.Close();
            }
            catch
            {
                PersistentData = null;
            }
        }

        public virtual System.Windows.Forms.UserControl GetControl()
        {
            return null;
        }

		/// <summary>
		/// Ran after the module is loaded, to prime it for processing if required
		/// </summary>
		/// <returns>Any error that occured during the starting of it</returns>
		public abstract ModuleError ModuleStart();

		/// <summary>
		/// Ran when the module is to be stopped, to clear up any uneeded resources
		/// </summary>
		/// <returns>Any error that occured during the stopping of it</returns>
		public abstract ModuleError ModuleStop();

		/// <summary>
		/// The wrapper function for processing packets
		/// </summary>
		/// <param name="in_packet">Packet to be processed</param>
		/// <returns>A PacketMainReturn object, either from the interiorMain or default error one</returns>
		public PacketMainReturn PacketMain(ref Packet in_packet) {
            if (Enabled)
            {
                try
                {
                    PacketMainReturn pmr = interiorMain(ref in_packet);
                    return pmr;
                }
                catch (Exception e)
                {
                    return new PacketMainReturn(moduleName, e);
                }
            }
            else
                return new PacketMainReturn(moduleName) { returnType = PacketMainReturnType.Allow };
		}

		/// <summary>
		/// The internal function for processing packets implemented by the module
		/// </summary>
		/// <param name="in_packet">Packet to be processed</param>
		/// <returns>PacketMainReturn object describing what to do with the packet and/or
		/// anything that is notable during the processing</returns>
		public abstract PacketMainReturn interiorMain(ref Packet in_packet);
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
}
