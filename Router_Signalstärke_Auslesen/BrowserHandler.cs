using Microsoft.Playwright;
using Router_Signalstärke_Auslesen.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Router_Signalstärke_Auslesen
{
    public class BrowserHandler
    {
        private IPage _page;
        private BrowserInstanceSetting _browserInstanceSetting;
        private float _freshSignalStrength = float.MinValue;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="BrowserHandler"/>-Klasse.
        /// </summary>
        /// <param name="page">
        /// Die <see cref="IPage"/>-Instanz, die für die Interaktion mit der Webseite verwendet wird.
        /// </param>
        /// <param name="browserInstanceSetting">
        /// Die Einstellungen für die Browser-Instanz, die zur API-Kommunikation genutzt werden.
        /// </param>
        public BrowserHandler(IPage page, BrowserInstanceSetting browserInstanceSetting)
        {
            _page = page;
            _browserInstanceSetting = browserInstanceSetting;
        }

        /// <summary>
        /// Führt einen automatisierten Login-Vorgang im Browser durch, indem  
        /// das Passwort Zeichen für Zeichen eingegeben und das Login-Element geklickt wird.
        /// </summary>
        /// <param name="delayMilliseconds">
        /// Die Wartezeit in Millisekunden zwischen den Login-Versuchen (Standard: 300 ms).
        /// </param>
        /// <param name="maxVersuche">
        /// Die maximale Anzahl an Versuchen, bevor der Login als fehlgeschlagen gilt (Standard: 15).
        /// </param>
        /// <returns>
        /// Ein <see cref="Task{TResult}"/> mit einem booleschen Wert:  
        /// <c>true</c>, wenn der Login erfolgreich war, andernfalls <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Die Methode führt ein JavaScript-Skript aus, das das Passwortfeld füllt  
        /// und den Login-Button betätigt. Falls der Login fehlschlägt, werden  
        /// bis zu <paramref name="maxVersuche"/> Wiederholungen durchgeführt,  
        /// mit einer Pause von <paramref name="delayMilliseconds"/> zwischen den Versuchen.
        /// Fehler werden protokolliert.
        /// </remarks>
        public async Task<bool> LoginAsync(int delayMilliseconds = 300, int maxVersuche = 15)
        {
            var jsCode = @"
  (async function(password) {
      var obj_txtPwd = document.getElementById('txtPwd');
      var obj_txtPwdShow = document.getElementById('btnLogin');
      
      if (!obj_txtPwd || !obj_txtPwdShow) {
          console.error('[Info-login] Eines oder beide Elemente wurden nicht gefunden:', {
              txtPwd: obj_txtPwd,
              btnLogin: obj_txtPwdShow
          });
          return false;
      }

      obj_txtPwd.value = '';
      obj_txtPwd.dispatchEvent(new Event('input'));

      obj_txtPwd.focus();
      obj_txtPwd.select();
      
      for (let char of password) {
          obj_txtPwd.value += char;  
          obj_txtPwd.dispatchEvent(new KeyboardEvent('keypress', { key: char })); 
          await new Promise(resolve => setTimeout(resolve, 100)); 
      }
      obj_txtPwd.dispatchEvent(new Event('input'));

      obj_txtPwdShow.focus();
      obj_txtPwdShow.select();
      obj_txtPwdShow.click();

      return true;
  })('" + _browserInstanceSetting.Password + @"');
";

            try
            {
                for (int versuch = 1; versuch <= maxVersuche; versuch++)
                {
                    try
                    {
                        var result = await _page.EvaluateAsync<bool>(jsCode);

                        if (result)
                        {
                            Console.WriteLine("Login erfolgreich.");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Login fehlgeschlagen.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Fehler bei Versuch {versuch}: {ex.Message}");
                    }

                    if (versuch < maxVersuche)
                    {
                        Console.WriteLine($"Versuch {versuch} von {maxVersuche} - Wartet {delayMilliseconds} ms...");
                        await Task.Delay(delayMilliseconds);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Ausführen des JavaScript-Codes: " + ex.Message);
                return false;
            }
            return false;
        }


        /// <summary>
        /// Ruft die Signalstärke als Text von der Webseite ab.
        /// </summary>
        /// <returns>
        /// Ein <see cref="Task{TResult}"/> mit einem String, der die Signalstärke beschreibt.  
        /// Gibt <c>null</c> zurück, wenn das Element nicht gefunden wird.
        /// </returns>
        /// <remarks>
        /// Die Methode sucht nach einem HTML-Element mit der ID <c>#fresh_signal_strength</c>  
        /// und liest dessen Textinhalt aus. Falls das Element nicht existiert, wird <c>null</c> zurückgegeben.
        /// </remarks>
        public async Task<string> GetSignalStrengthTextAsync()
        {
            var signalElement = await _page.QuerySelectorAsync("#fresh_signal_strength");

            // Wenn das Element vorhanden ist, den Textinhalt zurückgeben
            if (signalElement != null)
            {
                var signalText = await signalElement.TextContentAsync();
                Console.WriteLine($"Signalstärke (Text): {signalText}");
                return signalText!;
            }
            else
            {
                Console.WriteLine("Signalstärkenelement nicht gefunden.");
                return null!;
            }
        }


        /// <summary>
        /// Sendet die Signalstärke an eine API, sofern sich der Wert geändert hat.
        /// </summary>
        /// <param name="signal">
        /// Der Signalstärkewert als Zeichenkette.  
        /// Nicht numerische Zeichen werden herausgefiltert.
        /// </param>
        /// <remarks>
        /// Die Methode bereinigt den Eingabestring von unerwünschten Zeichen und prüft,  
        /// ob der Wert eine gültige Zahl ist. Falls der Wert sich seit der letzten  
        /// Übermittlung nicht geändert hat, wird kein erneuter Request gesendet.  
        /// Andernfalls wird die API-URL mit der aktuellen Signalstärke gebildet  
        /// und eine GET-Anfrage gesendet.  
        /// Eventuelle Fehler werden in der Konsole ausgegeben.
        /// </remarks>
        public async Task SendDatenAsync(string signal)
        {
            if (!String.IsNullOrEmpty(Settings.Instance.SignalPaserRegex))
            {
                signal = Regex.Replace(signal, Settings.Instance.SignalPaserRegex, "");
            }

            if (string.IsNullOrEmpty(signal) || !float.TryParse(signal, out float signalStrength))
            {
                Console.WriteLine("[Fehler-sendDaten] Ungültiger Signalwert.");
                return;
            }

            if (_freshSignalStrength == signalStrength)
            {
                return;
            }

            try
            {
                var url = _browserInstanceSetting.Endpoint.Replace("{signalStrength}", signalStrength.ToString());
                Console.WriteLine("API-Entpoint url: " + url);
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                using var client = new HttpClient(handler);
                var response = await client.GetStringAsync(url);

                Console.WriteLine("Antwort vom API-Entpoint: " + response);
                _freshSignalStrength = signalStrength;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Senden der Daten: {ex.Message}");
            }
        }
    }
}
