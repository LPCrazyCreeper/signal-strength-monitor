# 📡 Signal Strength Monitor

## 📌 Projektbeschreibung

Das Projekt dient dazu, sich auf einer Router-Seite des Herstellers (ZTE 5G Outdoor CPE) anzumelden und die Signalstärke an einen REST-Endpoint zur Analyse zu senden. Die Anwendung nutzt das Browser-Automatisierungs-Framework [Playwright](https://playwright.dev/docs/api/class-browsertype), um sich einzuloggen, relevante Daten zu extrahieren und diese periodisch an einen definierten Endpoint zu senden.

## ⚙️ Konfiguration (`config.json`)

Die Konfigurationsdatei (`config.json`) enthält verschiedene Einstellungen zur Steuerung des Verhaltens der Anwendung:

### 🌍 **Allgemeine Einstellungen**

```json
"settings": {
    "BrowserGUI": true,
    "BrowserAgent": "msedge",
    "SignalIntervalInMilliseconds": 5000,
    "LoginMaxAttempts": 15,
    "LoginDelayMilliseconds": 300,
    "SignalPaserRegex": "[^0-9+-]",
    "BrowserStartArgs": [
        "--ignore-certificate-errors",
        "--disable-web-security"
    ]
}
```

#### **Erklärung der Parameter:**

- **`BrowserGUI`** *(true/false)* – Gibt an, ob der Browser mit GUI oder im Headless-Modus gestartet wird.
- **`BrowserAgent`** *(String)* – Definiert den User-Agent für den Browser (z. B. `msedge`).
- **Unterstützte ********************************************************`BrowserAgent`********************************************************-Werte:**
  - `chromium`
  - `firefox`
  - `webkit`
  - `msedge`
- **`SignalIntervalInMilliseconds`** *(int)* – Zeitintervall in Millisekunden, in dem die Signalstärke ausgelesen wird.
- **`LoginMaxAttempts`** *(int)* – Maximale Anzahl an Anmeldeversuchen, bevor die Anwendung aufgibt.
- **`LoginDelayMilliseconds`** *(int)* – Wartezeit in Millisekunden zwischen den Login-Versuchen.
- **`SignalPaserRegex`** *(String)* – Regex für das Filtern der Signalstärke-Werte (z. B. entfernt unerwünschte Zeichen).
- **`BrowserStartArgs`** *(Array)* – Zusätzliche Startparameter für den Browser:
  - `--ignore-certificate-errors` → Ignoriert SSL-Zertifikatsfehler
  - `--disable-web-security` → Deaktiviert Sicherheitsrichtlinien (nur für lokale Tests empfohlen)

### 🔒 **Browser-Instanzen Konfiguration**

```json
"browserInstanceSettings": [
    {
        "password": "Password123",
        "loginPage": "https://example.de/login.html",
        "endpoint": "https://example.de/index.php?content=<prtg><result><channel>Empfangspegel</channel><value>{signalStrength}</value><float>1</float></result></prtg>",
        "enable": true
    }
]
```

#### **Erklärung der Parameter:**

- **`password`** *(String)* – Passwort für den Login auf der Router-Seite.
- **`loginPage`** *(String)* – URL der Login-Seite des Routers (kann auch eine lokale Datei sein).
- **`endpoint`** *(String)* – Ziel-URL, an die die Signalstärke gesendet wird. Der Platzhalter {signalStrength} wird später im Code mit dem Ausgelesenden wert ersätzt.
- **`enable`** *(true/false)* – Gibt an, ob diese Konfiguration aktiv ist.

## 🚀 Installation & Nutzung

### **1️⃣ Voraussetzungen**

- .NET 8 installiert
- [Playwright](https://playwright.dev/docs/api/class-browsertype)

### **2️⃣ Projekt klonen & starten**

```sh
git clone https://github.com/LPCrazyCreeper/signal-strength-monitor.git
cd signal-strength-monitor
dotnet run
```

## 🛠️ To-Do / Weiterentwicklung

-

## 📜 Lizenz

Dieses Projekt steht unter der **MIT-Lizenz**. Das bedeutet:

- Es darf kostenlos verwendet, modifiziert und weitergegeben werden.
- Es besteht keine Garantie oder Haftung.
- Eine Namensnennung des ursprünglichen Autors ist erwünscht, aber nicht verpflichtend.

---

📅 **Letzte Aktualisierung**: `$(date +%Y-%m-%d)`\

