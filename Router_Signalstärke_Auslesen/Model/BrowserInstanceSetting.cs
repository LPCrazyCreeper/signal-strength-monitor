using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router_Signalstärke_Auslesen.Model
{
    public class BrowserInstanceSetting
    {
        public string Password { get; set; }
        public string LoginPage { get; set; }
        public string Endpoint { get; set; }
        public bool Enable { get; set; } = false;
    }
}
