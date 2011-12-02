using System.Collections.Generic;

namespace PassThru
{
		public class ModuleList
        {
            List<int> ProcessingIndex = new List<int>();
            List<FirewallModule> modules = new List<FirewallModule>();
            object padlock = new object();

			public ModuleList() 
            {
			}

            public void InsertPIndex(int oIndex, int nIndex)
            {
                if (oIndex == nIndex) return;
                lock (padlock)
                {
                    int toMove = ProcessingIndex[oIndex];
                    ProcessingIndex.RemoveAt(oIndex);
                    ProcessingIndex.Insert(nIndex, toMove);
                }
            }

			public void AddModule(FirewallModule fm) 
            {
                lock (padlock)
                {
                    modules.Add(fm);
                    ProcessingIndex.Add(modules.Count - 1);
                }
			}

			public FirewallModule GetModule(int index) 
            {
                lock (padlock)
                {
                    return modules[ProcessingIndex[index]];
                }
			}

			public int Count 
            {
				get 
                {
					return modules.Count;
				}
			}
		}
}
