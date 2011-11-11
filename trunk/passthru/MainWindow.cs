using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PassThru
{
		public partial class MainWindow: Form {
			public MainWindow() {
				InitializeComponent();                
			}

			public void Exit() {
				Application.Exit();
			}

			void AddLogEvent(object le) {
				if (textBox1.InvokeRequired)
				{
						System.Threading.ParameterizedThreadStart d = new System.Threading.ParameterizedThreadStart(AddLogEvent);
						textBox1.Invoke(d, new object[] { le });
				}
				else
				{
						LogEvent e = (LogEvent)le;
						textBox1.Text = e.time.ToString() + " " + e.Module + ": " + e.Message + "\r\n" + textBox1.Text;
				}
			}

			void Instance_PushLogEvent(LogEvent e) {
				AddLogEvent(e);
			}

			private void MainWindow_FormClosing(object sender, FormClosingEventArgs e) {
				if (e.CloseReason == CloseReason.UserClosing)
				{
						this.Visible = false;
						e.Cancel = true;
				}
			}

			private void MainWindow_Load(object sender, EventArgs e) {
				LogCenter.Instance.PushLogEvent += new LogCenter.NewLogEvent(Instance_PushLogEvent);
				//RuleEditor re = new RuleEditor();
				//re.Dock = DockStyle.Fill;
				//tabPage2.Controls.Add(re);

                RuleEditor re = new RuleEditor(null);
                re.Dock = DockStyle.Fill;
                tabPage2.Controls.Add(re);

				AdapterControl ac = new AdapterControl();
				ac.Dock = DockStyle.Fill;
				tabPage3.Controls.Add(ac);
			}
		}
}
