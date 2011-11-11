using System;
using System.Collections.Generic;
using System.Text;

namespace PassThru
{
		public class LogEvent: System.Object {
			public LogEvent(string mo, string me) {
				Module = mo;
				Message = me;
				time = DateTime.Now;
			}
			public string Message = null;
			public string Module = null;
			public DateTime time;
		}

		public class LogCenter: System.Object {
			LogCenter() {
			}

			public delegate void NewLogEvent(LogEvent e);
			static readonly object padlock = new object();

			public void Push(string Module, string Message) {
				LogEvent le = new LogEvent(Module, Message);
				SendLogEvent(le);
			}

			void SendLogEvent(LogEvent le) {
				if (PushLogEvent != null)
				{
						PushLogEvent(le);
				}
			}

			public static LogCenter Instance {
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

			public event NewLogEvent PushLogEvent;
		}
}
