using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PassThru.Modules.BasicFirewallModule
{
    public partial class AddEditRule : Form
    {
        public AddEditRule()
        {
            InitializeComponent();
        }

        public BasicFirewall.Rule newRule;

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBoxIn.Checked || checkBoxOut.Checked)
            {
                try
                {
                    BasicFirewall.RuleType rt = BasicFirewall.RuleType.MAC;
                    switch (comboBox1.Text)
                    {
                        case "UDPALL":
                            rt = BasicFirewall.RuleType.UDPALL;
                            break;
                        case "UDPPORT":
                            rt = BasicFirewall.RuleType.UDPPORT;
                            break;
                        case "TCPIPPORT":
                            rt = BasicFirewall.RuleType.TCPIPPORT;
                            break;
                        case "TCPPORT":
                            rt = BasicFirewall.RuleType.TCPPORT;
                            break;
                        case "TCPALL":
                            rt = BasicFirewall.RuleType.TCPALL;
                            break;
                        case "MAC":
                            rt = BasicFirewall.RuleType.MAC;
                            break;
                    }
                    BasicFirewall.Direction dir;
                    if (checkBoxIn.Checked && checkBoxOut.Checked)
                    {
                        dir = BasicFirewall.Direction.IN | BasicFirewall.Direction.OUT;
                    }
                    else if (checkBoxOut.Checked)
                    {
                        dir = BasicFirewall.Direction.OUT;
                    }
                    else
                    {
                        dir = BasicFirewall.Direction.IN;
                    }
                    BasicFirewall.PacketStatus ps;
                    if (comboBoxAction.Text == "Block")
                        ps = BasicFirewall.PacketStatus.BLOCKED;
                    else
                        ps = BasicFirewall.PacketStatus.ALLOWED;
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    newRule = BasicFirewall.RuleFactory.MakeRule(rt, ps, dir, textBoxArguments.Text, checkBoxLog.Checked);
                    this.Close();
                }
                catch
                {
                    MessageBox.Show("Error in creating rule.");
                }
            }
            else
            {
                MessageBox.Show("You need to select in or out first");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
