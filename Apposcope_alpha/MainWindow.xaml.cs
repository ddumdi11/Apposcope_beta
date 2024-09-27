using System.Windows;
using System.Diagnostics;

namespace Apposcope_alpha
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

            // Monitorinformationen erfassen (einschließlich Höhe und Top-Position)
            primaryMonitor = new MonitorInfo(1, 0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            secondaryMonitor = new MonitorInfo(2, SystemParameters.PrimaryScreenWidth, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth - SystemParameters.PrimaryScreenWidth, SystemParameters.VirtualScreenHeight);

            // Debug-Ausgabe zur Überprüfung der Monitorinformationen
            Debug.WriteLine($"Primärer Monitor: Left = {primaryMonitor.Left}, Top = {primaryMonitor.Top}, Width = {primaryMonitor.Width}, Height = {primaryMonitor.Height}");
            Debug.WriteLine($"Sekundärer Monitor: Left = {secondaryMonitor.Left}, Top = {secondaryMonitor.Top}, Width = {secondaryMonitor.Width}, Height = {secondaryMonitor.Height}");
        }

        private void DrawFrame_Click(object sender, RoutedEventArgs e)
        {
            // Fensterposition anzeigen, bevor der Rahmen gezogen wird
            Debug.WriteLine($"Startfenster beim Klick auf 'Rahmen ziehen': Left = {this.Left}, Top = {this.Top}");

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

            // Neues Fenster für den zweiten Monitor erstellen und die Monitorinformationen übergeben
            SecondMonitorWindow secondMonitorWindow = new SecondMonitorWindow(targetMonitorSsTake, targetMonitorSsShow);

            // Setze die Position und Größe des zweiten Fensters basierend auf dem Zielmonitor
            secondMonitorWindow.Left = targetMonitorSsTake.Left;
            secondMonitorWindow.Top = targetMonitorSsTake.Top;
            secondMonitorWindow.Width = targetMonitorSsTake.Width;
            secondMonitorWindow.Height = targetMonitorSsTake.Height;

            Debug.WriteLine($"Zweites Fenster: Left = {secondMonitorWindow.Left}, Top = {secondMonitorWindow.Top}, Width = {secondMonitorWindow.Width}, Height = {secondMonitorWindow.Height}");

            // Fenster anzeigen (ohne Maximierung)
            secondMonitorWindow.Show();
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
