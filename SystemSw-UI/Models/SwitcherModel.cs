using System.Collections.Generic;

namespace SystemSw_UI.Models
{
    public class SwitcherModel
    {
        public Dictionary<string, string> Mappings { get; set; }
    
        public int Channels { get; set; }

        public int ActiveChannel { get; set; }

    }
}