using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router_Signalstärke_Auslesen.Model
{
    public class Settings
    {
        private static Settings? _instance;

        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (!LoadSettings()) { 
                        throw new InvalidOperationException("Settings wurden noch nicht geladen. Bitte rufen Sie LoadSettings() auf.");
                    }
                }
                return _instance!;
            }
        }

        public bool BrowserGUI { get;  set; } = true;
        public int SignalIntervalInMilliseconds { get;  set; } = 5000;
        public string BrowserAgent { get;  set; } = "msedge";
        public List<string> BrowserStartArgs { get;  set; }
        public int LoginMaxAttempts { get;  set; } = 15;
        public int LoginDelayMilliseconds { get;  set; } = 300;
        public string SignalPaserRegex { get;  set; } = "[^0-9+-]";
        public List<BrowserInstanceSetting> BrowserInstanceSettings { get;  set; }


        public static bool LoadSettings()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .Build();

                // Erstelle eine Instanz von Settings und binde die Konfiguration
                _instance = new Settings();
                configuration.GetSection("settings").Bind(_instance);

                Console.WriteLine(_instance.ToString());

                return _instance != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Einstellungen: {ex.Message}");
                return false;
            }
        }

        public override string ToString()
        {
            // Formatiere die Eigenschaften zu einer lesbaren String-Darstellung
            var browserStartArgs = BrowserStartArgs != null ? string.Join(", ", BrowserStartArgs) : "None";
            var browserInstanceSettings = BrowserInstanceSettings != null ? BrowserInstanceSettings.Count.ToString() : "None";

            return $"Settings:\n" +
                   $"- BrowserGUI: {BrowserGUI}\n" +
                   $"- SignalIntervalInMilliseconds: {SignalIntervalInMilliseconds} ms\n" +
                   $"- BrowserAgent: {BrowserAgent}\n" +
                   $"- BrowserStartArgs: {browserStartArgs}\n" +
                   $"- LoginMaxAttempts: {LoginMaxAttempts}\n" +
                   $"- LoginDelayMilliseconds: {LoginDelayMilliseconds} ms\n" +
                   $"- SignalPaserRegex: {SignalPaserRegex}\n" +
                   $"- BrowserInstanceSettings: {browserInstanceSettings.ToString()} instances";
        }
    }
}
