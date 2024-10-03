using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfPoint = System.Windows.Point;
using WpfRectangle = System.Windows.Shapes.Rectangle;
using Window = System.Windows.Window;

namespace Apposcope_beta
{
    public partial class ScreenshotShowWindow : Window
    {
        private WpfPoint clickLeftPoint;
        private WpfRectangle selectionRectangle;
        private MonitorInfoOld showScreenshotMonitor;
        private MonitorInfoOld takeScreenshotMonitor;

        public ScreenshotShowWindow(string imagePath, MonitorInfoOld showScreenshotMonitor, int screenshotLeft, int screenshotTop, MonitorInfoOld takeScreenshotMonitor)
        {
            InitializeComponent();

            this.takeScreenshotMonitor = takeScreenshotMonitor;

            // Lade den Screenshot und erhalte die Breite und Höhe des Bildes
            BitmapImage bitmap = new BitmapImage();

            try
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.EndInit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Laden des Bildes: {ex.Message}");
                return;
            }

            // Überprüfen, ob das Bild geladen wurde
            if (bitmap.PixelWidth == 0 || bitmap.PixelHeight == 0)
            {
                Debug.WriteLine("Bild konnte nicht geladen werden oder ist leer.");
                return;
            }

            double actualScreenshotWidth = bitmap.PixelWidth;   // Tatsächliche Breite des Screenshots
            double actualScreenshotHeight = bitmap.PixelHeight; // Tatsächliche Höhe des Screenshots
            Debug.WriteLine("Tatsächliche Breite des Screenshots: " + actualScreenshotWidth);
            Debug.WriteLine("Tatsächliche Höhe des Screenshots: " + actualScreenshotHeight);

            // Erstelle das Canvas
            Canvas canvas = new Canvas();

            // Erstelle das Bild und setze es auf das Canvas
            Image screenshotImage = new Image
            {
                Source = bitmap,
                Width = actualScreenshotWidth,  // Setze die tatsächliche Breite des Screenshots
                Height = actualScreenshotHeight // Setze die tatsächliche Höhe des Screenshots
            };

            // Setze das Bild im Canvas an die linke obere Ecke + Koordinaten des Ursprungsmonitors
            if (screenshotLeft > showScreenshotMonitor.Width)
            {
                Canvas.SetLeft(screenshotImage, screenshotLeft - showScreenshotMonitor.Width);
                Canvas.SetTop(screenshotImage, screenshotTop - 361);
            } else
            {
                Canvas.SetLeft(screenshotImage, screenshotLeft);
                Canvas.SetTop(screenshotImage, screenshotTop);

            }            
            

            // Füge das Bild dem Canvas hinzu
            canvas.Children.Add(screenshotImage);                        

            // Füge das Canvas dem Fenster hinzu
            this.Content = canvas;
        }



        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Der Klickpunkt innerhalb des ScreenshotWindow
            WpfPoint clickPoint = e.GetPosition(this);
            WpfPoint absoluteClickPoint = new WpfPoint(clickPoint.X + takeScreenshotMonitor.Width, clickPoint.Y + takeScreenshotMonitor.Top);

            // Debug-Ausgabe für den Klickpunkt im ScreenshotWindow
            Debug.WriteLine($"Klickpunkt im ScreenshotWindow: {clickPoint}");
            Debug.WriteLine($"WPF-Klickpunkt bei zwei Monitoren: {absoluteClickPoint}");

            // Berechne den äquivalenten Klickpunkt auf dem Bildschirm, von dem der Screenshot gemacht wurde
            // Rechne die absoluten Bildschirmkoordinaten
            int screenX = (int)(clickPoint.X);
            int screenY = (int)(clickPoint.Y);

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

            if (e.RightButton == MouseButtonState.Pressed)
            {
                var automation = new UIA3Automation();
                // Finde das Element an den Bildschirmkoordinaten (mit System.Drawing.Point)
                var element = automation.FromPoint(new System.Drawing.Point((int)screenX, (int)screenY));
                element.AsButton().DoubleClick();
            }

        }
    }
}
