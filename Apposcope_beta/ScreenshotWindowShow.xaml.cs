using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        
        // Zoom und Pan Variablen
        private ScaleTransform scaleTransform = new ScaleTransform();
        private TranslateTransform translateTransform = new TranslateTransform();
        private TransformGroup transformGroup = new TransformGroup();
        private bool isPanning = false;
        private WpfPoint panStartPoint;
        
        // Debug-Log
        private static string debugLogPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "", "debug.log");

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

            // Transform-Setup für Zoom und Pan
            SetupTransforms();
            
            // Debug-Log initialisieren
            InitializeDebugLog();

            // Fenster fokussierbar machen für Tasteneingaben
            this.Focus();
        }


        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Der Klickpunkt innerhalb des ScreenshotWindow
            WpfPoint clickPoint = e.GetPosition(this);
            WpfPoint absoluteClickPoint = new WpfPoint(clickPoint.X + thisMonitor.SystemWidth, clickPoint.Y + thisMonitor.SystemTop);

            // Linke Maustaste für Panning
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Prüfen ob der Klick auf dem Canvas ist (nicht auf Buttons)
                var hitTest = e.GetPosition(ScreenshotCanvas);
                var isOnCanvas = hitTest.X >= 0 && hitTest.Y >= 0 && 
                                hitTest.X <= ScreenshotCanvas.ActualWidth && hitTest.Y <= ScreenshotCanvas.ActualHeight;
                
                Debug.WriteLine($"Panning-Check: Canvas-Hit = {isOnCanvas}, HitTest = {hitTest}, Canvas Size = {ScreenshotCanvas.ActualWidth}x{ScreenshotCanvas.ActualHeight}");
                
                if (isOnCanvas)
                {
                    isPanning = true;
                    panStartPoint = clickPoint;
                    this.CaptureMouse();
                    Debug.WriteLine("Panning gestartet");
                    return;
                }
            }

            // Pan-Offset berücksichtigen für korrekte Original-Koordinaten
            int screenX = (int)(clickPoint.X - translateTransform.X);
            int screenY = (int)(clickPoint.Y - translateTransform.Y);

            // Debug-Log schreiben
            LogDebug($"CLICK - Window: {clickPoint} → Screen: {screenX},{screenY} | Pan: {translateTransform.X},{translateTransform.Y} | Scale: {scaleTransform.ScaleX}");

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

            // Rechte Maustaste für Element-Auswahl (nur wenn nicht gepant wird)
            if (e.RightButton == MouseButtonState.Pressed && !isPanning)
            {
                elementChecker.HighlightElement(screenX, screenY, true);                
                var element = FlaUIElementChecker.GetElementAt(screenX, screenY);

                if (element != null)
                {
                    // Öffne das ActionWindow mit den Details zum Element
                    var actionWindow = new ActionWindow(element);
                    actionWindow.Owner = this;
                    actionWindow.Show();

                    // Fenster in den Vordergrund bringen
                    actionWindow.Topmost = true;
                    actionWindow.Topmost = false;
                    actionWindow.Activate();
                    actionWindow.Focus();
                }
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

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // ESC-Taste: Fenster schließen und Hauptfenster wieder anzeigen
                Debug.WriteLine("ScreenshotShowWindow wird über ESC geschlossen - Hauptfenster wird angezeigt.");
                ReturnToMainWindow();
            }
        }

        private void ReturnToMainWindow()
        {
            // Cleanup: App.showScreenshotWindow zurücksetzen
            App.showScreenshotWindow = null;

            // Hauptfenster suchen und anzeigen
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow mainWindow)
                {
                    mainWindow.Show();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.Activate();
                    mainWindow.Focus();
                    Debug.WriteLine("Hauptfenster wurde wieder angezeigt.");
                    break;
                }
            }

            // Dieses Fenster schließen
            this.Close();
        }

        private void BackToMainWindow_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Zurück-Button wurde geklickt.");
            ReturnToMainWindow();
        }

        private void SetupTransforms()
        {
            // Transform-Gruppe erstellen
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);

            // Transform auf das Canvas anwenden
            ScreenshotCanvas.RenderTransform = transformGroup;
            
            // RenderTransformOrigin für Zoom-Zentrum setzen (Mitte)
            ScreenshotCanvas.RenderTransformOrigin = new WpfPoint(0.5, 0.5);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPoint = e.GetPosition(this);
                var deltaX = currentPoint.X - panStartPoint.X;
                var deltaY = currentPoint.Y - panStartPoint.Y;

                translateTransform.X += deltaX;
                translateTransform.Y += deltaY;
                
                // Debug-Log für Pan
                LogDebug($"PAN - Delta: {deltaX},{deltaY} → New Pan: {translateTransform.X},{translateTransform.Y}");

                panStartPoint = currentPoint;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isPanning)
            {
                isPanning = false;
                this.ReleaseMouseCapture();
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            
            scaleTransform.ScaleX *= zoomFactor;
            scaleTransform.ScaleY *= zoomFactor;

            // Zoom-Grenzen
            if (scaleTransform.ScaleX < 0.1)
            {
                scaleTransform.ScaleX = 0.1;
                scaleTransform.ScaleY = 0.1;
            }
            else if (scaleTransform.ScaleX > 10)
            {
                scaleTransform.ScaleX = 10;
                scaleTransform.ScaleY = 10;
            }
        }

        private WpfPoint TransformToScreenCoordinates(WpfPoint canvasClickPoint)
        {
            try
            {
                Debug.WriteLine($"Canvas Click Point: {canvasClickPoint}");
                
                // 2. Rück-transformation: Zoom und Pan berücksichtigen
                // Erst Pan rückgängig machen
                var unPannedX = (canvasClickPoint.X - translateTransform.X);
                var unPannedY = (canvasClickPoint.Y - translateTransform.Y);
                Debug.WriteLine($"Un-panned: {unPannedX}, {unPannedY}");
                
                // Dann Zoom rückgängig machen (durch Scale teilen)
                var originalX = unPannedX / scaleTransform.ScaleX;
                var originalY = unPannedY / scaleTransform.ScaleY;
                Debug.WriteLine($"Original (unscaled): {originalX}, {originalY}");
                
                // 3. Canvas-Offset hinzufügen (Position des Screenshots im Canvas)
                var canvasLeft = Canvas.GetLeft(ScreenshotImage);
                var canvasTop = Canvas.GetTop(ScreenshotImage);
                Debug.WriteLine($"Canvas Offset: {canvasLeft}, {canvasTop}");
                
                var finalX = originalX + canvasLeft;
                var finalY = originalY + canvasTop;
                
                Debug.WriteLine($"Final Screen Coordinates: {finalX}, {finalY}");
                return new WpfPoint(finalX, finalY);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Transform-Fehler: {ex.Message}");
            }

            // Fallback: Originale Koordinaten
            Debug.WriteLine("Fallback zu Original-Koordinaten");
            return canvasClickPoint;
        }

        private static void InitializeDebugLog()
        {
            try
            {
                File.WriteAllText(debugLogPath, $"=== DEBUG LOG STARTED {DateTime.Now} ===\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Log-Init Fehler: {ex.Message}");
            }
        }

        private static void LogDebug(string message)
        {
            try
            {
                File.AppendAllText(debugLogPath, $"{DateTime.Now:HH:mm:ss.fff} - {message}\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Log-Fehler: {ex.Message}");
            }
        }

    }

}
