using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace PassThru
{
		class TrayIcon
        {
			public TrayIcon() 
            {                

				ContextMenu cm = new ContextMenu();
				MenuItem closeButton = new MenuItem("Exit", new EventHandler(Program.Close));
				cm.MenuItems.Add(closeButton);
				tray = new NotifyIcon();
				tray.ContextMenu = cm;
                Assembly target = Assembly.GetExecutingAssembly();
                tray.Icon = new System.Drawing.Icon(target.GetManifestResourceStream("PassThru.Resources.HoneyPorts.ico"));
				tray.Visible = true;
				tray.BalloonTipClosed += new EventHandler(tray_BalloonTipClosed);
				tray.DoubleClick += new EventHandler(tray_DoubleClick);
			}
			NotifyIcon tray;

            Queue<string> lines = new Queue<string>();

			public void AddLine(string line) 
            {
				tray.BalloonTipText = "";
				if (lines.Count == 5)
				{
						lines.Dequeue();
				}
				lines.Enqueue(line);

				for (int x = lines.Count - 1; x >= 0; x-- )
				{
						tray.BalloonTipText += lines.ToArray()[x] + "\r\n";
				}

				tray.ShowBalloonTip(5000);
			}

			public void Dispose() 
            {
				tray.Dispose();
			}

			void tray_BalloonTipClosed(object sender, EventArgs e) 
            {
				lines.Clear();
				tray.BalloonTipText = "";
			}

			void tray_DoubleClick(object sender, EventArgs e) 
            {
				if (Program.mainWindow.Visible)
						Program.mainWindow.Visible = false;
				else
						Program.mainWindow.Visible = true;

				Program.mainWindow.Activate();
			}
		}
}
