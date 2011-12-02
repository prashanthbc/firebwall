namespace PassThru
{
		partial class MainWindow: System.Windows.Forms.Form {
			/// <summary>
			/// Required designer variable.
			/// </summary>
			private System.ComponentModel.IContainer components = null;
			private System.Windows.Forms.TabControl tabControl1;
            private System.Windows.Forms.TabPage tabPage1;
			private System.Windows.Forms.TabPage tabPage3;
			private System.Windows.Forms.TextBox textBox1;

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
                this.textBox1 = new System.Windows.Forms.TextBox();
                this.tabControl1 = new System.Windows.Forms.TabControl();
                this.tabPage1 = new System.Windows.Forms.TabPage();
                this.tabPage3 = new System.Windows.Forms.TabPage();
                this.tabControl1.SuspendLayout();
                this.tabPage1.SuspendLayout();
                this.SuspendLayout();
                // 
                // textBox1
                // 
                this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
                this.textBox1.Location = new System.Drawing.Point(3, 3);
                this.textBox1.Multiline = true;
                this.textBox1.Name = "textBox1";
                this.textBox1.ReadOnly = true;
                this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
                this.textBox1.Size = new System.Drawing.Size(462, 315);
                this.textBox1.TabIndex = 0;
                this.textBox1.WordWrap = false;
                // 
                // tabControl1
                // 
                this.tabControl1.Controls.Add(this.tabPage1);
                this.tabControl1.Controls.Add(this.tabPage3);
                this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
                this.tabControl1.Location = new System.Drawing.Point(0, 0);
                this.tabControl1.Name = "tabControl1";
                this.tabControl1.SelectedIndex = 0;
                this.tabControl1.Size = new System.Drawing.Size(476, 347);
                this.tabControl1.TabIndex = 1;
                // 
                // tabPage1
                // 
                this.tabPage1.Controls.Add(this.textBox1);
                this.tabPage1.Location = new System.Drawing.Point(4, 22);
                this.tabPage1.Name = "tabPage1";
                this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
                this.tabPage1.Size = new System.Drawing.Size(468, 321);
                this.tabPage1.TabIndex = 0;
                this.tabPage1.Text = "Log";
                this.tabPage1.UseVisualStyleBackColor = true;
                // 
                // tabPage3
                // 
                this.tabPage3.Location = new System.Drawing.Point(4, 22);
                this.tabPage3.Name = "tabPage3";
                this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
                this.tabPage3.Size = new System.Drawing.Size(468, 321);
                this.tabPage3.TabIndex = 2;
                this.tabPage3.Text = "Adapters";
                this.tabPage3.UseVisualStyleBackColor = true;
                // 
                // MainWindow
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(476, 347);
                this.Controls.Add(this.tabControl1);
                this.Name = "MainWindow";
                this.Text = "fireBwall v0.3.2.9";
                this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
                this.Load += new System.EventHandler(this.MainWindow_Load);
                this.tabControl1.ResumeLayout(false);
                this.tabPage1.ResumeLayout(false);
                this.tabPage1.PerformLayout();
                this.ResumeLayout(false);

            }
		}
}
