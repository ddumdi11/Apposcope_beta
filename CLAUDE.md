# Claude Code Entwicklungsnotizen - Apposcope Beta

## 🎯 Projekt-Übersicht
**Typ:** .NET 6 WPF Desktop-Anwendung  
**Zweck:** Dual-Monitor UI-Automatisierung mit Action-Recording  
**Status:** Erweiterte Beta mit Recording + Zoom/Pan-Funktionalität  

## 🛠️ Entwicklungsumgebung

### Build-System
- **Empfohlen:** `dotnet build` (moderne .NET CLI)
- **Vermeiden:** `msbuild` (legacy, XAML-Probleme bei modernen Projekten)

### Wichtige Kommandos
```bash
# Entwicklung
dotnet restore                    # Dependencies installieren
dotnet build                      # Debug-Build
dotnet build -c Release          # Release-Build  
dotnet run                        # App starten

# Deployment
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true

# Cleanup
dotnet clean                      # Build-Artifacts entfernen
```

### Projekt-Dependencies
```xml
<PackageReference Include="FlaUI.Core" Version="4.0.0" />
<PackageReference Include="FlaUI.UIA3" Version="4.0.0" />
<PackageReference Include="System.Drawing.Common" Version="8.0.8" />
```

## 🏗️ Architektur-Entscheidungen

### Element-Identifikation
**Problem:** Robust reproduzierbare Element-Erkennung für UI-Automation  
**Lösung:** Fallback-Hierarchie in `ElementIdentifier.cs`

```csharp
// Priorität (von beste zu schlechteste)
1. AutomationId           // Unveränderlich, beste Wahl
2. Name + ControlType     // Meist stabil
3. ClassName + Index      // Position-abhängig
4. HelpText/AccessKey     // Selten verfügbar
5. Parent-Path + Position // Letzter Ausweg
```

### Recording-System
**Design:** Singleton-Pattern für `ActionRecorder`  
**Vorteile:** Globaler Zugriff, Zustandsverwaltung  
**Threading:** UI-Thread (WPF-konform)

```csharp
// Verwendung
ActionRecorder.Instance.StartRecording("Session Name");
ActionRecorder.Instance.RecordAction("Click", element);
ActionRecorder.Instance.SaveCurrentRecording();
```

### Dual-Monitor-Handling
**Implementierung:** Win32 API über P/Invoke (`MonitorHelper.cs`)  
**Koordinatensystem:** Windows-global (nicht per-Monitor)

```csharp
// Monitor-Informationen abrufen
var monitorData = new MonitorData();
var lenseScreen = monitorData.LenseScreen;     // Monitor 1 
var remoteControl = monitorData.RemoteControl; // Monitor 2
```

## 🔧 Code-Organisation

### Haupt-Komponenten

1. **MainWindow** - Zentrale Steuerung, Recording-Controls
2. **ScreenshotTakeWindow** - Bereich-Auswahl auf Monitor 1
3. **ScreenshotShowWindow** - Remote-Control auf Monitor 2  
4. **ActionWindow** - Aktions-Auswahl Dialog
5. **ActionRecorder** - Recording-Engine (Singleton)

### Datenstrukturen
```csharp
// Action-Aufzeichnung
public class RecordedAction {
    public string ActionType { get; set; }
    public ElementIdentifier Element { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public DateTime Timestamp { get; set; }
}

// Element-Identifikation
public class ElementIdentifier {
    public string PrimaryId { get; set; }        // AutomationId
    public string FallbackSelector { get; set; } // XPath-ähnlich
    public string ParentPath { get; set; }       // Hierarchie-Info
}
```

## 🐛 Bekannte Probleme & Lösungen

### XAML-Kompilierung
**Problem:** MSBuild kompiliert XAML nicht korrekt → `InitializeComponent()` fehlt  
**Lösung:** Immer `dotnet build` verwenden, nie direktes MSBuild

### Nullable Reference Types
**Problem:** 28 Compiler-Warnungen (CS8618, CS8622, etc.)  
**Status:** Nicht kritisch, reine Code-Qualität  
**Lösung:** Optional - Felder als nullable markieren oder initialisieren

### UI-Thread-Zugriff
**Problem:** FlaUI-Operationen müssen im UI-Thread laufen  
**Implementierung:** Alle FlaUI-Calls sind bereits UI-Thread-konform

