using System;
using System.Collections.Generic;
using System.Text;

namespace PassThru
{
		class RuleUpdater
        {
            Dictionary<string, List<BasicFirewall.Rule>> rules = new Dictionary<string, List<BasicFirewall.Rule>>();
            List<BasicFirewall.Rule> globRules = new List<BasicFirewall.Rule>();
			RuleUpdater()             
            {

			}

			public delegate void GR(string pointer, List<BasicFirewall.Rule> rules);
			static readonly object padlock = new object();

			public void SetRules(string pointer, List<BasicFirewall.Rule> rules) 
            {
                this.globRules = new List<BasicFirewall.Rule>(rules);
			}

            public List<BasicFirewall.Rule> GetRules(string pointer)
            {
                return this.globRules;
            }

			public void UpdateRules(string pointer, List<BasicFirewall.Rule> rules) 
            {
                this.globRules = new List<BasicFirewall.Rule>(rules);
				if(GetRuleUpdates != null)
						GetRuleUpdates(pointer, rules);
			}

			public static RuleUpdater Instance {
				get 
                {
					lock (padlock)
					{
						return instance ?? (instance = new RuleUpdater());
					}
				}
			}
			static RuleUpdater instance = null;

			public event GR GetRuleUpdates;
		}
}
