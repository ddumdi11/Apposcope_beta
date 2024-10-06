using System.Text;

namespace Apposcope_beta
{
    public class MonitorData
    {
        public MonitorInfo LenseScreen { get; set; } // Primärer Monitor
        public MonitorInfo RemoteControl { get; set; } // Sekundärer Monitor

        // Boolean, um festzustellen, ob der primäre Monitor als RemoteControl fungiert
        private bool primaryMonitorIsRemoteControl;

        public MonitorData()
        {
            LenseScreen = InitializeMonitorInfo(1); // Initialisiere den primären Monitor (LenseScreen)
            RemoteControl = InitializeMonitorInfo(2); // Initialisiere den sekundären Monitor (RemoteControl)

            // Setze die Monitorrollen
            LenseScreen.MonitorRole = "LenseScreen";
            RemoteControl.MonitorRole = "RemoteControl";
        }

        // Hilfsmethode zur Initialisierung der Monitorinformationen
        private MonitorInfo InitializeMonitorInfo(int monitorNumber)
        {
            return new MonitorInfo
            {
                MonitorNumber = monitorNumber,
                SystemLeft = MonitorHelper.GetSystemMonitorLeft(monitorNumber),
                SystemTop = MonitorHelper.GetSystemMonitorTop(monitorNumber),
                SystemRight = MonitorHelper.GetSystemMonitorRight(monitorNumber),
                SystemBottom = MonitorHelper.GetSystemMonitorBottom(monitorNumber),
                SystemWidth = MonitorHelper.GetSystemMonitorWidth(monitorNumber),
                SystemHeight = MonitorHelper.GetSystemMonitorHeight(monitorNumber)
            };
        }

        // Methode, um den aktuellen Monitor festzulegen (wird nur benötigt, falls eine Rolle wechselt)
        public void SetCurrentMonitor(bool isPrimaryMonitor)
        {
            primaryMonitorIsRemoteControl = isPrimaryMonitor;
        }

        // Gibt den aktuellen RemoteControl-Monitor zurück
        public MonitorInfo GetRemoteControlMonitor()
        {
            return primaryMonitorIsRemoteControl ? LenseScreen : RemoteControl;
        }

        // Gibt den aktuellen LenseScreen-Monitor zurück
        public MonitorInfo GetLenseScreenMonitor()
        {
            return !primaryMonitorIsRemoteControl ? LenseScreen : RemoteControl;
        }

        // Methode zur Debug-Ausgabe der Monitorinformationen
        public override string ToString()
        {
            StringBuilder monitorDataStringBuilder = new StringBuilder();

            monitorDataStringBuilder.AppendLine($"Monitor LenseScreen: {LenseScreen.SystemLeft}, {LenseScreen.SystemTop}, {LenseScreen.SystemWidth}, {LenseScreen.SystemHeight}");
            monitorDataStringBuilder.AppendLine($"Monitor RemoteControl: {RemoteControl.SystemLeft}, {RemoteControl.SystemTop}, {RemoteControl.SystemWidth}, {RemoteControl.SystemHeight}");

            return monitorDataStringBuilder.ToString();
        }
    }

    public class MonitorInfo
    {
        public int MonitorNumber { get; set; }
        public string MonitorRole { get; set; } // Monitorrolle (LenseScreen oder RemoteControl)
        public double SystemLeft { get; set; } // System-Left-Position (Win32 API)
        public double SystemTop { get; set; } // System-Top-Position (Win32 API)
        public double SystemRight { get; set; } // System-Right-Position (Win32 API)
        public double SystemBottom { get; set; } // System-Bottom-Position (Win32 API)
        public double SystemWidth { get; set; } // System-Breite (Win32 API)
        public double SystemHeight { get; set; } // System-Höhe (Win32 API)
    }
}
