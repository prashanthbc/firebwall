﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Net;
using FM;
using System.Threading;

namespace ScanDetector
{
    public partial class ScanDetectorUI : UserControl
    {
        private ScanDetector detector;

        public ScanDetectorUI(ScanDetector d)
        {
            this.detector = d;
            InitializeComponent();
        }

        /// <summary>
        /// Load up the GUI settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScanDetectorUI_Load(object sender, EventArgs e)
        {
            // add the potential IPs from this session
            foreach (IPAddress i in detector.potentials.Keys)
            {
                potentialIPBox.Items.Add(i.ToString());
            }

            // add the persistent blocked IPs
            foreach ( IPAddress i in detector.data.BlockCache.Keys)
            {
                blockedIPList.Rows.Add(i.ToString());
            }

            // set the block immediate check
            skipPotential.Checked = detector.data.blockImmediately;
        }

        /// <summary>
        /// Adds a potential IP address 
        /// </summary>
        /// <param name="addr"></param>
        public void addPotential(IPAddress addr)
        {
            if (potentialIPBox.InvokeRequired)
            {
                potentialIPBox.Invoke((MethodInvoker)delegate
                {
                    addPotential(addr);
                });
            }
            else
            {
                if (!potentialIPBox.Items.Contains(addr.ToString()))
                    this.potentialIPBox.Items.Add(addr.ToString());
            }
        }

        /// <summary>
        /// swap button state when the check changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void skipPotential_CheckedChanged(object sender, EventArgs e)
        {
            blockButton.Enabled = !blockButton.Enabled;
            detector.data.blockImmediately = skipPotential.Checked;
        }

        /// <summary>
        /// handles the block button action; adds the selected IP address
        /// to the block list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void blockButton_Click(object sender, EventArgs e)
        {
            if (potentialIPBox.SelectedIndex < 0)
                return;

            String ip = potentialIPBox.SelectedItem.ToString();

            // add it to the data block cache
            detector.data.BlockCache.Add(IPAddress.Parse(ip), detector.potentials[IPAddress.Parse(ip)]);
            // remove it from the potential list
            potentialIPBox.Items.Remove(ip);
            detector.potentials.Remove(IPAddress.Parse(ip));
            // add it to the block list
            blockedIPList.Rows.Add(ip);
        }

        /// <summary>
        /// removes the blocked IP address from the table and the block cache
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeBlockedButton_Click(object sender, EventArgs e)
        {
            if (blockedIPList.SelectedRows.Count < 0)
                return;

            int rowIdx = blockedIPList.SelectedCells[0].RowIndex;
            string ip = blockedIPList["IP", rowIdx].Value.ToString();
            // remove it from the block cache
            detector.data.BlockCache.Remove(IPAddress.Parse(ip));
            blockedIPList.Rows.RemoveAt(rowIdx);
        }

        /// <summary>
        /// Generates a report on the selected IP address; pretty much just formats everything found
        /// in its IPObj object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void infoButton_Click(object sender, EventArgs e)
        {
            int rowIdx = 0;
            string ip = null;
            IPObj tmp;

            if (blockedIPList.SelectedRows.Count <= 0)
            {
                ip = potentialIPBox.SelectedItem.ToString();
                tmp = detector.potentials[IPAddress.Parse(ip)];
            }
            else if (potentialIPBox.SelectedIndex < 0)
            {
                rowIdx = blockedIPList.SelectedCells[0].RowIndex;
                ip = blockedIPList["IP", rowIdx].Value.ToString();
                tmp = detector.data.BlockCache[IPAddress.Parse(ip)];
            }
            else
                return;

            // go generate the report
            Report report = new Report();
            report.GenerateReport(tmp);
        }

        // if they clicked the blocked table, deselect the potential box
        private void blockedIPList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            potentialIPBox.ClearSelected();
        }

        // if they clicked the potential listbox, deselect the blocked box
        private void potentialIPBox_Click(object sender, EventArgs e)
        {
            blockedIPList.ClearSelection();
        }
    }
}
