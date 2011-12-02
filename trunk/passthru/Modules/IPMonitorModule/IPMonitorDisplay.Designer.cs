namespace PassThru
{
    partial class IPMonitorDisplay
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ipDisplay = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tcptotalField = new System.Windows.Forms.Label();
            this.tcptotalLabel = new System.Windows.Forms.Label();
            this.tcpDisplay = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.udptotalField = new System.Windows.Forms.Label();
            this.udptotalLabel = new System.Windows.Forms.Label();
            this.udpDisplay = new System.Windows.Forms.DataGridView();
            this.statistics = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.incDataErrField = new System.Windows.Forms.Label();
            this.incDataDiscField = new System.Windows.Forms.Label();
            this.dataSentField = new System.Windows.Forms.Label();
            this.dataReceivedField = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.segsSentField = new System.Windows.Forms.Label();
            this.segsResentField = new System.Windows.Forms.Label();
            this.segsReceivedField = new System.Windows.Forms.Label();
            this.resetsSentField = new System.Windows.Forms.Label();
            this.maxConnField = new System.Windows.Forms.Label();
            this.failedConAttField = new System.Windows.Forms.Label();
            this.errorsReceivedField = new System.Windows.Forms.Label();
            this.cumulativeConnectionsField = new System.Windows.Forms.Label();
            this.connInitiatedField = new System.Windows.Forms.Label();
            this.resetConnField = new System.Windows.Forms.Label();
            this.connAcceptField = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ipDisplay.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tcpDisplay)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udpDisplay)).BeginInit();
            this.statistics.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ipDisplay
            // 
            this.ipDisplay.Controls.Add(this.tabPage1);
            this.ipDisplay.Controls.Add(this.tabPage2);
            this.ipDisplay.Controls.Add(this.statistics);
            this.ipDisplay.Location = new System.Drawing.Point(3, 3);
            this.ipDisplay.Name = "ipDisplay";
            this.ipDisplay.SelectedIndex = 0;
            this.ipDisplay.Size = new System.Drawing.Size(854, 494);
            this.ipDisplay.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tcptotalField);
            this.tabPage1.Controls.Add(this.tcptotalLabel);
            this.tabPage1.Controls.Add(this.tcpDisplay);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(846, 468);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "TCP Connections";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tcptotalField
            // 
            this.tcptotalField.AutoSize = true;
            this.tcptotalField.Location = new System.Drawing.Point(78, 369);
            this.tcptotalField.Name = "tcptotalField";
            this.tcptotalField.Size = new System.Drawing.Size(13, 13);
            this.tcptotalField.TabIndex = 2;
            this.tcptotalField.Text = "0";
            // 
            // tcptotalLabel
            // 
            this.tcptotalLabel.AutoSize = true;
            this.tcptotalLabel.Location = new System.Drawing.Point(3, 369);
            this.tcptotalLabel.Name = "tcptotalLabel";
            this.tcptotalLabel.Size = new System.Drawing.Size(69, 13);
            this.tcptotalLabel.TabIndex = 1;
            this.tcptotalLabel.Text = "Connections:";
            // 
            // tcpDisplay
            // 
            this.tcpDisplay.AllowUserToResizeRows = false;
            this.tcpDisplay.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tcpDisplay.Location = new System.Drawing.Point(6, 6);
            this.tcpDisplay.Name = "tcpDisplay";
            this.tcpDisplay.ReadOnly = true;
            this.tcpDisplay.RowHeadersVisible = false;
            this.tcpDisplay.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tcpDisplay.Size = new System.Drawing.Size(553, 360);
            this.tcpDisplay.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.udptotalField);
            this.tabPage2.Controls.Add(this.udptotalLabel);
            this.tabPage2.Controls.Add(this.udpDisplay);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(846, 468);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "UDP Listeners";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // udptotalField
            // 
            this.udptotalField.AutoSize = true;
            this.udptotalField.Location = new System.Drawing.Point(64, 369);
            this.udptotalField.Name = "udptotalField";
            this.udptotalField.Size = new System.Drawing.Size(13, 13);
            this.udptotalField.TabIndex = 2;
            this.udptotalField.Text = "0";
            // 
            // udptotalLabel
            // 
            this.udptotalLabel.AutoSize = true;
            this.udptotalLabel.Location = new System.Drawing.Point(6, 369);
            this.udptotalLabel.Name = "udptotalLabel";
            this.udptotalLabel.Size = new System.Drawing.Size(52, 13);
            this.udptotalLabel.TabIndex = 1;
            this.udptotalLabel.Text = "Listeners:";
            // 
            // udpDisplay
            // 
            this.udpDisplay.AllowUserToAddRows = false;
            this.udpDisplay.AllowUserToDeleteRows = false;
            this.udpDisplay.AllowUserToResizeRows = false;
            this.udpDisplay.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.udpDisplay.Location = new System.Drawing.Point(6, 6);
            this.udpDisplay.Name = "udpDisplay";
            this.udpDisplay.ReadOnly = true;
            this.udpDisplay.RowHeadersVisible = false;
            this.udpDisplay.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.udpDisplay.Size = new System.Drawing.Size(553, 360);
            this.udpDisplay.TabIndex = 0;
            // 
            // statistics
            // 
            this.statistics.Controls.Add(this.groupBox2);
            this.statistics.Controls.Add(this.groupBox1);
            this.statistics.Location = new System.Drawing.Point(4, 22);
            this.statistics.Name = "statistics";
            this.statistics.Padding = new System.Windows.Forms.Padding(3);
            this.statistics.Size = new System.Drawing.Size(846, 468);
            this.statistics.TabIndex = 2;
            this.statistics.Text = "Statistics";
            this.statistics.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.incDataErrField);
            this.groupBox2.Controls.Add(this.incDataDiscField);
            this.groupBox2.Controls.Add(this.dataSentField);
            this.groupBox2.Controls.Add(this.dataReceivedField);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Location = new System.Drawing.Point(21, 241);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(553, 119);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "UDP";
            // 
            // incDataErrField
            // 
            this.incDataErrField.AutoSize = true;
            this.incDataErrField.Location = new System.Drawing.Point(410, 44);
            this.incDataErrField.Name = "incDataErrField";
            this.incDataErrField.Size = new System.Drawing.Size(41, 13);
            this.incDataErrField.TabIndex = 7;
            this.incDataErrField.Text = "label30";
            // 
            // incDataDiscField
            // 
            this.incDataDiscField.AutoSize = true;
            this.incDataDiscField.Location = new System.Drawing.Point(410, 20);
            this.incDataDiscField.Name = "incDataDiscField";
            this.incDataDiscField.Size = new System.Drawing.Size(41, 13);
            this.incDataDiscField.TabIndex = 6;
            this.incDataDiscField.Text = "label29";
            // 
            // dataSentField
            // 
            this.dataSentField.AutoSize = true;
            this.dataSentField.Location = new System.Drawing.Point(142, 44);
            this.dataSentField.Name = "dataSentField";
            this.dataSentField.Size = new System.Drawing.Size(41, 13);
            this.dataSentField.TabIndex = 5;
            this.dataSentField.Text = "label28";
            // 
            // dataReceivedField
            // 
            this.dataReceivedField.AutoSize = true;
            this.dataReceivedField.Location = new System.Drawing.Point(142, 20);
            this.dataReceivedField.Name = "dataReceivedField";
            this.dataReceivedField.Size = new System.Drawing.Size(41, 13);
            this.dataReceivedField.TabIndex = 4;
            this.dataReceivedField.Text = "label27";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(241, 44);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(162, 13);
            this.label15.TabIndex = 3;
            this.label15.Text = "Incoming Datagrams With Errors:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(245, 20);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(158, 13);
            this.label14.TabIndex = 2;
            this.label14.Text = "Incoming Datagrams Discarded:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(49, 44);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(86, 13);
            this.label13.TabIndex = 1;
            this.label13.Text = "Datagrams Sent:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(25, 20);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(110, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Datagrams Received:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.segsSentField);
            this.groupBox1.Controls.Add(this.segsResentField);
            this.groupBox1.Controls.Add(this.segsReceivedField);
            this.groupBox1.Controls.Add(this.resetsSentField);
            this.groupBox1.Controls.Add(this.maxConnField);
            this.groupBox1.Controls.Add(this.failedConAttField);
            this.groupBox1.Controls.Add(this.errorsReceivedField);
            this.groupBox1.Controls.Add(this.cumulativeConnectionsField);
            this.groupBox1.Controls.Add(this.connInitiatedField);
            this.groupBox1.Controls.Add(this.resetConnField);
            this.groupBox1.Controls.Add(this.connAcceptField);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(21, 18);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(553, 200);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "TCP";
            // 
            // segsSentField
            // 
            this.segsSentField.AutoSize = true;
            this.segsSentField.Location = new System.Drawing.Point(384, 136);
            this.segsSentField.Name = "segsSentField";
            this.segsSentField.Size = new System.Drawing.Size(41, 13);
            this.segsSentField.TabIndex = 21;
            this.segsSentField.Text = "label26";
            // 
            // segsResentField
            // 
            this.segsResentField.AutoSize = true;
            this.segsResentField.Location = new System.Drawing.Point(384, 107);
            this.segsResentField.Name = "segsResentField";
            this.segsResentField.Size = new System.Drawing.Size(41, 13);
            this.segsResentField.TabIndex = 20;
            this.segsResentField.Text = "label25";
            // 
            // segsReceivedField
            // 
            this.segsReceivedField.AutoSize = true;
            this.segsReceivedField.Location = new System.Drawing.Point(384, 79);
            this.segsReceivedField.Name = "segsReceivedField";
            this.segsReceivedField.Size = new System.Drawing.Size(41, 13);
            this.segsReceivedField.TabIndex = 19;
            this.segsReceivedField.Text = "label24";
            // 
            // resetsSentField
            // 
            this.resetsSentField.AutoSize = true;
            this.resetsSentField.Location = new System.Drawing.Point(384, 45);
            this.resetsSentField.Name = "resetsSentField";
            this.resetsSentField.Size = new System.Drawing.Size(41, 13);
            this.resetsSentField.TabIndex = 18;
            this.resetsSentField.Text = "label23";
            // 
            // maxConnField
            // 
            this.maxConnField.AutoSize = true;
            this.maxConnField.Location = new System.Drawing.Point(153, 162);
            this.maxConnField.Name = "maxConnField";
            this.maxConnField.Size = new System.Drawing.Size(41, 13);
            this.maxConnField.TabIndex = 17;
            this.maxConnField.Text = "label22";
            // 
            // failedConAttField
            // 
            this.failedConAttField.AutoSize = true;
            this.failedConAttField.Location = new System.Drawing.Point(153, 137);
            this.failedConAttField.Name = "failedConAttField";
            this.failedConAttField.Size = new System.Drawing.Size(41, 13);
            this.failedConAttField.TabIndex = 16;
            this.failedConAttField.Text = "label21";
            // 
            // errorsReceivedField
            // 
            this.errorsReceivedField.AutoSize = true;
            this.errorsReceivedField.Location = new System.Drawing.Point(153, 107);
            this.errorsReceivedField.Name = "errorsReceivedField";
            this.errorsReceivedField.Size = new System.Drawing.Size(41, 13);
            this.errorsReceivedField.TabIndex = 15;
            this.errorsReceivedField.Text = "label20";
            // 
            // cumulativeConnectionsField
            // 
            this.cumulativeConnectionsField.AutoSize = true;
            this.cumulativeConnectionsField.Location = new System.Drawing.Point(153, 79);
            this.cumulativeConnectionsField.Name = "cumulativeConnectionsField";
            this.cumulativeConnectionsField.Size = new System.Drawing.Size(41, 13);
            this.cumulativeConnectionsField.TabIndex = 14;
            this.cumulativeConnectionsField.Text = "label19";
            // 
            // connInitiatedField
            // 
            this.connInitiatedField.AutoSize = true;
            this.connInitiatedField.Location = new System.Drawing.Point(152, 46);
            this.connInitiatedField.Name = "connInitiatedField";
            this.connInitiatedField.Size = new System.Drawing.Size(41, 13);
            this.connInitiatedField.TabIndex = 13;
            this.connInitiatedField.Text = "label18";
            // 
            // resetConnField
            // 
            this.resetConnField.AutoSize = true;
            this.resetConnField.Location = new System.Drawing.Point(383, 16);
            this.resetConnField.Name = "resetConnField";
            this.resetConnField.Size = new System.Drawing.Size(41, 13);
            this.resetConnField.TabIndex = 12;
            this.resetConnField.Text = "label17";
            // 
            // connAcceptField
            // 
            this.connAcceptField.AutoSize = true;
            this.connAcceptField.Location = new System.Drawing.Point(152, 16);
            this.connAcceptField.Name = "connAcceptField";
            this.connAcceptField.Size = new System.Drawing.Size(41, 13);
            this.connAcceptField.TabIndex = 11;
            this.connAcceptField.Text = "label16";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(295, 137);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(82, 13);
            this.label11.TabIndex = 10;
            this.label11.Text = "Segments Sent:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(283, 107);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(94, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "Segments Resent:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(271, 79);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(106, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "Segments Received:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(309, 46);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(68, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Resets Sent:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(277, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(100, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "Reset Connections:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(30, 162);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(116, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Maximum Connections:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 137);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(139, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Failed Connection Attempts:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(60, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Errors Received:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Cumulative Connections:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Connections Initiated:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Connections Accepted:";
            // 
            // IPMonitorDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ipDisplay);
            this.Name = "IPMonitorDisplay";
            this.Size = new System.Drawing.Size(857, 497);
            this.ipDisplay.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tcpDisplay)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udpDisplay)).EndInit();
            this.statistics.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl ipDisplay;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView udpDisplay;
        private System.Windows.Forms.DataGridView tcpDisplay;
        private System.Windows.Forms.Label tcptotalField;
        private System.Windows.Forms.Label tcptotalLabel;
        private System.Windows.Forms.Label udptotalField;
        private System.Windows.Forms.Label udptotalLabel;
        private System.Windows.Forms.TabPage statistics;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label incDataErrField;
        private System.Windows.Forms.Label incDataDiscField;
        private System.Windows.Forms.Label dataSentField;
        private System.Windows.Forms.Label dataReceivedField;
        private System.Windows.Forms.Label segsSentField;
        private System.Windows.Forms.Label segsResentField;
        private System.Windows.Forms.Label segsReceivedField;
        private System.Windows.Forms.Label resetsSentField;
        private System.Windows.Forms.Label maxConnField;
        private System.Windows.Forms.Label failedConAttField;
        private System.Windows.Forms.Label errorsReceivedField;
        private System.Windows.Forms.Label cumulativeConnectionsField;
        private System.Windows.Forms.Label connInitiatedField;
        private System.Windows.Forms.Label resetConnField;
        private System.Windows.Forms.Label connAcceptField;

    }
}
