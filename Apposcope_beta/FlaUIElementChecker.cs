using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media;
using FlaUI.Core.AutomationElements;  // Für Point und andere Geometrie-Funktionen

public class FlaUIElementChecker
{
    private const bool V = true;

    public void CheckAndHighlightElement(double screenX, double screenY)
    {
        using (var automation = new UIA3Automation())
        {
            // Finde das Element an den Bildschirmkoordinaten (mit System.Drawing.Point)
            var element = automation.FromPoint(new System.Drawing.Point((int)screenX, (int)screenY));

            if (element != null && element.Patterns.Invoke.IsSupported)
            {
                // Element ist klickbar oder anderweitig automatisierbar
                System.Diagnostics.Debug.WriteLine($"Element gefunden an X: {screenX}, Y: {screenY}");

                // Element mit rotem Rahmen markieren
                element.DrawHighlight(V, System.Drawing.Color.Red);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Kein Element gefunden an X: {screenX}, Y: {screenY}");
            }
        }
    }
}
