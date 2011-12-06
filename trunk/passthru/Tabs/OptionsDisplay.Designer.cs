namespace PassThru
{
    partial class OptionsDisplay
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
            this.displayTrayLogs = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // displayTrayLogs
            // 
            this.displayTrayLogs.AutoSize = true;
            this.displayTrayLogs.Checked = true;
            this.displayTrayLogs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.displayTrayLogs.Location = new System.Drawing.Point(3, 3);
            this.displayTrayLogs.Name = "displayTrayLogs";
            this.displayTrayLogs.Size = new System.Drawing.Size(123, 17);
            this.displayTrayLogs.TabIndex = 0;
            this.displayTrayLogs.Text = "Display Icon Popups";
            this.displayTrayLogs.UseVisualStyleBackColor = true;
            this.displayTrayLogs.CheckedChanged += new System.EventHandler(this.displayTrayLogs_CheckedChanged);
            // 
            // OptionsDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.displayTrayLogs);
            this.Name = "OptionsDisplay";
            this.Size = new System.Drawing.Size(555, 330);
            this.Load += new System.EventHandler(this.OptionsDisplay_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.CheckBox displayTrayLogs;



    }
}
