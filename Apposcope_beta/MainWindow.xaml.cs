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

        /// <summary>
        /// Hides the main window and opens a ScreenshotTakeWindow on the secondary monitor using the stored monitor data.
        /// </summary>
        /// <param name="sender">The event source (unused).</param>
        /// <param name="e">Event data (unused).</param>
        private void DrawFrame_Click(object sender, RoutedEventArgs e)
        {
            // Hauptfenster ausblenden
            Hide();

            // Neues Fenster für den zweiten Monitor erstellen und die Monitorinformationen übergeben
            var screenshotTakeWindow = new ScreenshotTakeWindow(monitorData);

            // Fenster anzeigen
            screenshotTakeWindow.Show();
        }

        /// <summary>
        /// Toggles the recording state: starts a new recording if none is running, or stops the current recording.
        /// </summary>
        /// <remarks>
        /// When starting or stopping, the method updates the RecordingButton's text and background to reflect the new state.
        /// Uses ActionRecorder.Instance to control recording.
        /// </remarks>
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

        /// <summary>
        /// Saves the current recording if one exists and notifies the user of the result.
        /// </summary>
        /// <remarks>
        /// If ActionRecorder.Instance.CurrentRecording is non-null this calls SaveCurrentRecording() and displays a success message.
        /// Otherwise a warning message is shown indicating no recording is available.
        /// </remarks>
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

        /// <summary>
        /// Generates a C# automation script from the current recording and saves it to the user's Desktop with a timestamped filename.
        /// </summary>
        /// <remarks>
        /// If a recording is available (ActionRecorder.Instance.CurrentRecording), the script is written to
        /// "AutomationScript_yyyyMMdd_HHmmss.cs" on the current user's Desktop using FlaUIScriptGenerator.SaveScriptToFile,
        /// and an informational message box is shown with the file path. If no recording is available, a warning message box is shown instead.
        /// </remarks>
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

        /// <summary>
        /// Handles the Cancel button click and closes the main window.
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
