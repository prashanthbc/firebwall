using System;
using System.Collections.Generic;
using System.Text;
using NdisapiSpace;
using System.Threading;
using Win32APISPace;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace PassThru
{
		public class NetworkAdapter
        {
            static List<NetworkAdapter> currentAdapters = new List<NetworkAdapter>();

			public NetworkAdapter(IntPtr adapterHandle, string name) 
            {
				this.adapterHandle = adapterHandle;
				ndisDeviveName = name;
				ndisDeviveName = this.ndisDeviveName.Substring(0, this.ndisDeviveName.IndexOf((char)0x00));
				foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
				{
						if (name.StartsWith("\\DEVICE\\" + ni.Id))
						{
								inter = ni;
								ndisDeviveName = ni.Id;
						}
				}
			}

			NetworkAdapter() 
            {
				lock (staticPadlock)
				{
						UpdateAdapterList();
						//return new List<NetworkAdapter>(currentAdapters);
				}
			}

			public enum AdapterStartReturn
			{
					NoError,
					Error
			}
			IntPtr adapterHandle;
			ManualResetEvent hEvent = null;
			NetworkInterface inter = null;
			ADAPTER_MODE mode = new ADAPTER_MODE();
			ModuleList modules = new ModuleList();
			string ndisDeviveName = "";
			object padlock = new object();
			bool processing = false;
			Thread processingThread = null;
			static IntPtr hNdisapi = IntPtr.Zero;
			static bool isNdisFilterDriverOpen = false;
			static object staticPadlock = new object();

			public static void ShutdownAll() {
				lock (staticPadlock)
				{
						CloseAllInterfaces();
						CloseNDISDriver();
				}
			}

			public AdapterStartReturn StartProcessing() {
				SetAdapterMode();
				SetPacketEvent();
				processingThread = new Thread(ProcessLoop);
				processing = true;
				processingThread.Start();
				return AdapterStartReturn.NoError;
			}

			public void StopProcessing() {
				if (processingThread != null)
				{
						processingThread.Abort();
						processing = false;
						hEvent.Close();
						for (int x = 0; x < modules.Count; x++)
						{
								modules.GetModule(x).ModuleStop();
						}
				}
			}

			static void CloseAllInterfaces() {
				foreach (NetworkAdapter na in currentAdapters)
				{
						na.StopProcessing();
				}
			}

			static void CloseNDISDriver() {
				Ndisapi.CloseFilterDriver(hNdisapi);
			}

			static void OpenNDISDriver() {
				if (hNdisapi != IntPtr.Zero)
				{
						LogCenter.Instance.Push("NetworkAdapter-static", "Bad state was found, attempting to open the NDIS Filter Driver while the IntPtr != IntPtr.Zero, continuing");
				}

				hNdisapi = Ndisapi.OpenFilterDriver("NDISRD");
				isNdisFilterDriverOpen = true;
			}

			unsafe void ProcessLoop() {
				// Allocate and initialize packet structures
				ETH_REQUEST Request = new ETH_REQUEST();
				INTERMEDIATE_BUFFER PacketBuffer = new INTERMEDIATE_BUFFER();

				IntPtr PacketBufferIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(PacketBuffer));
				try
				{
						win32api.ZeroMemory(PacketBufferIntPtr, Marshal.SizeOf(PacketBuffer));

						Request.hAdapterHandle = adapterHandle;
						Request.EthPacket.Buffer = PacketBufferIntPtr;

						ETHER_HEADER* pEthHeader = null;
						IPHeader* pIpHeader = null;
						TcpHeader* pTcpHeader = null;
						UdpHeader* pUdpHeader = null;

						//Static and test modules
						BasicFirewall tfm = new BasicFirewall(this);
						tfm.ModuleStart();

						modules.AddModule(tfm);

						//DumpToPcapModule dtpm = new DumpToPcapModule(this);
						//dtpm.ModuleStart();
						//modules.Add(dtpm);

						SimpleAntiARPPoisoning saap = new SimpleAntiARPPoisoning(this);
						saap.ModuleStart();
						modules.AddModule(saap);

						while (true)
						{
								hEvent.WaitOne();

								while (Ndisapi.ReadPacket(hNdisapi, ref Request))
								{

										PacketBuffer = (INTERMEDIATE_BUFFER)Marshal.PtrToStructure(PacketBufferIntPtr, typeof(INTERMEDIATE_BUFFER));

										Packet pkt = new EthPacket(PacketBuffer).MakeNextLayerPacket();

										bool drop = false;

										for (int x = 0; x < modules.Count; x++)
										{
												FirewallModule fm = modules.GetModule(x);
												PacketMainReturn pmr = fm.PacketMain(pkt);
												if ((pmr.returnType & PacketMainReturnType.Log) == PacketMainReturnType.Log && pmr.logMessage != null)
												{
														LogCenter.Instance.Push(pmr.Module, pmr.logMessage);
												}
												if ((pmr.returnType & PacketMainReturnType.Drop) == PacketMainReturnType.Drop)
												{
														drop = true;
														break;
												}
										}

										if (!drop)
										{
												if (PacketBuffer.m_dwDeviceFlags == Ndisapi.PACKET_FLAG_ON_SEND)
														Ndisapi.SendPacketToAdapter(hNdisapi, ref Request);
												else
														Ndisapi.SendPacketToMstcp(hNdisapi, ref Request);
										}
								}

								hEvent.Reset();
						}
				}
				catch (ThreadAbortException tae)
				{
						Marshal.FreeHGlobal(PacketBufferIntPtr);
				}
			}

			void SetAdapterMode() 
            {
				mode.dwFlags = Ndisapi.MSTCP_FLAG_SENT_TUNNEL | Ndisapi.MSTCP_FLAG_RECV_TUNNEL;
				mode.hAdapterHandle = adapterHandle;
				Ndisapi.SetAdapterMode(hNdisapi, ref mode);
			}

			void SetNoLongerAvailable() 
            {
				//see if any unloading needs to be done
				//stop any processing
				//save any module information
				//save any adapter information
				//reset any variables to their defaults(non static)
			}

			void SetPacketEvent() 
            {
				hEvent = new ManualResetEvent(false);
				Ndisapi.SetPacketEvent(hNdisapi, adapterHandle, hEvent.SafeWaitHandle);
			}

			static void UpdateAdapterList() 
            {
				if (!isNdisFilterDriverOpen)
				{
						OpenNDISDriver();
				}
				TCP_AdapterList adList = new TCP_AdapterList();
				Ndisapi.GetTcpipBoundAdaptersInfo(hNdisapi, ref adList);
				List<NetworkAdapter> tempList = new List<NetworkAdapter>();

				//Populate with current adapters
				List<NetworkAdapter> notFound = new List<NetworkAdapter>();
				for (int x = 0; x < currentAdapters.Count; x++)
				{
						bool found = false;
						for (int y = 0; y < adList.m_nAdapterCount; y++)
						{
								if (adList.m_nAdapterHandle[y] == currentAdapters[x].adapterHandle)
								{
										tempList.Add(currentAdapters[x]);
										found = true;
								}
						}
						if (!found)
						{
								notFound.Add(currentAdapters[x]);
						}
				}

				//Deal with no longer existant adapters
				for(int x = 0; x < notFound.Count; x++)
				{
						notFound[x].SetNoLongerAvailable();
				}

				//Adding any new adapters
				for (int x = 0; x < adList.m_nAdapterCount; x++)
				{
						bool found = false;
						for (int y = 0; y < currentAdapters.Count; y++)
						{
								if (adList.m_nAdapterHandle[x] == currentAdapters[y].adapterHandle)
										found = true;
						}
						if (!found)
						{
								NetworkAdapter newAdapter = new NetworkAdapter(adList.m_nAdapterHandle[x], Encoding.ASCII.GetString(adList.m_szAdapterNameList, x * 256, 256));
								tempList.Add(newAdapter);
						}
				}

				currentAdapters = new List<NetworkAdapter>(tempList);
			}

			public NetworkInterface InterfaceInformation {
				get {
					return inter;
				}
			}

			public string Name {
				get {
					return ndisDeviveName;
				}
			}

			public string Pointer {
				get {
					return adapterHandle.ToString();
				}
			}

            public static List<NetworkAdapter> GetAllAdapters()
            {
                UpdateAdapterList();
                return new List<NetworkAdapter>(currentAdapters);
            }
        }
}
