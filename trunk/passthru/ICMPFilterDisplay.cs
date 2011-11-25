using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace PassThru
{
    /*
     * Main display class for ICMPFilter object
     */
    public partial class ICMPFilterDisplay : UserControl
    {
        // global filter obj
        private ICMPFilterModule filter;

        // table of rule mappings
        // key: type
        // value: list<string> of codes
        private Dictionary<string, List<string>> ruletable;

        // constructor initializes the local ICMPFilter object and the UI
        public ICMPFilterDisplay(ICMPFilterModule filter)
        {
            this.filter = filter;
            ruletable = new Dictionary<string, List<string>>(filter.RuleTable);
            InitializeComponent();
        }

        /*
         * Parse the input and add them to the textBox and list
         */
        private void button1_Click(object sender, EventArgs e)
        {
            List<string> temp = new List<String>();
            string[] codes;
            string type;

            // remove spaces and split on comma
            codes = codeField.Text.Replace(" ", string.Empty).Split(',');
            type = typeField.Text;

            // iterate through codes and add them to the table and hashtable
            foreach ( string s in codes )
            {
                // if that type already exists
                if (ruletable.ContainsKey(type))
                {
                    // get the list of the key
                    ruletable.TryGetValue(type, out temp);
                    // don't allow duplicate entries
                    if (temp.Contains(s))
                        continue;
                    // add the code to the list
                    temp.Add(s);
                    // remove the old kv pair
                    ruletable.Remove(type);
                    // add the new kv pair
                    ruletable.Add(type, temp);
                }

                // if the type doesn't exist yet, create it
                else
                {
                    temp.Add(s);
                    ruletable.Add(type, temp);
                }

                // add to table
                object[] row = { type, s, getDescription(type, s) };
                tableDisplay.Rows.Add(row);
            }
            
            // consume input
            codeField.Text = "";
            typeField.Text = "";

            // update our rule table
            UpdateRuleTable();
        }

        /*
         * Method handles the delete button
         * 
         * @param sender
         * @param e
         */
        private void button1_Click_1(object sender, EventArgs e)
        {
            // grab the type/code/rowIdx from the table
            int rowIdx = tableDisplay.SelectedCells[0].RowIndex;
            string type = tableDisplay["Type", rowIdx].Value.ToString();
            string code = tableDisplay["Code", rowIdx].Value.ToString();

            // if ruletable contains the key (it should)
            if (ruletable.ContainsKey(type))
            {
                List<string> temp;
                ruletable.TryGetValue(type, out temp);
                // check if the code list of the key contains the code
                if (temp.Contains(code))
                {
                    // remove code from temp list
                    temp.Remove(code);
                    // remove type from ruletable
                    ruletable.Remove(type);
                    // add it all back updated
                    ruletable.Add(type, temp);
                    // remove from display table
                    tableDisplay.Rows.Remove(tableDisplay.Rows[rowIdx]);
                }
            }

            // update our rule table
            UpdateRuleTable();
        }

        /*
         * Loads settings back up into the table
         * 
         * @param sender
         * @param e
         */
        private void ICMPFilterDisplay_Load(object sender, EventArgs e)
        {
            ruletable = new Dictionary<string, List<string>>(filter.RuleTable);
            ICollection keys = ruletable.Keys;
            List<string> lVal;

            // iterate through each type
            foreach (string s in keys)
            {
                // get the list of codes from the key
                ruletable.TryGetValue(s, out lVal);
                
                // iterate through the codes, adding them to the table
                // with the appropriate description
                foreach (string code in lVal)
                {
                    object[] row = { s, code, getDescription(s, code) };
                    tableDisplay.Rows.Add(row);
                }
            }

            // load up the deny all box
            if (filter.DenyAll)
            {
                allBox.CheckState = CheckState.Checked;
            }
        }

        // pushes update to the ruletable object
        private void UpdateRuleTable()
        {
            this.filter.RuleTable = ruletable;
        }

        /*
         * Method handles the All ICMP check box; 
         * Disables the type/code fields if selected, enables if not.
         * Updates the denyAll bool in the module.
         * Disables the add/delete button.
        */
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            typeField.Enabled = !(allBox.Checked);
            codeField.Enabled = !(allBox.Checked);
            addButton.Enabled = !(allBox.Checked);
            deleteButton.Enabled = !(allBox.Checked);
            this.filter.DenyAll = allBox.Checked;
        }
        
        /*
         * Method returns the ICMP description of a given type and code
         * http://www.iana.org/assignments/icmp-parameters/icmp-parameters.xml
         * 
         * @param type is the ICMP type 
         * @param code is the ICMP code
         */
        private string getDescription(string type, string code)
        {
            string description = "";
            switch (type)
            {
                // echo
                case "0":
                    if ( code.Equals("0"))
                        description = "Echo reply";
                    break;

                // reserve
                case "1": case "2":
                    description = "Reserved";
                    break;

                // destination unreachable
                case "3":
                    if (code.Equals("0"))
                        description = "Destination network unreachable";
                    else if (code.Equals("1"))
                        description = "Destination host unreachable";
                    else if (code.Equals("2"))
                        description = "Destination protocol unreachable";
                    else if (code.Equals("3"))
                        description = "Destination port unreachable";
                    else if (code.Equals("4"))
                        description = "Fragmentation required, and DF flag set";
                    else if (code.Equals("5"))
                        description = "Source route failed";
                    else if (code.Equals("6"))
                        description = "Destination network unknown";
                    else if (code.Equals("7"))
                        description = "Destination host unknown";
                    else if (code.Equals("8"))
                        description = "Source host isolated";
                    else if (code.Equals("9"))
                        description = "Network administratively prohibited";
                    else if (code.Equals("10"))
                        description = "Host administratively prohibited";
                    else if (code.Equals("11"))
                        description = "Network unreachable for TOS";
                    else if (code.Equals("12"))
                        description = "Host unreachable for TOS";
                    else if (code.Equals("13"))
                        description = "Communication administratively prohibited";
                    break;

                // source quench
                case "4":
                    if ( code.Equals("0"))
                        description = "Source quenched(congestion control)";
                    break;

                // redirect message
                case "5":
                    if (code.Equals("0"))
                        description = "Redirect datagram for the network";
                    else if (code.Equals("1"))
                        description = "Redirect datagram for the host";
                    else if (code.Equals("2"))
                        description = "Redirect datagram for the TOS & network";
                    else if (code.Equals("3"))
                        description = "Redirect datagram for the TOS & host";
                    break;

                // alt host addr
                case "6":
                    description = "Alternate host address";
                    break;
                
                // reserve
                case "7":
                    description = "Reserved";
                    break;

                // echo request
                case "8":
                    if (code.Equals("0"))
                        description = "Echo request (used to ping)";
                    break;

                // router advertisement
                case "9":
                    if (code.Equals("0"))
                        description = "Router advertisement";
                    break;

                // router solicitation
                case "10":
                    if (code.Equals("0"))
                        description = "Router discovery/selection/solicitation";
                    break;

                // time exceeded
                case "11":
                    if (code.Equals("0"))
                        description = "TTL expired in transit";
                    else if (code.Equals("1"))
                        description = "Fragment reassembly time exceeded";
                    break;

                // parameter problem
                case "12":
                    if (code.Equals("0"))
                        description = "Pointer indicates the error";
                    else if (code.Equals("1"))
                        description = "Missing a required option";
                    else if (code.Equals("2"))
                        description = "Bad length";
                    break;

                // timestamp
                case "13":
                    if (code.Equals("0"))
                        description = "Timestamp";
                    break;

                // timestamp reply
                case "14":
                    if (code.Equals("0"))
                        description = "Timestamp reply";
                    break;

                // information request
                case "15":
                    if (code.Equals("0"))
                        description = "Information request";
                    break;

                // information reply
                case "16":
                    if (code.Equals("0"))
                        description = "Information reply";
                    break;

                // address mask request
                case "17":
                    if (code.Equals("0"))
                        description = "Address mask request";
                    break;

                // address mask reply
                case "18":
                    if (code.Equals("0"))
                        description = "Address mask reply";
                    break;

                // Reserved
                case "19":
                    description = "Reserved for security";
                    break;

                // reserved
                case "20":
                case "21":
                case "22":
                case "23":
                case "24":
                case "25":
                case "26":
                case "27":
                case "28":
                case "29":
                    if (code.Equals("0"))
                        description = "Reserved for robustness experiment";
                    break;

                // traceroute
                case "30":
                    if (code.Equals("0"))
                        description = "Information request";
                    break;

                // datagram 
                case "31":
                    description = "datagram conversion error";
                    break;

                // mobile host redirect
                case "32":
                    description = "Mobile Host Redirect";
                    break;

                // where-are-you
                case "33":
                    description = "Where-Are-You (originally meant for IPv6)";
                    break;

                // here-i-am
                case "34":
                    description = "Here-I-Am (originally meant for ipv6)";
                    break;

                // mobile reg
                case "35":
                    description = "Mobile registration request";
                    break;

                // mobile reg
                case "36":
                    description = "Mobile registration reply";
                    break;

                // domain name request
                case "37":
                    description = "domain name request";
                    break;

                // domain name reply
                case "38":
                    description = "domain name reply";
                    break;
                
                // SKIP
                case "39":
                    description = "SKIP algorithm discovery protocol";
                    break;

                // Photuris
                case "40":
                    description = "Photuris, Security Failures";
                    break;

                // experimental ICMP
                case "41":
                    description = "ICMP for experimental mobility protocols";
                    break;

                // ICMP 42-255
                default:
                    description = "Reserved";
                    break;
            }

            return description;
        }
    }
}
