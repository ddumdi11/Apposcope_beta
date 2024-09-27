using System.Drawing; // Für Graphics und Bitmap (du musst System.Drawing.Common per NuGet hinzufügen)
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfPoint = System.Windows.Point;
using WpfRectangle = System.Windows.Shapes.Rectangle;


namespace Apposcope_alpha
{
    public partial class SecondMonitorWindow : Window
    {
        private WpfPoint startPoint;
        private WpfRectangle selectionRectangle;
        private MonitorInfo currentMonitor; // Die aktuellen Monitorinformationen
        private MonitorInfo targetMonitorShow; // Der Monitor, auf dem das ScreenshotWindow angezeigt werden soll

        public SecondMonitorWindow(MonitorInfo monitorInfo, MonitorInfo targetScreenshotWindowMonitor)
        {
            InitializeComponent();
            currentMonitor = monitorInfo; // Monitorinformationen speichern
            targetMonitorShow = targetScreenshotWindowMonitor;
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

            var screenshotWindow = new ScreenshotWindow(screenshotPath);

            // Setze die Größe des Fensters auf die Größe des Screenshots
            screenshotWindow.Width = bitmap.PixelWidth;
            screenshotWindow.Height = bitmap.PixelHeight;

            // Positioniere das Fenster auf dem Monitor des Startfensters
            //screenshotWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            screenshotWindow.Top = targetMonitorShow.Top + 50;
            screenshotWindow.Left = targetMonitorShow.Left + 50;
            screenshotWindow.Width = targetMonitorShow.Width;
            screenshotWindow.Height = targetMonitorShow.Height;
            screenshotWindow.Show();
        }
    }
}
