using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfPoint = System.Windows.Point;
using WpfRectangle = System.Windows.Shapes.Rectangle;

namespace Apposcope_beta
{
    public partial class ScreenshotShowWindow : Window
    {
        private WpfPoint clickLeftPoint;
        private WpfRectangle selectionRectangle;
        private MonitorInfo targetMonitorShow;

        public ScreenshotShowWindow(string imagePath, MonitorInfo finalTargetMonitor)
        {
            InitializeComponent();

            // Monitor, auf dem die zu untersuchende App sich befindet
            targetMonitorShow = finalTargetMonitor;

            // Lade den Screenshot in das Image-Control
            var bitmap = new BitmapImage(new Uri(imagePath));
            ScreenshotImage.Source = bitmap;

            // Setze die Fenstergröße basierend auf der Bildgröße
            this.Width = bitmap.PixelWidth;
            this.Height = bitmap.PixelHeight;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Der Klickpunkt innerhalb des ScreenshotWindow
            WpfPoint clickPoint = e.GetPosition(this);

            // Debug-Ausgabe für den Klickpunkt im ScreenshotWindow
            Debug.WriteLine($"Klickpunkt im ScreenshotWindow: {clickPoint}");

            // Berechne den äquivalenten Klickpunkt auf dem Bildschirm, von dem der Screenshot gemacht wurde

            // Verschiebung des ScreenshotWindow relativ zum Zielmonitor
            double windowOffsetX = this.Left - targetMonitorShow.Left;
            double windowOffsetY = this.Top - targetMonitorShow.Top;

            // Rechne die absoluten Bildschirmkoordinaten
            int screenX = (int)(clickPoint.X + windowOffsetX);
            int screenY = (int)(clickPoint.Y + windowOffsetY);

            // Debug-Ausgabe für den äquivalenten Klickpunkt auf dem Bildschirm
            Debug.WriteLine($"Äquivalenter Klickpunkt auf dem Bildschirm: X = {screenX}, Y = {screenY}");

            if (screenX < 0 || screenY < 0)
            {
                Debug.WriteLine("Die berechneten Koordinaten sind negativ, möglicherweise ist der Monitor links vom Hauptbildschirm.");
            }
            else
            {
                Debug.WriteLine($"Berechnete Bildschirmkoordinaten: X = {screenX}, Y = {screenY}");
            }


            // Verwende den FlaUI-Checker
            var elementChecker = new FlaUIElementChecker();
            elementChecker.CheckAndHighlightElement(screenX, screenY);
        }
    }
}
