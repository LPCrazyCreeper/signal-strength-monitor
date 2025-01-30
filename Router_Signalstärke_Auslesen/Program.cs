using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Router_Signalstärke_Auslesen;
using Router_Signalstärke_Auslesen.Model;
using System;

namespace MyApp
{
    internal class Router_Signalstärke_Auslesen
    {
        class Program
        {


            /// <summary>
            /// Hauptmethode zum Starten der Browser-Instanzen und Verarbeitung der Signalstärke.
            /// </summary>
            /// <remarks>
            /// 1. Lädt die Einstellungen und bricht ab, falls dies fehlschlägt.  
            /// 2. Initialisiert Playwright und startet den Browser.  
            /// 3. Durchläuft die konfigurierten Browser-Instanzen.  
            /// 4. Führt den Login durch und prüft den Erfolg.  
            /// 5. Erfasst alle <paramref name="x"/> Sekunden die Signalstärke und sendet sie an die REST-API.  
            /// 6. Wartet auf alle laufenden Instanzen und schließt den Browser nach Abschluss.
            /// </remarks>
            static async Task Main(string[] args)
            {
                if (!Settings.LoadSettings())
                {
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine(Settings.Instance.ToString());

                // Playwright Initialisierung
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Channel = Settings.Instance?.BrowserAgent,
                    Args = Settings.Instance?.BrowserStartArgs,
                    Headless = !Settings.Instance?.BrowserGUI
                });

                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    IgnoreHTTPSErrors = true
                });

                // Erstelle eine Liste, um alle Tasks zu speichern
                List<Task> browserTasks = new List<Task>();

                foreach (BrowserInstanceSetting browserSetting in Settings.Instance?.BrowserInstanceSettings)
                {
                    if (!browserSetting.Enable) //Wenn er die Settings nicht brauch muss die Webseite auch nicht geöffnet werden.
                    {
                        continue;
                    }

                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    CancellationToken token = cancellationTokenSource.Token;
                    var task = Task.Run(async () =>
                    {
                        IPage page = await browser.NewPageAsync();
                        try
                        {
                            await page.GotoAsync(browserSetting.LoginPage);
                        }
                        catch (Exception ex) { 
                            Console.WriteLine(ex);
                            cancellationTokenSource.Cancel();
                            return;
                        }

                        BrowserHandler browserHandler = new BrowserHandler(page, browserSetting);
                        if(await browserHandler.LoginAsync(Settings.Instance.LoginDelayMilliseconds, Settings.Instance.LoginMaxAttempts))
                        {
                            while (true)
                            {
                                try
                                {
                                    string signalText = await browserHandler.GetSignalStrengthTextAsync();
                                    if (signalText != null)
                                    {
                                        await browserHandler.SendDatenAsync(signalText);
                                    }
                                    await Task.Delay(Settings.Instance.SignalIntervalInMilliseconds);
                                }
                                catch (Exception ex) { Console.WriteLine(ex); }
                            }
                        }
                    }, token);
                    browserTasks.Add(task);
                }

                await Task.WhenAll(browserTasks);

                // Schließe den Browser, wenn alle Tasks fertig sind
                await browser.CloseAsync();
                Console.WriteLine("Alle Browser-Tasks beendet.");
            }
        }
    }
}