using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System.Windows;
using Window = System.Windows.Window;

namespace Apposcope_beta
{
    public partial class ActionWindow : Window
    {
        private AutomationElement element;

        public ActionWindow(AutomationElement clickedElement)
        {
            InitializeComponent();
            element = clickedElement;

            // Elementdetails anzeigen
            ElementNameText.Text = $"Element Name: {element.Name}";
            var controlType = element.Properties.ControlType.TryGetValue(out var control) ? control.ToString() : "Unbekannt";
            ElementTypeText.Text = $"Element Typ: {controlType}";


        }

        // Aktion: Doppelklick
        private void DoubleClickAction(object sender, RoutedEventArgs e)
        {
            element.AsButton().DoubleClick();
            this.Close(); // Fenster schließen nach der Aktion
        }

        // Aktion: Einfachklick
        private void SingleClickAction(object sender, RoutedEventArgs e)
        {
            element.AsButton().Click();
            this.Close();
        }

        // Aktion: SendKeys
        private void SendKeysAction(object sender, RoutedEventArgs e)
        {
            element.AsTextBox().Enter("Test"); // Beispieltext
            this.Close();
        }

        // Aktion: Dropdown öffnen
        private void OpenDropdownAction(object sender, RoutedEventArgs e)
        {
            element.AsComboBox().Expand();
            this.Close();
        }
    }
}
