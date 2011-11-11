using System;
using System.Collections.Generic;
using System.Text;

namespace PassThru
{
		class PcapCreator: System.Object {
			PcapCreator() {
				last = DateTime.Now;
			}
			DateTime last;
			static readonly object padlock = new object();

			public string GetNewDate() {
				while (DateTime.Now == last)
				{
						System.Threading.Thread.Sleep(1);
				}
				last = DateTime.Now;
				return last.Month.ToString() + "-" + last.Day.ToString() + "-" + last.Year.ToString() + "_" + last.Hour.ToString() + "-" + last.Minute.ToString() + "-" + last.Second.ToString() + "-" + last.Millisecond.ToString();
			}

			public static PcapCreator Instance {
				get {
					lock (padlock)
					{
							return instance ?? (instance = new PcapCreator());
					}
				}
			}
			static PcapCreator instance = null;
		}
}
