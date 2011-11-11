using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;

namespace PassThru
{
		public partial class AdapterControl: UserControl 
        {
			public AdapterControl() 
            {
				InitializeComponent();
				UpdateAdapterList();
			}

			public class AdapterInfo
            {
				public AdapterInfo(string P, string N, NetworkInterface NI) 
                {
					pointer = P;
					deviceName = N;
					ni = NI;
				}
				string deviceName = "";
				NetworkInterface ni = null;

				public string DName 
                {
					get 
                    {
						return deviceName;
					}
				}

				public string IPv4 
                {
					get 
                    {
						if (ni == null)
								return "";
						else
						{
								foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
								{
										if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
										{
												return ip.Address.ToString();
										}
								}
								return "";
						}
					}
				}

				public string IPv6 
                {
					get 
                    {
						if (ni == null)
								return "";
						else
						{
								foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
								{
										if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
										{
												return ip.Address.ToString();
										}
								}
								return "";
						}
					}
				}

				public string NIDescription 
                {
					get 
                    {
						if (ni == null)
								return "";
						else
								return ni.Description;
					}
				}

				public string NIName 
                {
					get 
                    {
						if (ni == null)
								return "";
						else
								return ni.Name;
					}
				}

				public string Pointer 
                {
					get 
                    {
						return pointer;
					}
				}
				string pointer = null;
			}
			BindingSource source = new BindingSource();

			public void UpdateAdapterList() 
            {
				source.Clear();
				foreach (NetworkAdapter na in NetworkAdapter.GetAllAdapters())
				{
					source.Add(new AdapterInfo(na.Pointer, na.Name, na.InterfaceInformation));
				}
				dataGridView1.DataSource = source;
			}
		}
}
