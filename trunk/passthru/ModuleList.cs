using System.Collections.Generic;

namespace PassThru
{
		public class ModuleList
        {
            List<int> ProcessingIndex = new List<int>();
            List<FirewallModule> modules = new List<FirewallModule>();

			public ModuleList() 
            {
			}

			public void AddModule(FirewallModule fm) 
            {
				modules.Add(fm);
				ProcessingIndex.Add(modules.Count - 1);
			}

			public FirewallModule GetModule(int index) 
            {
				return modules[ProcessingIndex[index]];
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
