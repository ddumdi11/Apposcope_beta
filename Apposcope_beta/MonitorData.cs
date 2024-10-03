using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apposcope_beta
{
    public class MonitorData
    {
        // Daten für den primären Monitor
        public MonitorInfo PrimaryMonitor { get; set; }

        // Daten für den sekundären Monitor
        public MonitorInfo SecondaryMonitor { get; set; }

        // Methode zum Aktualisieren der Monitor-Nummer
        public void UpdateMonitorNumber(int newMonitorNumber)
        {
            if (newMonitorNumber == 1)
            {
                // Aktualisiere Primär-Monitor
                this.PrimaryMonitor.MonitorNumber = newMonitorNumber;
            }
            else if (newMonitorNumber == 2)
            {
                // Aktualisiere Sekundär-Monitor
                this.SecondaryMonitor.MonitorNumber = newMonitorNumber;
            }
        }
    }

    public class MonitorInfo
    {
        public int MonitorNumber { get; set; } // Aktuelle Monitor-Nummer
        public double WpfLeft { get; set; } // WPF-Left-Position
        public double WpfTop { get; set; } // WPF-Top-Position
        public double WpfWidth { get; set; } // WPF-Breite
        public double WpfHeight { get; set; } // WPF-Höhe

        public double SystemLeft { get; set; } // System-Left-Position (Win32 API)
        public double SystemTop { get; set; } // System-Top-Position (Win32 API)
        public double SystemWidth { get; set; } // System-Breite (Win32 API)
        public double SystemHeight { get; set; } // System-Höhe (Win32 API)
    }


}
