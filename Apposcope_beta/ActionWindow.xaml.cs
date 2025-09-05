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

        /// <summary>
        /// Record a "DoubleClick" action for the stored AutomationElement, perform a double-click on that element, and close the window.
        /// </summary>
        private void DoubleClickAction(object sender, RoutedEventArgs e)
        {
            ActionRecorder.Instance.RecordAction("DoubleClick", element);
            element.AsButton().DoubleClick();
            this.Close(); // Fenster schließen nach der Aktion
        }

        /// <summary>
        /// Records a "Click" action for the stored AutomationElement, invokes a single click on it, and closes the window.
        /// </summary>
        private void SingleClickAction(object sender, RoutedEventArgs e)
        {
            ActionRecorder.Instance.RecordAction("Click", element);
            element.AsButton().Click();
            this.Close();
        }

        /// <summary>
        /// Records a "SendKeys" action for the stored AutomationElement, enters the literal text "Test" into the element's text box, and closes the window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data associated with the routed event.</param>
        private void SendKeysAction(object sender, RoutedEventArgs e)
        {
            var parameters = new Dictionary<string, object> { { "text", "Test" } };
            ActionRecorder.Instance.RecordAction("SendKeys", element, parameters);
            element.AsTextBox().Enter("Test"); // Beispieltext
            this.Close();
        }

        /// <summary>
        /// Records an "ExpandDropdown" action for the current element, expands the element as a combo box, and closes the window.
        /// </summary>
        /// <remarks>
        /// Side effects: calls ActionRecorder.Instance.RecordAction("ExpandDropdown", element), expands the target element via <c>AsComboBox().Expand()</c>, and closes this window.
        /// </remarks>
        private void OpenDropdownAction(object sender, RoutedEventArgs e)
        {
            ActionRecorder.Instance.RecordAction("ExpandDropdown", element);
            element.AsComboBox().Expand();
            this.Close();
        }
    }
}
