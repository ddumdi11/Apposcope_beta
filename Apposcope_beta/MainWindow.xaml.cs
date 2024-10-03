using System.Windows;
using System.Diagnostics;

namespace Apposcope_beta
{
    // Datenstruktur zur Erfassung der Monitorinformationen
    public struct MonitorInfoOld
    {
        public int MonitorNumber;
        public double Left;
        public double Top;
        public double Width;
        public double Height;

        public MonitorInfoOld(int monitorNumber, double left, double top, double width, double height)
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
        private MonitorInfoOld primaryMonitor;
        private MonitorInfoOld secondaryMonitor;
        private double topOffset;
        private MonitorData startMonitorData;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMonitorData();

            GetMonitorData();


            // Monitorinformationen erfassen (einschließlich Höhe und Top-Position)
            primaryMonitor = new MonitorInfoOld(1, 0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            secondaryMonitor = new MonitorInfoOld(2, SystemParameters.PrimaryScreenWidth, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth - SystemParameters.PrimaryScreenWidth, SystemParameters.VirtualScreenHeight);

            // Debug-Ausgabe zur Überprüfung der Monitorinformationen
            Debug.WriteLine($"Primärer Monitor: Monitor = {primaryMonitor.MonitorNumber} Left = {primaryMonitor.Left}, Top = {primaryMonitor.Top}, Width = {primaryMonitor.Width}, Height = {primaryMonitor.Height}");
            Debug.WriteLine($"Sekundärer Monitor: Monitor = {secondaryMonitor.MonitorNumber} Left = {secondaryMonitor.Left}, Top = {secondaryMonitor.Top}, Width = {secondaryMonitor.Width}, Height = {secondaryMonitor.Height}");

        }

        private bool IsPrimaryMonitor() => this.Left >= primaryMonitor.Left && this.Left < primaryMonitor.Left + primaryMonitor.Width;

        private void DrawFrame_Click(object sender, RoutedEventArgs e)
        {

            // Fensterposition anzeigen, bevor der Rahmen gezogen wird
            Debug.WriteLine($"Startfenster beim Klick auf 'Rahmen ziehen': Monitor = {startMonitorData.PrimaryMonitor.MonitorNumber} Left = {this.Left}, Top = {this.Top}");

            // Hauptfenster ausblenden
            Hide();

            // Prüfen, ob das Startfenster auf dem primären oder sekundären Monitor ist
            MonitorInfo targetMonitorSsTake; // TargetMonitor für Screenshot nehmen
            MonitorInfo targetMonitorSsShow; // TargetMonitor für Screenshot zeigen

            double topAddVal = 0;

            if (IsPrimaryMonitor())
            {
                Debug.WriteLine("Hauptfenster ist auf dem primären Monitor");
                targetMonitorSsTake = startMonitorData.SecondaryMonitor;
                targetMonitorSsShow = startMonitorData.PrimaryMonitor;
                topAddVal = 0;
            }
            else
            {
                Debug.WriteLine("Hauptfenster ist auf dem sekundären Monitor");
                targetMonitorSsTake = startMonitorData.PrimaryMonitor;
                targetMonitorSsShow = startMonitorData.SecondaryMonitor;
                topAddVal = topOffset;
                targetMonitorSsShow.WpfTop += topAddVal;
            }

            // Aktualisiere die Monitor-Nummer basierend auf der aktuellen Position des Fensters
            UpdateMonitorNumberIfMoved(startMonitorData);

            // Kontrolle
            Debug.WriteLine($"WPF Left: {targetMonitorSsTake.WpfLeft}, WPF Top: {targetMonitorSsTake.WpfTop}, WPF Width: {targetMonitorSsTake.WpfWidth}, WPF Height: {targetMonitorSsTake.WpfHeight}");


            // Monitorfenster für den Rahmen des Screenshots
            Debug.WriteLine($"Fenster für Screenshot aufnehmen: Monitor = {targetMonitorSsTake.MonitorNumber} Left = {targetMonitorSsTake.WpfLeft}, Top = {targetMonitorSsTake.WpfTop}, Width = {targetMonitorSsTake.WpfWidth}, Height = {targetMonitorSsTake.WpfHeight}");

            // Monitor für das Anzeigen des Screenshots (zugleich auch Monitor, auf dem das Hauptfenster jetzt ist)
            Debug.WriteLine($"Fenster für Screenshot anzeigen: Monitor = {targetMonitorSsShow.MonitorNumber} Left = {targetMonitorSsShow.WpfLeft}, Top = {targetMonitorSsShow.WpfTop}, Width = {targetMonitorSsShow.WpfWidth}, Height = {targetMonitorSsShow.WpfHeight}");

            // Neues Fenster für den zweiten Monitor erstellen und die Monitorinformationen übergeben
            var screenshotTakeWindow = new ScreenshotTakeWindow(monitorInfoConverter(targetMonitorSsTake), monitorInfoConverter(targetMonitorSsShow), topAddVal);

            // Berechne den Offset und setze die Position des Fensters
            Debug.WriteLine("Der berechnete topOffset mit Monitor-1-Top - Monitor-2-top ist: " + topAddVal);
            screenshotTakeWindow.Top = targetMonitorSsTake.WpfTop;
            screenshotTakeWindow.Left = targetMonitorSsTake.WpfLeft;
            screenshotTakeWindow.Height = targetMonitorSsTake.WpfHeight;
            screenshotTakeWindow.Width = targetMonitorSsTake.WpfWidth;


            // Fenster anzeigen
            screenshotTakeWindow.Show();
        }

