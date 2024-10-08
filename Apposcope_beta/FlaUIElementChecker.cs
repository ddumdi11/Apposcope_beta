using FlaUI.UIA3;
using FlaUI.Core.AutomationElements;  // Für Point und andere Geometrie-Funktionen
using Debug = System.Diagnostics.Debug;
using FlaUI.Core.Exceptions;

public class FlaUIElementChecker
{
    private readonly UIA3Automation automation;

    public FlaUIElementChecker()
    {
        automation = new UIA3Automation();
    }

    public void HighlightElement(double screenX, double screenY, bool doubleClick = false)
    {
        using (var automation = new UIA3Automation())
        {
            try
            {
                var element = automation.FromPoint(new System.Drawing.Point((int)screenX, (int)screenY));

                if (element != null && element.Patterns.Invoke.IsSupported)
                {
                    Debug.WriteLine($"Element gefunden an X: {screenX}, Y: {screenY}");
                    element.DrawHighlight(true, System.Drawing.Color.Red);

                    if (doubleClick)
                    {
                        element.WaitUntilClickable().AsButton().DoubleClick();
                    }
                }
                else
                {
                    Debug.WriteLine($"Kein Element gefunden an X: {screenX}, Y: {screenY}");
                }
            }
            catch (ElementNotAvailableException ex)
            {
                Debug.WriteLine($"Element nicht verfügbar: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unbekannter Fehler: {ex.Message}");
            }
        }
    }
}