### Administrator-Rechte
**Problem:** FlaUI benötigt oft Admin-Rechte für manche Apps  
**Lösung:** App als Administrator starten bei Problemen

## 📦 Deployment-Strategien

### Single-File Executable (empfohlen)
```bash
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```
**Vorteile:** Eine Datei, keine Runtime-Installation nötig  
**Größe:** ~1.5MB  

### Framework-Dependent
```bash
dotnet publish -c Release
```
**Vorteile:** Kleinere Datei  
**Nachteile:** .NET Runtime muss installiert sein

### Self-Contained
```bash
dotnet publish -c Release -r win-x64 --self-contained true
```
**Vorteile:** Runtime inklusive  
**Nachteile:** ~100MB Größe

## 🧪 Testing-Strategien

### Manuelle Tests
1. **Dual-Monitor-Setup** - App startet auf Monitor 2
2. **Bereich-Auswahl** - Rahmen-Ziehen funktioniert
3. **Element-Erkennung** - Rechtsklick zeigt ActionWindow  
4. **Recording** - JSON/C#-Export funktioniert

### Test-Apps
- **Taschenrechner** (calc.exe) - Einfache Buttons
- **Notepad** - Texteingabe-Tests
- **Explorer** - Komplexere UI-Strukturen

### Debugging
```csharp
// Debug-Output aktiviert in allen Klassen
Debug.WriteLine("Status-Information");
```

## 📚 Entwicklungsstand (September 2025)

### ✅ Implementierte Features
- **Rahmen-Abbruch** - ESC/Rechtsklick bricht Rahmen-Auswahl ab
- **Verbessertes Beenden** - ESC + sauberer Fenster-Cleanup in allen Fenstern
- **Pan/Verschieben** - Screenshot mit Linker Maus + Ziehen verschieben  
- **Zoom-Grundlagen** - Mausrad vergrößert/verkleinert Screenshot
- **Debug-Logging** - Koordinaten-Tracking in `debug.log` für Entwicklung
- **Koordinaten-Korrektur** - Pan-Offset wird bei Element-Erkennung berücksichtigt

### 🔧 Zoom/Pan-System
```csharp
// Aktuelle Pan-Korrektur (funktioniert)
screenX = (int)(clickPoint.X - translateTransform.X);
screenY = (int)(clickPoint.Y - translateTransform.Y);

// TODO: Zoom-Korrektur (noch offen)
screenX = (int)((clickPoint.X - translateTransform.X) / scaleTransform.ScaleX);
screenY = (int)((clickPoint.Y - translateTransform.Y) / scaleTransform.ScaleY);
```

### ⏳ Nächste Schritte
- [ ] **Zoom-Koordinaten** - Korrekte Element-Erkennung nach Zoom/Scale
- [ ] Zoom-Grenzen und User-Experience optimieren

### 🎯 Geplante Features
- [ ] Live-Mirroring (statt Screenshot-Update)
- [ ] Mehr Action-Types (Drag&Drop, etc.)
- [ ] Recording-Playback direkt in App
- [ ] Timeline-View für aufgezeichnete Aktionen

### Code-Qualität Verbesserungen
- [ ] Nullable Reference Type Warnungen beheben
- [ ] Unit-Tests hinzufügen
- [ ] Async/Await für IO-Operationen
- [ ] Konfigurationsdatei für Settings

### Performance-Optimierungen
- [ ] Screenshot-Caching
- [ ] Element-Suche optimieren  
- [ ] Memory-Management verbessern

## 🔍 Debugging-Tipps

### Visual Studio
- Breakpoints in Event-Handlers setzen
- Watch-Window für Monitor-Koordinaten nutzen
- Debug-Output im Output-Window verfolgen

### Debug-Logging (NEU)
```bash
# Debug-Log wird automatisch erstellt beim App-Start
# Ort: \bin\Debug\net6.0-windows\debug.log
# Inhalt: Koordinaten-Transformationen, Pan/Zoom-Events
```

### FlaUI-Inspektor
- FlaUI.Inspect.exe für Element-Analyse nutzen
- UISpy (Windows SDK) als Alternative

---

**Entwicklung abgeschlossen:** September 2025  
**Nächste Schritte:** Testing in produktiver Umgebung, Feature-Requests sammeln