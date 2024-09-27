using System.Windows;
using System.Windows.Media.Imaging;

namespace Apposcope_alpha
{
    public partial class ScreenshotWindow : Window
    {
        public ScreenshotWindow(string imagePath)
        {
            InitializeComponent();

            // Lade den Screenshot in das Image-Control
            var bitmap = new BitmapImage(new Uri(imagePath));
            ScreenshotImage.Source = bitmap;

            // Setze die Fenstergröße basierend auf der Bildgröße
            this.Width = bitmap.PixelWidth;
            this.Height = bitmap.PixelHeight;
        }
    }
}
