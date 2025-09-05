# Apposcope Beta 🔬

Ein "digitales Mikroskop" für Windows-Desktop-Anwendungen mit Action-Recording und FlaUI-Automatisierung.

## Was ist Apposcope?

Apposcope verwandelt dein Dual-Monitor-Setup in ein leistungsfähiges Tool für UI-Automatisierung:
- **Monitor 1 (LenseScreen)**: Deine normale Arbeitsumgebung
- **Monitor 2 (RemoteControl)**: Spiegelt ausgewählte Bereiche und ermöglicht Remote-Steuerung

## 🚀 Features

### Dual-Monitor-System
- Automatische Erkennung von zwei Monitoren
- Bereich-Auswahl auf dem primären Monitor
- Live-Spiegelung auf dem sekundären Monitor
- Remote-Steuerung über gespiegelte Oberfläche

### Action Recording
- **Robuste Element-Identifikation**: AutomationId, Name, ClassName-basierte Selektoren
- **Minimal Mode Recording**: Nur die wesentlichen Daten für maximale Kompatibilität
- **JSON-Export**: Strukturierte Speicherung aller aufgezeichneten Aktionen
- **FlaUI Script Generator**: Automatische C#-Code-Generierung aus Recordings

### UI-Automatisierung
- **FlaUI-Integration**: Professionelle Windows UI-Automatisierung
- **Element-Erkennung**: Präzise Identifikation von Buttons, TextBoxes, ComboBoxes
- **Multi-Action Support**: Click, DoubleClick, SendKeys, ExpandDropdown
- **Screenshot-basierte Workflows**: Visuelle Kontrolle über automatisierte Abläufe

## 🛠️ Technische Details

**Framework:** .NET 6.0 Windows (WPF)  
**UI-Automation:** FlaUI 4.0  
**Plattform:** Windows 10/11 (x64)  
**Sprache:** C# 10

### Architektur

```
┌─────────────────┐    ┌──────────────────┐
│   LenseScreen   │    │  RemoteControl   │
│   (Monitor 1)   │────│   (Monitor 2)    │
│                 │    │                  │
│  ┌───────────┐  │    │ ┌──────────────┐ │
│  │   App     │  │    │ │  Screenshot  │ │
│  │ auswählen │  │────▶│  + Controls   │ │
│  └───────────┘  │    │ └──────────────┘ │
└─────────────────┘    └──────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────────────────────────────┐
│            FlaUI Engine                │
│     ┌─────────────────────────────┐     │
│     │     Action Recorder         │     │
│     │  ┌─────────────────────┐    │     │
│     │  │  Element Identifier │    │     │
│     │  └─────────────────────┘    │     │
│     └─────────────────────────────┘     │
└─────────────────────────────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────┐    ┌──────────────────┐
│  JSON Recording │    │  C# Script       │
│     Export      │    │   Generator      │
└─────────────────┘    └──────────────────┘
```

## 📦 Installation & Verwendung

### Voraussetzungen
- Windows 10/11
- .NET 6.0 Runtime
- Dual-Monitor-Setup

### Build von Quellcode
```bash
git clone <repository>
cd Apposcope_beta
dotnet restore
dotnet build -c Release
```

### Ausführbare Version erstellen
```bash
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

Die .exe findest du dann in: `bin/Release/net6.0-windows/win-x64/publish/`

## 🎯 Verwendung

### 1. App starten
- Doppelklick auf `Apposcope_beta.exe`
- App öffnet sich automatisch auf dem sekundären Monitor

### 2. Recording starten
1. **"Recording starten"** klicken (Button wird rot)
2. **"Rahmen ziehen"** → Bereich auf Monitor 1 auswählen
3. Ausgewählter Bereich wird auf Monitor 2 gespiegelt

### 3. Aktionen aufzeichnen
1. **Rechtsklick** auf UI-Elemente im gespiegelten Bereich
2. **ActionWindow** öffnet sich mit verfügbaren Aktionen:
   - Einfachklick
   - Doppelklick  
   - SendKeys (Texteingabe)
   - Dropdown öffnen
3. **Aktion auswählen** → wird ausgeführt und aufgezeichnet

### 4. Export
- **"Recording speichern"**: JSON-Datei mit allen Aktionen
- **"C# Script generieren"**: Fertiger FlaUI-Code für Automatisierung

## 📁 Projektstruktur

```
Apposcope_beta/
├── ActionRecorder.cs          # Zentrale Recording-Verwaltung
├── ActionWindow.xaml(.cs)     # UI für Aktion-Auswahl
├── App.xaml(.cs)             # WPF-Application Entry Point
├── ElementIdentifier.cs       # Robuste Element-Identifikation
├── FlaUIElementChecker.cs     # Element-Erkennung und Highlighting
├── FlaUIScriptGenerator.cs    # C#-Code-Generator
├── MainWindow.xaml(.cs)       # Hauptfenster mit Recording-Controls
├── MonitorData.cs            # Dual-Monitor-System
├── MonitorHelper.cs          # Win32 Monitor-API Wrapper
├── RecordedAction.cs         # Action-Datenstrukturen
├── ScreenshotTakeWindow.xaml(.cs)  # Bereich-Auswahl Interface
└── ScreenshotWindowShow.xaml(.cs)  # Remote-Control Interface
```

## 🔧 Erweiterte Konfiguration

### Element-Identifikation (Priorität)
1. **AutomationId** (beste Wahl - unveränderlich)
2. **Name + ControlType**
3. **ClassName + Index**
4. **HelpText/AccessKey + ControlType**  
5. **Parent-Path + Position** (letzter Ausweg)

### Recording-Modi
- **Minimal Mode** (Standard): Nur essentielle Daten für maximale Kompatibilität
- Erweiterbar um Debug/Analysis-Modi bei Bedarf

## 🐛 Troubleshooting

### Häufige Probleme

**App startet nicht:**
- .NET 6.0 Runtime installiert?
- Als Administrator ausführen (für UI-Automation)

**Kein zweiter Monitor erkannt:**
- Windows-Anzeigeeinstellungen prüfen
- Monitor muss als "erweitert" konfiguriert sein (nicht gespiegelt)

**Element-Erkennung funktioniert nicht:**
- FlaUI benötigt oft Administrator-Rechte
- Manche Apps (besonders ältere) unterstützen UI Automation nicht vollständig

**Recording-Dateien werden nicht erstellt:**
- Schreibrechte im Desktop-Ordner prüfen
- Antivirus-Software kann Dateierstellung blockieren

## 🤝 Entwicklung

Das Projekt ist in C# mit WPF entwickelt und nutzt moderne .NET 6-Features. 
Beiträge und Verbesserungen sind willkommen!

### Entwickler-Setup
```bash
# Repository klonen
git clone <repository>
cd Apposcope_beta

# Dependencies installieren
dotnet restore

# Entwickeln mit hot reload
dotnet run --project Apposcope_beta

# Tests (wenn vorhanden)
dotnet test
```

---
**Letztes Update:** September 2025  
**Version:** Beta (Recording-System implementiert)