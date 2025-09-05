using System.Windows;
using System.Diagnostics;
using System.IO;

namespace Apposcope_beta
{
    public partial class MainWindow : Window
    {
        private MonitorData monitorData;

        public MainWindow()
        {
            InitializeComponent();

            // Monitor-Daten ermitteln
            monitorData = new MonitorData();

            // Setze das Startfenster automatisch auf den sekundären Monitor (RemoteControl)
            SetWindowToSecondaryMonitor();

            // Kontrolle der Monitor-Daten
            Debug.WriteLine(monitorData.ToString());

            // Hinzufügen des Ereignisses, um das Verschieben auf den primären Monitor zu verhindern
            this.LocationChanged += MainWindow_LocationChanged;
        }

        // Methode, um das Fenster automatisch auf den sekundären Monitor zu setzen
        private void SetWindowToSecondaryMonitor()
        {
            this.Left = monitorData.RemoteControl.SystemLeft;
            this.Top = monitorData.RemoteControl.SystemTop;
            this.Width = monitorData.RemoteControl.SystemWidth;
            this.Height = monitorData.RemoteControl.SystemHeight;
        }

        // Verhindern, dass das Fenster auf den primären Monitor verschoben wird
        private void MainWindow_LocationChanged(object sender, System.EventArgs e)
        {
            if (this.Left < monitorData.RemoteControl.SystemLeft || this.Left >= monitorData.RemoteControl.SystemLeft + monitorData.RemoteControl.SystemWidth)
            {
                // Wenn das Fenster auf den primären Monitor verschoben wird, setzen wir es zurück
                Debug.WriteLine("Das Fenster darf nicht auf den primären Monitor verschoben werden.");
                SetWindowToSecondaryMonitor();
            }
        }

        private void DrawFrame_Click(object sender, RoutedEventArgs e)
        {
            // Hauptfenster ausblenden
            Hide();

            // Neues Fenster für den zweiten Monitor erstellen und die Monitorinformationen übergeben
            var screenshotTakeWindow = new ScreenshotTakeWindow(monitorData);

            // Fenster anzeigen
            screenshotTakeWindow.Show();
        }

        private void ToggleRecording_Click(object sender, RoutedEventArgs e)
        {
            if (ActionRecorder.Instance.IsRecording)
            {
                ActionRecorder.Instance.StopRecording();
                RecordingButton.Content = "Recording starten";
                RecordingButton.Background = System.Windows.Media.Brushes.LightGray;
            }
            else
            {
                ActionRecorder.Instance.StartRecording("Apposcope Recording");
                RecordingButton.Content = "Recording stoppen";
                RecordingButton.Background = System.Windows.Media.Brushes.Red;
            }
        }

        private void SaveRecording_Click(object sender, RoutedEventArgs e)
        {
            if (ActionRecorder.Instance.CurrentRecording != null)
            {
                ActionRecorder.Instance.SaveCurrentRecording();
                MessageBox.Show("Recording wurde gespeichert!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Kein Recording verfügbar.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GenerateScript_Click(object sender, RoutedEventArgs e)
        {
            if (ActionRecorder.Instance.CurrentRecording != null)
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var scriptPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"AutomationScript_{timestamp}.cs");
                
                FlaUIScriptGenerator.SaveScriptToFile(ActionRecorder.Instance.CurrentRecording, scriptPath);
                MessageBox.Show($"C# Script wurde generiert:\n{scriptPath}", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Kein Recording verfügbar.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
