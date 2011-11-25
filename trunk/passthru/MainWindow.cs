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

            /*
             * Object handles logging of a log event to the window
             * @param le is the log event object to be logged
             */
			void AddLogEvent(object le) {
                // if the logger is busy, invoke it
				if (textBox1.InvokeRequired)
				{
						System.Threading.ParameterizedThreadStart d = new System.Threading.ParameterizedThreadStart(AddLogEvent);
						textBox1.Invoke(d, new object[] { le });
				}
                // else log the message
				else
				{
						LogEvent e = (LogEvent)le;
						textBox1.Text = e.time.ToString() + " " + e.Module + ": " + e.Message + "\r\n" + textBox1.Text;
				}
			}

            // receives a log event and pushes it to AddLogEvent
			void Instance_PushLogEvent(LogEvent e) {
				AddLogEvent(e);
			}
			
            private void MainWindow_FormClosing(object sender, FormClosingEventArgs e) 
            {
				if (e.CloseReason == CloseReason.UserClosing)
				{
					this.Visible = false;
					e.Cancel = true;
                    ac.Kill();
				}
			}

            //RuleEditor re;
            AdapterControl ac;

			private void MainWindow_Load(object sender, EventArgs e) 
            {
                System.Reflection.Assembly target = System.Reflection.Assembly.GetExecutingAssembly();
                this.Icon = new System.Drawing.Icon(target.GetManifestResourceStream("PassThru.Resources.HoneyPorts.ico"));
				LogCenter.PushLogEvent += new LogCenter.NewLogEvent(Instance_PushLogEvent);
                
                // call the log purger
                LogCenter.cleanLogs();
				//RuleEditor re = new RuleEditor();
				//re.Dock = DockStyle.Fill;
				//tabPage2.Controls.Add(re);

                //re = new RuleEditor(null);
                //re.Dock = DockStyle.Fill;
                //tabPage2.Controls.Add(re);

				ac = new AdapterControl();
				ac.Dock = DockStyle.Fill;
				tabPage3.Controls.Add(ac);
			}
		}
}
