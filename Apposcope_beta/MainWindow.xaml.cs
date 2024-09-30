using System.Windows;
using System.Diagnostics;

namespace Apposcope_beta
{
    // Datenstruktur zur Erfassung der Monitorinformationen
    public struct MonitorInfo
    {
        public int MonitorNumber;
        public double Left;
        public double Top;
        public double Width;
        public double Height;

        public MonitorInfo(int monitorNumber, double left, double top, double width, double height)
        {
            MonitorNumber = monitorNumber;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
    }

    public partial class MainWindow : Window
    {
        private MonitorInfo primaryMonitor;
        private MonitorInfo secondaryMonitor;

        public MainWindow()
        {
            InitializeComponent();

            GetMonitorData();

            // Monitorinformationen erfassen (einschließlich Höhe und Top-Position)
            primaryMonitor = new MonitorInfo(1, 0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            secondaryMonitor = new MonitorInfo(2, SystemParameters.PrimaryScreenWidth, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth - SystemParameters.PrimaryScreenWidth, SystemParameters.VirtualScreenHeight);

            // Debug-Ausgabe zur Überprüfung der Monitorinformationen
            Debug.WriteLine($"Primärer Monitor: Monitor = {primaryMonitor.MonitorNumber} Left = {primaryMonitor.Left}, Top = {primaryMonitor.Top}, Width = {primaryMonitor.Width}, Height = {primaryMonitor.Height}");
            Debug.WriteLine($"Sekundärer Monitor: Monitor = {secondaryMonitor.MonitorNumber} Left = {secondaryMonitor.Left}, Top = {secondaryMonitor.Top}, Width = {secondaryMonitor.Width}, Height = {secondaryMonitor.Height}");


        }

        private void DrawFrame_Click(object sender, RoutedEventArgs e)
        {
            // Fensterposition anzeigen, bevor der Rahmen gezogen wird
            Debug.WriteLine($"Startfenster beim Klick auf 'Rahmen ziehen': Monitor = {this.primaryMonitor.MonitorNumber} Left = {this.Left}, Top = {this.Top}");

            // Hauptfenster ausblenden
            this.Hide();

            // Prüfen, ob das Startfenster auf dem primären oder sekundären Monitor ist
            MonitorInfo targetMonitorSsTake; // TargetMonitor für Screenshot nehmen
            MonitorInfo targetMonitorSsShow; // TargetMonitor für Screenshot zeigen

            if (this.Left >= primaryMonitor.Left && this.Left < primaryMonitor.Left + primaryMonitor.Width)
            {
                Debug.WriteLine("Hauptfenster ist auf dem primären Monitor");
                targetMonitorSsTake = secondaryMonitor;
                targetMonitorSsShow = primaryMonitor;
            }
            else
            {
                Debug.WriteLine("Hauptfenster ist auf dem sekundären Monitor");
                targetMonitorSsTake = primaryMonitor;
                targetMonitorSsShow = secondaryMonitor;
            }

            // Monitorfenster für den Rahmen des Screenshots
            Debug.WriteLine($"Fenster für Screenshot aufnehmen: Monitor = {targetMonitorSsTake.MonitorNumber} Left = {targetMonitorSsTake.Left}, Top = {targetMonitorSsTake.Top}, Width = {targetMonitorSsTake.Width}, Height = {targetMonitorSsTake.Height}");

            // Monitor für das Anzeigen des Screenshots (zugleich auch Monitor, auf dem das Hauptfenster jetzt ist)
            Debug.WriteLine($"Fenster für Screenshot anzeigen: Monitor = {targetMonitorSsShow.MonitorNumber} Left = {targetMonitorSsShow.Left}, Top = {targetMonitorSsShow.Top}, Width = {targetMonitorSsShow.Width}, Height = {targetMonitorSsShow.Height}");

            // Neues Fenster für den zweiten Monitor erstellen und die Monitorinformationen übergeben
            ScreenshotTakeWindow screenshotTakeWindow = new ScreenshotTakeWindow(targetMonitorSsTake, targetMonitorSsShow);

            // Berechne den Höhenversatz zwischen den Monitoren
            double heightOffset = primaryMonitor.Top - secondaryMonitor.Top;

            Debug.WriteLine("Der berechnete heightOffset mit primaryMonitor.Top - secondaryMonitor.Top ist: " + heightOffset);

            heightOffset = targetMonitorSsShow.Top - targetMonitorSsTake.Top;

            Debug.WriteLine("Der berechnete heightOffset mit targetMonitorSsShow.Top - targetMonitorSsTake.Top ist: " + heightOffset);

            // Berechne den Y-Versatz zwischen den Monitoren
            heightOffset = targetMonitorSsTake.Top - targetMonitorSsShow.Top;

            // Prüfe den Versatz im Debugging
            Debug.WriteLine($"Der berechnete heightOffset mit targetMonitorSsTake.Top - targetMonitorSsShow.Top ist: {heightOffset}");


            // Setze die Position des Fensters auf die berechnete Startposition unter Berücksichtigung des Versatzes
            screenshotTakeWindow.Top = 0;
            screenshotTakeWindow.Left = primaryMonitor.Left;
            screenshotTakeWindow.Height = targetMonitorSsTake.Height;
            screenshotTakeWindow.Width = targetMonitorSsTake.Width;

            // Fenster anzeigen (ohne Maximierung)
            screenshotTakeWindow.Show();
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GetMonitorData()
        {
            var monitors = MonitorHelper.GetMonitors();
            if (monitors.Count == 0)
            {
                Debug.WriteLine("Keine Monitore gefunden.");
            }
            else
            {
                foreach (var monitor in monitors)
                {
                    Debug.WriteLine($"Monitor: {monitor.szDevice}, Left: {monitor.rcMonitor.Left}, Top: {monitor.rcMonitor.Top}, Width: {monitor.rcMonitor.Right - monitor.rcMonitor.Left}, Height: {monitor.rcMonitor.Bottom - monitor.rcMonitor.Top}");
                }
            }
        }


    }
}
