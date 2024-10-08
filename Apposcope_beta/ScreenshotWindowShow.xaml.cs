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
using Label = System.Windows.Controls.Label;
using Button = System.Windows.Controls.Button;

namespace Apposcope_beta
{
    public partial class ScreenshotShowWindow : Window
    {
        private WpfPoint clickLeftPoint;
        private WpfRectangle selectionRectangle;
        private MonitorData monitorData;
        private MonitorInfo thisMonitor;

        public ScreenshotShowWindow(string imagePath, MonitorData monitorData, int screenshotLeft, int screenshotTop)
        {
            InitializeComponent();

            this.monitorData = monitorData;
            thisMonitor = monitorData.GetRemoteControlMonitor();

            // Positionieren des RemoteControl-Fensters
            this.WindowStyle = WindowStyle.None; // Kein Rahmen
            this.WindowState = WindowState.Normal; // Kein Maximieren, damit wir die Position setzen können
            this.Topmost = true; // Immer im Vordergrund

            // Setze die Position des Fensters auf den Zielmonitor (targetMonitorShow)
            this.Left = thisMonitor.SystemLeft; // X-Koordinate des RemoteControl-Fensters
            this.Top = thisMonitor.SystemTop;   // Y-Koordinate des RemoteControl-Fensters

            // Setze die Größe des Fensters entsprechend dem RemoteControl-Monitor
            this.Width = thisMonitor.SystemWidth;
            this.Height = thisMonitor.SystemHeight;

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

            // Setze die Größe des Canvas basierend auf der Größe des Bildes
            ScreenshotCanvas.Width = actualScreenshotWidth;
            ScreenshotCanvas.Height = actualScreenshotHeight;

            // Setze den Screenshot im bereits vorhandenen Image-Element
            ScreenshotImage.Source = bitmap;

            // Setze das Bild im Canvas an die linke obere Ecke + Koordinaten des Ursprungsmonitors
            Canvas.SetLeft(ScreenshotImage, screenshotLeft);
            Canvas.SetTop(ScreenshotImage, screenshotTop);
        }


        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Der Klickpunkt innerhalb des ScreenshotWindow
            WpfPoint clickPoint = e.GetPosition(this);
            WpfPoint absoluteClickPoint = new WpfPoint(clickPoint.X + thisMonitor.SystemWidth, clickPoint.Y + thisMonitor.SystemTop);

            // Debug-Ausgabe für den Klickpunkt im ScreenshotWindow
            Debug.WriteLine($"Klickpunkt im ScreenshotWindow: {clickPoint}");
            Debug.WriteLine($"WPF-Klickpunkt bei zwei Monitoren: {absoluteClickPoint}");

            // Berechne den äquivalenten Klickpunkt auf dem Bildschirm, von dem der Screenshot gemacht wurde
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

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                elementChecker.HighlightElement(screenX, screenY);
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                elementChecker.HighlightElement(screenX, screenY, true);
            }

        }

        private async void DrawNewFrame_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                Debug.WriteLine("Button wurde geklickt."); // Debug-Ausgabe, um sicherzustellen, dass der Klick registriert wird

                // Ändere den Text und die Farbe des Buttons als Feedback
                button.Content = "Rahmen wird gezogen...";
                button.IsEnabled = false; // Deaktiviere den Button, bis der Vorgang abgeschlossen ist
            }

            this.ShowOverlayMessage("Rahmen ziehen auf dem linken Bildschirm...");

            // Erstelle ein neues ScreenshotTakeWindow
            var screenshotTakeWindow = new ScreenshotTakeWindow(monitorData);
            screenshotTakeWindow.ShowDialog(); // ShowDialog blockiert, bis das Fenster geschlossen wird

            // Rückmeldung, wenn der Vorgang abgeschlossen ist
            if (button != null)
            {
                button.Content = "Neuen Rahmen ziehen";
                button.IsEnabled = true; // Aktiviere den Button wieder
                Debug.WriteLine("Rahmen-Ziehen abgeschlossen, Button wurde zurückgesetzt."); // Weitere Debug-Ausgabe
            }
        }


        public void UpdateScreenshot(string newScreenshotPath, int newScreenshotLeft, int newScreenshotTop)
        {
            Debug.WriteLine("UpdateScreenshot wurde aufgerufen."); // Prüfe, ob die Methode korrekt aufgerufen wird.

            // Lade den neuen Screenshot
            BitmapImage newBitmap = new BitmapImage();

            try
            {
                newBitmap.BeginInit();
                newBitmap.UriSource = new Uri(newScreenshotPath);
                newBitmap.EndInit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Laden des neuen Screenshots: {ex.Message}");
                return;
            }

            // Debug-Ausgaben, um zu sehen, ob der neue Screenshot korrekt geladen wurde
            Debug.WriteLine($"Neuer Screenshot geladen: {newScreenshotPath}");
            Debug.WriteLine($"Screenshot-Größe: Breite = {newBitmap.PixelWidth}, Höhe = {newBitmap.PixelHeight}");


            // Aktualisiere den Screenshot im bestehenden Fenster
            ScreenshotImage.Source = newBitmap;

            // Aktualisiere die Position des Bildes im Canvas, falls erforderlich
            Canvas.SetLeft(ScreenshotImage, newScreenshotLeft);
            Canvas.SetTop(ScreenshotImage, newScreenshotTop);

            Debug.WriteLine("Screenshot wurde im bestehenden Fenster aktualisiert.");
        }

        public void ShowOverlayMessage(string message)
        {
            // Einfaches Overlay-Label
            Label overlay = new Label
            {
                Content = message,
                Background = System.Windows.Media.Brushes.Black,
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Füge das Overlay dem Fenster hinzu
            ScreenshotCanvas.Children.Add(overlay);

            // Entferne das Overlay nach 2 Sekunden
            Task.Delay(2000).ContinueWith(_ =>
            {
                ScreenshotCanvas.Children.Remove(overlay);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

    }

}
