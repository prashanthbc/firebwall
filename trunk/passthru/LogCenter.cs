﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace PassThru
{
        /*
         * Object used to describe an incoming event
         * houses local vars and constructor
         */
		public class LogEvent
        {
            /*
             * @param mo is the module of the log
             * @param me is the message to log
             */
			public LogEvent(string mo, string me) 
            {
				Module = mo;
				Message = me;
				time = DateTime.Now;
			}
			public string Message = null;
			public string Module = null;
			public DateTime time;
		}

		public class LogCenter
        {
            static Thread pusher = new Thread(new ThreadStart(PushLoop));
            public static TrayIcon ti = new TrayIcon();

			LogCenter() 
            {
                pusher.Start();
			}

            public static void Kill()
            {
                pusher.Abort();
            }

            static void PushLoop()
            {
                while (true)
                {
                    Thread.Sleep(50);
                    LogEvent[] temp = null;
                    lock (lpadlock)
                    {
                        if (logQueue.Count != 0)
                        {
                            temp = logQueue.ToArray();
                            logQueue.Clear();
                        }
                    }
                    if (temp != null)
                    {
                        foreach (LogEvent le in temp)
                            SendLogEvent(le);
                    }
                }
            }

            static Queue<LogEvent> logQueue = new Queue<LogEvent>();
            static object lpadlock = new object();

			public delegate void NewLogEvent(LogEvent e);
			static readonly object padlock = new object();

            // generates the LogEvent object and pushes it out to be logged
			public void Push(string Module, string Message) 
            {
				LogEvent le = new LogEvent(Module, Message);
                lock (lpadlock)
                {
                    logQueue.Enqueue(le);
                }
			}

            /*
             * pushes a log message to the tray icon, as well as the
             * log window
             * 
             * @param le is the log event to be logged
             */
			static void SendLogEvent(LogEvent le) 
            {
				if (PushLogEvent != null)
				{
                    ti.AddLine(le.Message);
					PushLogEvent(le);
                    WriteLogFile(le);
				}
			}

            /*
             * pushes log events to the event log file
             * Log\Event_<date>.log
             * 
             * These logs are purged by the cleanLogs() method.
             */
            private static void WriteLogFile(LogEvent le)
            {
                string currentdate = DateTime.Now.ToString("M-d-yyyy");
                string filepath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Log";
                string filename = "\\Event_" + currentdate + ".log";

                // if the log event is not null
                if (le != null)
                {
                    // if the Log folder exists already
                    if (Directory.Exists(filepath))
                    {
                        // if todays log file exists
                        if (File.Exists(filepath + filename))
                        {
                            // log the event in todays log
                            File.AppendAllText(filepath + filename, le.time.ToString() +
                                " " + le.Module + ": " + le.Message + "\r\n");
                        }
                        // if todays log file does not exist, create and write to it
                        else
                        {
                            File.Create(filepath + filename);
                            File.WriteAllText(filepath + filename, le.time.ToString() +
                                " " + le.Module + ": " + le.Message + "\r\n");
                        }
                    }
                    // if the log path does not exist, create it and write out the log
                    else
                    {
                        Directory.CreateDirectory(filepath);
                        File.WriteAllText(filepath + filename, le.time.ToString() +
                            " " + le.Module + ": " + le.Message + "\r\n");
                    }
                }
            }

            /*
             * Method called once per run, used to check log paths to 
             * clean up any old logs.  Called from MainWindow.cs - Load().
             * 
             * Logs are retained for up to 5 days before being purged.
             */
            public static void cleanLogs()
            {
                string filepath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Log";

                if (Directory.Exists(filepath))
                {
                    // grab all the logs in the directory
                    string[] files = Directory.GetFiles(filepath);
                    
                    // iterate through them all looking for any that are old (>5)
                    foreach (string s in files)
                    {
                        // grab the log date from file path name and 
                        // convert to DateTime for day check
                        string logdate = s.Substring(s.IndexOf("_")+1, 10);
                        DateTimeFormatInfo dtfi = new DateTimeFormatInfo();
                        dtfi.ShortDatePattern = "MM-dd-yyyy";
                        dtfi.DateSeparator = "-";
                        DateTime logDate = Convert.ToDateTime(logdate, dtfi);

                        // if it's old, get rid of it
                        if ((DateTime.Now - logDate).Days > 5)
                        {
                            File.Delete(s);
                        }
                    }
                }
            }
            
            public static LogCenter Instance 
            {
				get 
                {
					lock (padlock)
					{
						if (instance==null)
						{
								instance = new LogCenter();
						}
						return instance;
					}
				}
			}
			static LogCenter instance = null;

			public static event NewLogEvent PushLogEvent;
		}
}
