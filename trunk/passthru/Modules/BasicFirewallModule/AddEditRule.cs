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


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.Text)
            {
                case "All UDP Rule":
                    labelArgs.Text = "(Space Separated) No args";
                    break;
                case "UDP Port Rule":
                    labelArgs.Text = "(Space Separated) Port";
                    break;
                case "TCP IP and Port Rule":
                    labelArgs.Text = "(Space Separated) IP Port";
                    break;
                case "TCP Port Rule":
                    labelArgs.Text = "(Space Separated) Port";
                    break;
                case "All TCP Rule":
                    labelArgs.Text = "(Space Separated) No args";
                    break;
                case "All Rule":
                    labelArgs.Text = "(Space Separated) No args";
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBoxIn.Checked || checkBoxOut.Checked)
            {
                try
                {
                    BasicFirewall.RuleType rt = BasicFirewall.RuleType.ALL;
                    switch (comboBox1.Text)
                    {
                        case "All UDP Rule":
                            rt = BasicFirewall.RuleType.UDPALL;
                            break;
                        case "UDP Port Rule":
                            rt = BasicFirewall.RuleType.UDPPORT;
                            break;
                        case "TCP IP and Port Rule":
                            rt = BasicFirewall.RuleType.TCPIPPORT;
                            break;
                        case "TCP Port Rule":
                            rt = BasicFirewall.RuleType.TCPPORT;
                            break;
                        case "All TCP Rule":
                            rt = BasicFirewall.RuleType.TCPALL;
                            break;
                        case "All Rule":
                            rt = BasicFirewall.RuleType.ALL;
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

        private void AddEditRule_Load(object sender, EventArgs e)
        {
            comboBox1.Text = "All Rule";
            comboBox1.Items.Add("All TCP Rule");
            comboBox1.Items.Add("TCP IP and Port Rule");
            comboBox1.Items.Add("TCP Port Rule");
            comboBox1.Items.Add("All UDP Rule");
            comboBox1.Items.Add("UDP Port Rule");
            comboBox1.Items.Add("All Rule");
        }

        private void textBoxArguments_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
