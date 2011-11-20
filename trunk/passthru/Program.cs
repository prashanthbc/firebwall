/*************************************************************************/
/*				Copyright (c) 2000-2010 NT Kernel Resources.		     */
/*                           All Rights Reserved.                        */
/*                          http://www.ntkernel.com                      */
/*                           ndisrd@ntkernel.com                         */
/*                                                                       */
/* Module Name:  PassThru main module                                    */
/*                                                                       */
/* Abstract: Defines the entry point for the console application         */
/*                                                                       */
/*************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Net;
using NdisapiSpace;
using Win32APISPace;

namespace PassThru
{
		class Program: System.Object {
			public static LookupService ls = new LookupService(LookupService.GEOIP_STANDARD);
			public static MainWindow mainWindow;
			public static TrayIcon tray;
			static bool Running = true;

			public static void Close(object o, EventArgs ea) {
				NetworkAdapter.ShutdownAll();
				mainWindow.Exit();
				Running = false;
				tray.Dispose();
			}

			static void Main(string[] args) 
            {
                tray = new TrayIcon();
                mainWindow = new MainWindow();
                foreach (NetworkAdapter ni in NetworkAdapter.GetAllAdapters())
                {
                    ni.StartProcessing();
                }
                Application.Run(mainWindow);
                while (Running)
                    Thread.Sleep(100);
			}
		}
}
