namespace PassThru
{
		partial class AdapterControl: System.Windows.Forms.UserControl {
			private System.Windows.Forms.DataGridViewTextBoxColumn AdapterName;
			private System.Windows.Forms.DataGridViewTextBoxColumn NIDescription;
			private System.Windows.Forms.DataGridViewTextBoxColumn NIIPv4Address;
			private System.Windows.Forms.DataGridViewTextBoxColumn NIIPv6Address;
			private System.Windows.Forms.DataGridViewTextBoxColumn NIName;

			/// <summary>
			/// Required designer variable.
			/// </summary>
			private System.ComponentModel.IContainer components = null;
			private System.Windows.Forms.DataGridView dataGridView1;

			/// <summary>
			/// Clean up any resources being used.
			/// </summary>
			/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
			protected override void Dispose(bool disposing) {
				if (disposing && (components != null))
				{
						components.Dispose();
				}
				base.Dispose(disposing);
			}

			/// <summary>
			/// Required method for Designer support - do not modify
			/// the contents of this method with the code editor.
			/// </summary>
			private void InitializeComponent() {
				this.dataGridView1 = new System.Windows.Forms.DataGridView();
				this.AdapterName = new System.Windows.Forms.DataGridViewTextBoxColumn();
				this.NIName = new System.Windows.Forms.DataGridViewTextBoxColumn();
				this.NIDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
				this.NIIPv4Address = new System.Windows.Forms.DataGridViewTextBoxColumn();
				this.NIIPv6Address = new System.Windows.Forms.DataGridViewTextBoxColumn();
				((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
				this.SuspendLayout();
				//
				// dataGridView1
				//
				this.dataGridView1.AllowUserToAddRows = false;
				this.dataGridView1.AllowUserToDeleteRows = false;
				this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
				this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
				this.AdapterName,
				this.NIName,
				this.NIDescription,
				this.NIIPv4Address,
				this.NIIPv6Address});
				this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
				this.dataGridView1.Location = new System.Drawing.Point(0, 0);
				this.dataGridView1.Name = "dataGridView1";
				this.dataGridView1.ReadOnly = true;
				this.dataGridView1.RowHeadersVisible = false;
				this.dataGridView1.Size = new System.Drawing.Size(413, 361);
				this.dataGridView1.TabIndex = 0;
				//
				// AdapterName
				//
				this.AdapterName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
				this.AdapterName.DataPropertyName = "DName";
				this.AdapterName.HeaderText = "Device Name";
				this.AdapterName.Name = "AdapterName";
				this.AdapterName.ReadOnly = true;
				this.AdapterName.Width = 97;
				//
				// NIName
				//
				this.NIName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
				this.NIName.DataPropertyName = "NIName";
				this.NIName.HeaderText = "Name";
				this.NIName.Name = "NIName";
				this.NIName.ReadOnly = true;
				this.NIName.Width = 60;
				//
				// NIDescription
				//
				this.NIDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
				this.NIDescription.DataPropertyName = "NIDescription";
				this.NIDescription.HeaderText = "Description";
				this.NIDescription.Name = "NIDescription";
				this.NIDescription.ReadOnly = true;
				this.NIDescription.Width = 85;
				//
				// NIIPv4Address
				//
				this.NIIPv4Address.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
				this.NIIPv4Address.DataPropertyName = "IPv4";
				this.NIIPv4Address.HeaderText = "IPv4 Address";
				this.NIIPv4Address.Name = "NIIPv4Address";
				this.NIIPv4Address.ReadOnly = true;
				this.NIIPv4Address.Width = 95;
				//
				// NIIPv6Address
				//
				this.NIIPv6Address.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
				this.NIIPv6Address.DataPropertyName = "IPv6";
				this.NIIPv6Address.HeaderText = "IPv6 Address";
				this.NIIPv6Address.Name = "NIIPv6Address";
				this.NIIPv6Address.ReadOnly = true;
				this.NIIPv6Address.Width = 95;
				//
				// AdapterControl
				//
				this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
				this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
				this.Controls.Add(this.dataGridView1);
				this.Name = "AdapterControl";
				this.Size = new System.Drawing.Size(413, 361);
				((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
				this.ResumeLayout(false);
			}
		}
}
