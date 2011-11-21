using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PassThru
{
		public class LogEvent
        {
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
                    Thread.Sleep(100);
                    lock (lpadlock)
                    {
                        if (logQueue.Count != 0)
                            SendLogEvent(logQueue.Dequeue());
                    }
                }
            }

            static Queue<LogEvent> logQueue = new Queue<LogEvent>();
            static object lpadlock = new object();

			public delegate void NewLogEvent(LogEvent e);
			static readonly object padlock = new object();

			public void Push(string Module, string Message) 
            {
				LogEvent le = new LogEvent(Module, Message);
                lock (lpadlock)
                {
                    logQueue.Enqueue(le);
                }
				//SendLogEvent(le);
			}

			static void SendLogEvent(LogEvent le) 
            {
				if (PushLogEvent != null)
				{
                    ti.AddLine(le.Message);
					PushLogEvent(le);
				}
			}

			public static LogCenter Instance 
            {
				get {
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
