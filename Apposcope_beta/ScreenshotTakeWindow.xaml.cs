using System.Diagnostics;
using System.Drawing; // Für Graphics und Bitmap (du musst System.Drawing.Common per NuGet hinzufügen)
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfPoint = System.Windows.Point;
using WpfRectangle = System.Windows.Shapes.Rectangle;


namespace Apposcope_beta
{
    public partial class ScreenshotTakeWindow : Window
    {
        private WpfPoint startPoint;
        private WpfRectangle selectionRectangle;
        private MonitorInfo currentMonitor; // Die aktuellen Monitorinformationen
        private MonitorInfo targetMonitorShow; // Der Monitor, auf dem das ScreenshotWindow angezeigt werden soll

        public ScreenshotTakeWindow(MonitorInfo takeScreenshotMonitor, MonitorInfo showScreenshotMonitor)
        {
            InitializeComponent();
            currentMonitor = takeScreenshotMonitor; // Monitorinformationen speichern
            targetMonitorShow = showScreenshotMonitor;

            // Positionsangaben nach Wechsel zu TakeScreenshotMonitor
            // Monitorfenster für den Rahmen des Screenshots
            Debug.WriteLine("Jetzt ist das Fenster für die Screenshot-Aufnahme geöffnet!");
            Debug.WriteLine($"Fenster für Screenshot aufnehmen: Monitor = {currentMonitor.MonitorNumber} Left = {currentMonitor.Left}, Top = {currentMonitor.Top}, Width = {currentMonitor.Width}, Height = {currentMonitor.Height}");

            // Monitor für das Anzeigen des Screenshots (zugleich auch Monitor, auf dem das Hauptfenster jetzt ist)
            Debug.WriteLine($"Fenster für Screenshot anzeigen: Monitor = {targetMonitorShow.MonitorNumber} Left = {targetMonitorShow.Left}, Top = {targetMonitorShow.Top}, Width = {targetMonitorShow.Width}, Height = {targetMonitorShow.Height}");

        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(this);

            // Rechteck für den Rahmen erstellen
            selectionRectangle = new WpfRectangle
            {
                Stroke = System.Windows.Media.Brushes.Red,
                StrokeThickness = 2
            };

            Canvas.SetLeft(selectionRectangle, startPoint.X);
            Canvas.SetTop(selectionRectangle, startPoint.Y);
            secondCanvas.Children.Add(selectionRectangle);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (selectionRectangle == null) return;

            var pos = e.GetPosition(this);
            var width = Math.Abs(pos.X - startPoint.X);
            var height = Math.Abs(pos.Y - startPoint.Y);

            selectionRectangle.Width = width;
            selectionRectangle.Height = height;

            Canvas.SetLeft(selectionRectangle, Math.Min(pos.X, startPoint.X));
            Canvas.SetTop(selectionRectangle, Math.Min(pos.Y, startPoint.Y));
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selectionRectangle != null)
            {
                // Erfassung des markierten Bereichs und Screenshot aufnehmen
                var selectionTopLeft = selectionRectangle.TransformToAncestor(this).Transform(new WpfPoint(0, 0));

                // Verwende die Monitorinformationen, um den richtigen Bereich zu erfassen
                var screenshotPath = TakeScreenshot(
                    (int)(currentMonitor.Left + selectionTopLeft.X),
                    (int)(currentMonitor.Top + selectionTopLeft.Y),
                    (int)selectionRectangle.Width,
                    (int)selectionRectangle.Height);

                // Screenshot anzeigen (in einem neuen Fenster auf dem ursprünglichen Monitor)
                ShowScreenshot(screenshotPath);

                secondCanvas.Children.Remove(selectionRectangle);
                selectionRectangle = null;
            }

            this.Close(); // Zweites Fenster schließen, wenn der Bereich festgelegt wurde
        }

        private string TakeScreenshot(int x, int y, int width, int height)
        {
            string screenshotPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "screenshot.png");

            using (Bitmap bitmap = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // Verwende die berechneten Koordinaten basierend auf dem Monitor
                    g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));
                }

                // Screenshot speichern
                bitmap.Save(screenshotPath, System.Drawing.Imaging.ImageFormat.Png);
            }

            return screenshotPath;
        }

        private void ShowScreenshot(string screenshotPath)
        {
            // Lade den Screenshot und erhalte die Breite und Höhe des Bildes
            var bitmap = new BitmapImage(new Uri(screenshotPath));

            var screenshotShowWindow = new ScreenshotShowWindow(screenshotPath, currentMonitor);

            // Setze die Größe des Fensters auf die Größe des Screenshots
            screenshotShowWindow.Width = bitmap.PixelWidth;
            screenshotShowWindow.Height = bitmap.PixelHeight;

            // Berechne die Mitte des Zielmonitors, basierend auf der Monitorgröße und dem Fenster
            double centerX = targetMonitorShow.Left + (targetMonitorShow.Width - screenshotShowWindow.Width) / 2;
            double centerY = targetMonitorShow.Top + (targetMonitorShow.Height - screenshotShowWindow.Height) / 2;

            // Setze die Position des Fensters auf die berechnete Mitte
            screenshotShowWindow.Left = centerX;
            screenshotShowWindow.Top = centerY;

            screenshotShowWindow.Show();
        }

    }
}
