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
        private MonitorInfoOld takeScreenshotMonitor; // Die aktuellen Monitorinformationen
        private MonitorInfoOld showScreenshotMonitor; // Der Monitor, auf dem das ScreenshotWindow angezeigt werden soll
        private int screenshotLeft;
        private int screenshotTop;
        private double topOffset;

        public ScreenshotTakeWindow(MonitorInfoOld takeScreenshotMonitor, MonitorInfoOld showScreenshotMonitor, double topOffset)
        {
            InitializeComponent();
            this.takeScreenshotMonitor = takeScreenshotMonitor; // Monitorinformationen speichern
            this.showScreenshotMonitor = showScreenshotMonitor;
            this.topOffset = topOffset;

            // Positionsangaben nach Wechsel zu TakeScreenshotMonitor
            // Monitorfenster für den Rahmen des Screenshots
            Debug.WriteLine("Jetzt ist das Fenster für die Screenshot-Aufnahme geöffnet!");
            Debug.WriteLine($"Fenster für Screenshot aufnehmen: Monitor = {this.takeScreenshotMonitor.MonitorNumber} Left = {this.takeScreenshotMonitor.Left}, Top = {this.takeScreenshotMonitor.Top}, Width = {this.takeScreenshotMonitor.Width}, Height = {this.takeScreenshotMonitor.Height}");

            // Monitor für das Anzeigen des Screenshots (zugleich auch Monitor, auf dem das Hauptfenster jetzt ist)
            Debug.WriteLine($"Fenster für Screenshot anzeigen: Monitor = {this.showScreenshotMonitor.MonitorNumber} Left = {this.showScreenshotMonitor.Left}, Top = {this.showScreenshotMonitor.Top}, Width = {this.showScreenshotMonitor.Width}, Height = {this.showScreenshotMonitor.Height}");

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
                    (int)(takeScreenshotMonitor.Left + selectionTopLeft.X),
                    (int)(takeScreenshotMonitor.Top + selectionTopLeft.Y),
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
            try
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
                    // Koordinaten speichern
                    screenshotLeft = x;
                    screenshotTop = y;
                    Debug.WriteLine("Screenshot Abstand von links: " + screenshotLeft);
                    Debug.WriteLine("Screenshot Abstand von oben: " + screenshotTop);
                }

                return screenshotPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Erstellen des Screenshots: {ex.Message}");
                return string.Empty;
            }
        }

        private void ShowScreenshot(string screenshotPath)
        {
            // Lade den Screenshot und erhalte die Breite und Höhe des Bildes
            var bitmap = new BitmapImage(new Uri(screenshotPath));

            // Verwende den Zielmonitor, um das Fenster dort zu öffnen (statt auf dem aktuellen Monitor)
            var screenshotShowWindow = new ScreenshotShowWindow(screenshotPath, showScreenshotMonitor, screenshotLeft, screenshotTop, takeScreenshotMonitor); // Zielmonitor ist gegenüberliegend

            screenshotShowWindow.WindowStyle = WindowStyle.None; // Kein Rahmen
            screenshotShowWindow.WindowState = WindowState.Normal; // Kein Maximieren, damit wir die Position setzen können
            screenshotShowWindow.Topmost = true; // Immer im Vordergrund

            // Setze die Position des Fensters auf den Zielmonitor (targetMonitorShow)
            screenshotShowWindow.Left = showScreenshotMonitor.Left; // X-Koordinate des Zielmonitors
            screenshotShowWindow.Top = showScreenshotMonitor.Top - topOffset;   // Y-Koordinate des Zielmonitors

            // Setze die Größe des Fensters entsprechend dem Zielmonitor
            screenshotShowWindow.Width = showScreenshotMonitor.Width;
            screenshotShowWindow.Height = showScreenshotMonitor.Height;

            // Zeige das Fenster
            screenshotShowWindow.Show();
        }



    }
}
