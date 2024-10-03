using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media;
using FlaUI.Core.AutomationElements;  // Für Point und andere Geometrie-Funktionen
using Debug = System.Diagnostics.Debug;

public class FlaUIElementChecker : IDisposable
{
    private readonly UIA3Automation automation;

    public FlaUIElementChecker()
    {
        automation = new UIA3Automation();
    }

    public void CheckAndHighlightElement(double screenX, double screenY)
    {
        var element = automation.FromPoint(new System.Drawing.Point((int)screenX, (int)screenY));

        if (element != null && element.Patterns.Invoke.IsSupported)
        {
            System.Diagnostics.Debug.WriteLine($"Element gefunden an X: {screenX}, Y: {screenY}");
            element.DrawHighlight(true, System.Drawing.Color.Red);
        }
        else
        {
            Debug.WriteLine($"Kein Element gefunden an X: {screenX}, Y: {screenY}");
        }
    }

    public void Dispose()
    {
        automation.Dispose();
    }
}
