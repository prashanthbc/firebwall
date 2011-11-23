using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;

namespace PassThru
{
    public partial class ArpPoisoningProtection : UserControl
    {
        SimpleAntiARPPoisoning saap;
        Dictionary<int, PhysicalAddress> cache = new Dictionary<int, PhysicalAddress>();

        public ArpPoisoningProtection(SimpleAntiARPPoisoning saap)
        {
            this.saap = saap;
            cache = saap.GetCache();
            saap.UpdatedArpCache += new System.Threading.ThreadStart(saap_UpdatedArpCache);
            InitializeComponent();
            saap_UpdatedArpCache();
        }

        void saap_UpdatedArpCache()
        {
            if (listBox1.InvokeRequired)
            {
                cache = saap.GetCache();
                System.Threading.ThreadStart ts = new System.Threading.ThreadStart(saap_UpdatedArpCache);
                listBox1.Invoke(ts);
            }
            else
            {
                listBox1.Items.Clear();                
                foreach (KeyValuePair<int, PhysicalAddress> i in cache)
                {
                    listBox1.Items.Add(i.Value.ToString() + " -> " + new IPAddress(i.Key).ToString());
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string i = (string)listBox1.SelectedItem;
                IPAddress ip = IPAddress.Parse(i.Split(' ')[2]);
                cache.Remove(ip.GetHashCode());
                saap.UpdateCache(cache);
                cache = saap.GetCache();
                saap_UpdatedArpCache();
            }
        }
    }
}
