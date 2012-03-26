﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;

namespace PassThru.Modules.MacFilter
{
    public partial class AddEditMacRule : Form
    {
        public MacFilterModule.MacRule newRule;
        public AddEditMacRule()
        {
            InitializeComponent();
        }

        // overloaded constructor for editing an existing rule
        public AddEditMacRule(MacFilterModule.MacRule RULE)
        {
            InitializeComponent();

            // set the in/out boxes
            if ((RULE.direction & MacFilterModule.Direction.IN) != 0 ) 
                checkBoxIn.Checked = true;
            if ((RULE.direction & MacFilterModule.Direction.OUT) != 0 )
                checkBoxOut.Checked = true;

            // set the MAC
            if (!String.IsNullOrEmpty(RULE.mac.ToString()))
            {
                textBoxArguments.Text = new PhysicalAddress(RULE.mac).ToString();
            }

            // set logging
            checkBoxLog.Checked = RULE.log;
            
            // set the status
            if ((RULE.ps & MacFilterModule.PacketStatus.ALLOWED) != 0)
                comboBoxAction.SelectedIndex = 1;
            else 
                comboBoxAction.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                MacFilterModule.Direction dir;
                if (checkBoxIn.Checked && checkBoxOut.Checked)
                    dir = MacFilterModule.Direction.IN | MacFilterModule.Direction.OUT;
                else if (checkBoxOut.Checked)
                {
                    dir = MacFilterModule.Direction.OUT;
                }
                else
                    dir = MacFilterModule.Direction.IN;

                MacFilterModule.PacketStatus ps;
                if (comboBoxAction.Text == "Block")
                    ps = MacFilterModule.PacketStatus.BLOCKED;
                else
                    ps = MacFilterModule.PacketStatus.ALLOWED;

                if (String.IsNullOrEmpty(textBoxArguments.Text))
                {
                    newRule = new MacFilterModule.MacRule(ps, dir, checkBoxLog.Checked);
                }
                else
                {
                    string macString = textBoxArguments.Text.ToUpper().Replace("-", "").Replace(":", "").Replace(";", "");
                    newRule = new MacFilterModule.MacRule(ps, System.Net.NetworkInformation.PhysicalAddress.Parse(macString), dir, checkBoxLog.Checked);
                }
                DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            catch { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
