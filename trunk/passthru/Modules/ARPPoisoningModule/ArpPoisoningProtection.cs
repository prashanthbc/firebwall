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
        ARPPP saap;
        SerializableDictionary<IPAddress, string> cache = new SerializableDictionary<IPAddress, string>();

        public ArpPoisoningProtection(ARPPP saap)
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
                foreach (KeyValuePair<IPAddress, string> i in cache)
                {
                    listBox1.Items.Add(i.Value + " -> " + i.Key.ToString());
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBox1.SelectedItem != null)
                {
                    string i = (string)listBox1.SelectedItem;
                    IPAddress ip = IPAddress.Parse(i.Split(' ')[2]);
                    cache.Remove(ip);
                    saap.UpdateCache(cache);
                    cache = saap.GetCache();
                    saap_UpdatedArpCache();
                }
            }
            catch { }
        }

        private void checkBoxSave_CheckedChanged(object sender, EventArgs e)
        {
            saap.data.Save = checkBoxSave.Checked;
        }

        private void checkBoxLogUnsolicited_CheckedChanged(object sender, EventArgs e)
        {
            saap.data.LogUnsolic = checkBoxLogUnsolicited.Checked;
        }

        private void checkBoxLogPoisoning_CheckedChanged(object sender, EventArgs e)
        {
            saap.data.LogAttacks = checkBoxLogPoisoning.Checked;
        }

        private void ArpPoisoningProtection_Load(object sender, EventArgs e)
        {
            checkBoxLogPoisoning.Checked = saap.data.LogAttacks;
            checkBoxLogUnsolicited.Checked = saap.data.LogUnsolic;
            checkBoxSave.Checked = saap.data.Save;
            saap_UpdatedArpCache();
        }
    }
}