        private MonitorInfoOld monitorInfoConverter(MonitorInfo monitorInfo)
        {

            MonitorInfoOld monitorInfoOld = new MonitorInfoOld();
            monitorInfoOld.MonitorNumber = monitorInfo.MonitorNumber;
            monitorInfoOld.Left = monitorInfo.WpfLeft;  // Nutze WPF-Koordinaten
            monitorInfoOld.Top = monitorInfo.WpfTop;    // Nutze WPF-Koordinaten
            monitorInfoOld.Height = monitorInfo.WpfHeight;  // Nutze WPF-Größe
            monitorInfoOld.Width = monitorInfo.WpfWidth;    // Nutze WPF-Größe            


            return monitorInfoOld;
        }


        public void InitializeMonitorData()
        {
            startMonitorData = new MonitorData
            {
                PrimaryMonitor = new MonitorInfo
                {
                    MonitorNumber = 1,
                    WpfLeft = MonitorHelper.GetSystemMonitorLeft(1),  // Nutze Monitordaten
                    WpfTop = MonitorHelper.GetSystemMonitorTop(1),
                    WpfWidth = MonitorHelper.GetSystemMonitorWidth(1),
                    WpfHeight = MonitorHelper.GetSystemMonitorHeight(1),
                    SystemLeft = MonitorHelper.GetSystemMonitorLeft(1),
                    SystemTop = MonitorHelper.GetSystemMonitorTop(1),
                    SystemWidth = MonitorHelper.GetSystemMonitorWidth(1),
                    SystemHeight = MonitorHelper.GetSystemMonitorHeight(1)
                },
                SecondaryMonitor = new MonitorInfo
                {
                    MonitorNumber = 2,
                    WpfLeft = MonitorHelper.GetSystemMonitorLeft(2),
                    WpfTop = MonitorHelper.GetSystemMonitorTop(2),
                    WpfWidth = MonitorHelper.GetSystemMonitorWidth(2),
                    WpfHeight = MonitorHelper.GetSystemMonitorHeight(2),
                    SystemLeft = MonitorHelper.GetSystemMonitorLeft(2),
                    SystemTop = MonitorHelper.GetSystemMonitorTop(2),
                    SystemWidth = MonitorHelper.GetSystemMonitorWidth(2),
                    SystemHeight = MonitorHelper.GetSystemMonitorHeight(2)
                }
            };


            // Aktualisiere die Monitor-Nummer basierend auf der aktuellen Position des Fensters
            UpdateMonitorNumberIfMoved(startMonitorData);

            // Hier könntest du die Struktur dann an das nächste Fenster übergeben, wenn nötig
            // var screenshotTakeWindow = new ScreenshotTakeWindow(startMonitorData);
            // screenshotTakeWindow.Show();
        }

        private void UpdateMonitorNumberIfMoved(MonitorData monitorData)
        {
            // Überprüfe, ob das Fenster auf den sekundären Monitor verschoben wurde
            if (this.Left >= monitorData.SecondaryMonitor.WpfLeft)
            {
                monitorData.UpdateMonitorNumber(2); // Monitor-Nummer aktualisieren
            }
            else
            {
                monitorData.UpdateMonitorNumber(1); // Andernfalls auf dem primären Monitor
            }
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
                    Debug.WriteLine($"Monitor:, Left: {monitor.rcMonitor.Left}, Top: {monitor.rcMonitor.Top}, Width: {monitor.rcMonitor.Right - monitor.rcMonitor.Left}, Height: {monitor.rcMonitor.Bottom - monitor.rcMonitor.Top}");
                }
            }

            topOffset = monitors[0].rcMonitor.Top - monitors[1].rcMonitor.Top;
        }

    }
}
