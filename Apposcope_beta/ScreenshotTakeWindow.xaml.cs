using System.Diagnostics;
using System.Drawing; // Für Graphics und Bitmap (System.Drawing.Common per NuGet hinzufügen)
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
        // Weitere Felder und Variablen, die zur Klasse gehören
        private WpfPoint startPoint;
        private WpfRectangle selectionRectangle;
        private MonitorData monitorData;
        private MonitorInfo thisMonitor;
        private int screenshotLeft;
        private int screenshotTop;
        public string CapturedImagePath { get; private set; } // Öffentliche Eigenschaft        

        public ScreenshotTakeWindow(MonitorData monitorData)
        {
            InitializeComponent();
            this.monitorData = monitorData;

            // Monitor-Informationen setzen
            SetMonitorInfo();

            // Debug-Ausgabe zur Kontrolle
            Debug.WriteLine("Fenster für die Screenshot-Aufnahme geöffnet!");
            Debug.WriteLine(thisMonitor.ToString());

            // Debugging-Hilfe: Überprüfen, ob die Variable im Konstruktor null ist
            Debug.WriteLine("Konstruktor: showScreenshotWindow ist: " + (App.showScreenshotWindow == null ? "null" : "vorhanden"));

            // Fenster fokussierbar machen für Tasteneingaben
            this.Focus();

        }

        // Setzt die Monitorinformationen und platziert das Fenster
        private void SetMonitorInfo()
        {
            thisMonitor = monitorData.GetLenseScreenMonitor();  // LenseScreen-Monitor festlegen
            this.Left = thisMonitor.SystemLeft;
            this.Top = thisMonitor.SystemTop;
            this.Width = thisMonitor.SystemWidth;
            this.Height = thisMonitor.SystemHeight;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                // Rechtsklick: Abbrechen
                CancelFrameSelection();
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                startPoint = e.GetPosition(this);

                // Rechteck für den Auswahlrahmen erstellen
                selectionRectangle = CreateSelectionRectangle(startPoint);
                secondCanvas.Children.Add(selectionRectangle);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (selectionRectangle == null) return;

            var currentPosition = e.GetPosition(this);
            UpdateSelectionRectangle(currentPosition);
        }

        private async void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selectionRectangle != null)
            {
                // Kleine Verzögerung, um sicherzustellen, dass der Rahmen korrekt gezeichnet wird
                await Task.Delay(100); // 100 Millisekunden

                // Koordinaten des Rahmens ermitteln
                var selectionTopLeft = selectionRectangle.TransformToAncestor(this).Transform(new WpfPoint(0, 0));
                CapturedImagePath = TakeScreenshot(
                    (int)(thisMonitor.SystemLeft + selectionTopLeft.X),
                    (int)(thisMonitor.SystemTop + selectionTopLeft.Y),
                    (int)selectionRectangle.Width,
                    (int)selectionRectangle.Height);

                secondCanvas.Children.Remove(selectionRectangle);
                selectionRectangle = null;
            }

            // Screenshot-Fenster aktualisieren oder anzeigen
            ShowScreenshot(CapturedImagePath);
            this.Close(); // Fenster nach der Auswahl schließen
        }


        // Erstellt das Auswahlrechteck
        private WpfRectangle CreateSelectionRectangle(WpfPoint startPoint)
        {
            return new WpfRectangle
            {
                Stroke = System.Windows.Media.Brushes.Red,
                StrokeThickness = 2,
                Width = 0, // Initial Breite
                Height = 0 // Initial Höhe
            };
        }

        // Aktualisiert das Auswahlrechteck bei Bewegung der Maus
        private void UpdateSelectionRectangle(WpfPoint currentPosition)
        {
            var width = Math.Abs(currentPosition.X - startPoint.X);
            var height = Math.Abs(currentPosition.Y - startPoint.Y);

            // Debug-Ausgaben für die Rahmenposition und -größe
            // Debug.WriteLine($"Rahmen: Start: {startPoint}, Aktuelle Position: {currentPosition}, Breite: {width}, Höhe: {height}");

            selectionRectangle.Width = width;
            selectionRectangle.Height = height;

            Canvas.SetLeft(selectionRectangle, Math.Min(currentPosition.X, startPoint.X));
            Canvas.SetTop(selectionRectangle, Math.Min(currentPosition.Y, startPoint.Y));
        }

        // Screenshot aufnehmen
        private string TakeScreenshot(int x, int y, int width, int height)
        {
            try
            {
                // Erzeuge einen Dateinamen, um sicherzustellen, dass es keine Konflikte gibt
                string screenshotPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                                                              $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");

                using (Bitmap bitmap = new Bitmap(width, height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        // Screenshot erstellen
                        g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));
                    }
                    bitmap.Save(screenshotPath, System.Drawing.Imaging.ImageFormat.Png);
                }

                // Debug-Ausgaben zur Kontrolle
                screenshotLeft = x;
                screenshotTop = y;
                Debug.WriteLine($"Screenshot Abstand von links: {screenshotLeft}");
                Debug.WriteLine($"Screenshot Abstand von oben: {screenshotTop}");

                return screenshotPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Erstellen des Screenshots: {ex.Message}");
                return string.Empty;
            }
        }

        // Screenshot anzeigen
        private void ShowScreenshot(string screenshotPath)
        {
            Debug.WriteLine($"Vor Bedingung: showScreenshotWindow == null: {App.showScreenshotWindow == null}");
            Debug.WriteLine($"showScreenshotWindow == null: {App.showScreenshotWindow == null}"); // Prüfe, ob das Fenster wirklich null ist

            if (string.IsNullOrEmpty(screenshotPath))
            {
                Debug.WriteLine("Kein gültiger Screenshot-Pfad, Fenster wird nicht aktualisiert.");
                return;
            }

            if (App.showScreenshotWindow != null)
            {
                Debug.WriteLine("Aktualisiere bestehendes Screenshot-Fenster.");
                App.showScreenshotWindow.UpdateScreenshot(screenshotPath, screenshotLeft, screenshotTop); // Aktualisiere das Fenster
                //BringWindowToFront(App.showScreenshotWindow); // Fenster in den Vordergrund holen
            }
            else
            {
                Debug.WriteLine("Erstelle neues Screenshot-Fenster.");
                App.showScreenshotWindow = new ScreenshotShowWindow(screenshotPath, monitorData, screenshotLeft, screenshotTop);
                App.showScreenshotWindow.Show();
            }

            Debug.WriteLine($"Nach Erstellung/Update: showScreenshotWindow == null: {App.showScreenshotWindow == null}");


        }




        private void BringWindowToFront(Window window)
        {
            if (!window.IsVisible)
            {
                window.Show();
            }

            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }
            window.Activate();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // ESC-Taste: Abbrechen
                CancelFrameSelection();
            }
        }

        private void CancelFrameSelection()
        {
            Debug.WriteLine("Rahmen-Ziehen abgebrochen.");

            // Rechteck entfernen falls vorhanden
            if (selectionRectangle != null)
            {
                secondCanvas.Children.Remove(selectionRectangle);
                selectionRectangle = null;
            }

            // Fenster schließen ohne Screenshot zu erstellen
            this.Close();
        }

    }
}
