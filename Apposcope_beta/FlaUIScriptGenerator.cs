using System.Text;
using System.IO;

namespace Apposcope_beta
{
    public class FlaUIScriptGenerator
    {
        /// <summary>
        /// Generates a complete C# FlaUI automation script from the given action recording.
        /// </summary>
        /// <param name="recording">An ActionRecording containing the sequence of recorded actions to convert into code.</param>
        /// <returns>A string with the full C# source for a GeneratedAutomation.AutomatedTest class that, when compiled and run, will perform the recorded actions using UIA3Automation and the included helper element-finding methods.</returns>
        public static string GenerateScript(ActionRecording recording)
        {
            var script = new StringBuilder();
            
            // Header
            script.AppendLine("using FlaUI.Core.AutomationElements;");
            script.AppendLine("using FlaUI.UIA3;");
            script.AppendLine("using System;");
            script.AppendLine();
            script.AppendLine("namespace GeneratedAutomation");
            script.AppendLine("{");
            script.AppendLine("    public class AutomatedTest");
            script.AppendLine("    {");
            script.AppendLine("        public static void Execute()");
            script.AppendLine("        {");
            script.AppendLine("            using (var automation = new UIA3Automation())");
            script.AppendLine("            {");
            
            // Actions
            foreach (var action in recording.Actions)
            {
                script.AppendLine(GenerateActionCode(action));
                script.AppendLine();
            }
            
            // Footer
            script.AppendLine("            }");
            script.AppendLine("        }");
            script.AppendLine();
            
            // Helper methods
            script.AppendLine(GenerateHelperMethods());
            
            script.AppendLine("    }");
            script.AppendLine("}");
            
            return script.ToString();
        }

        /// <summary>
        /// Generates the C# code fragment that performs a single recorded UI action.
        /// </summary>
        /// <remarks>
        /// The produced fragment includes: an element lookup (using the element identifier from <paramref name="action"/>),
        /// a null-check, and the action invocation (Click, DoubleClick, SendKeys, ExpandDropdown). For SendKeys, the
        /// method uses action.Parameters["text"] if present, otherwise the literal "Test". The returned string is trimmed
        /// of trailing whitespace/newlines.
        /// </remarks>
        /// <param name="action">The recorded action to convert into a code block.</param>
        /// <returns>A string containing the generated C# code for the action, or an if/else block that logs when the element is not found.</returns>
        private static string GenerateActionCode(RecordedAction action)
        {
            var code = new StringBuilder();
            var elementVar = $"element{action.GetHashCode().ToString().Replace("-", "N")}";
            
            // Element finding
            code.AppendLine($"                // {action.ActionType} action");
            code.AppendLine($"                var {elementVar} = {GenerateElementFinder(action.Element)};");
            code.AppendLine($"                if ({elementVar} != null)");
            code.AppendLine("                {");
            
            // Action execution
            switch (action.ActionType)
            {
                case "Click":
                    code.AppendLine($"                    {elementVar}.AsButton().Click();");
                    break;
                case "DoubleClick":
                    code.AppendLine($"                    {elementVar}.AsButton().DoubleClick();");
                    break;
                case "SendKeys":
                    var text = action.Parameters.ContainsKey("text") ? action.Parameters["text"].ToString() : "Test";
                    code.AppendLine($"                    {elementVar}.AsTextBox().Enter(\"{text}\");");
                    break;
                case "ExpandDropdown":
                    code.AppendLine($"                    {elementVar}.AsComboBox().Expand();");
                    break;
                default:
                    code.AppendLine($"                    // Unknown action: {action.ActionType}");
                    break;
            }
            
            code.AppendLine("                }");
            code.AppendLine("                else");
            code.AppendLine("                {");
            code.AppendLine($"                    Console.WriteLine(\"Element not found for {action.ActionType} action\");");
            code.AppendLine("                }");
            
            return code.ToString().TrimEnd();
        }

        /// <summary>
        /// Produces a C# expression (as a string) that, when emitted in the generated script, will locate the target UI element described by <paramref name="identifier"/>.
        /// </summary>
        /// <param name="identifier">An element descriptor containing either a PrimaryId (AutomationId) or a FallbackSelector with one of: Name=, ClassName= (optionally with Index), or HelpText=.</param>
        /// <returns>
        /// A string containing a call to one of the generated helper finders (e.g. "FindElementByAutomationId(automation, \"id\")",
        /// "FindElementByName(automation, \"name\")", "FindElementByClassName(automation, \"class\", index)" or
        /// "FindElementByHelpText(automation, \"text\")"). If no valid identifier can be derived, returns the literal string
        /// "null // Could not generate element finder".
        /// </returns>
        private static string GenerateElementFinder(ElementIdentifier identifier)
        {
            if (!string.IsNullOrEmpty(identifier.PrimaryId))
            {
                // Use AutomationId
                return $"FindElementByAutomationId(automation, \"{identifier.PrimaryId}\")";
            }
            else if (!string.IsNullOrEmpty(identifier.FallbackSelector))
            {
                // Parse fallback selector
                if (identifier.FallbackSelector.Contains("Name="))
                {
                    var name = ExtractValue(identifier.FallbackSelector, "Name");
                    return $"FindElementByName(automation, \"{name}\")";
                }
                else if (identifier.FallbackSelector.Contains("ClassName="))
                {
                    var className = ExtractValue(identifier.FallbackSelector, "ClassName");
                    var index = ExtractValue(identifier.FallbackSelector, "Index");
                    return $"FindElementByClassName(automation, \"{className}\", {index})";
                }
                else if (identifier.FallbackSelector.Contains("HelpText="))
                {
                    var helpText = ExtractValue(identifier.FallbackSelector, "HelpText");
                    return $"FindElementByHelpText(automation, \"{helpText}\")";
                }
            }
            
            return "null // Could not generate element finder";
        }

        /// <summary>
        /// Extracts the value for a named property from a selector string.
        /// </summary>
        /// <param name="selector">The selector string to parse (e.g., "Name='OK' ClassName='Button' Index=2").</param>
        /// <param name="property">The property name to extract (e.g., "Name", "ClassName", or "Index").</param>
        /// <returns>The extracted property value, or an empty string if the property is not present or cannot be parsed. For "Index" the method also accepts an unquoted numeric form like <c>Index=2</c>.</returns>
        private static string ExtractValue(string selector, string property)
        {
            var startIndex = selector.IndexOf($"{property}='") + property.Length + 2;
            var endIndex = selector.IndexOf("'", startIndex);
            if (startIndex > property.Length + 1 && endIndex > startIndex)
            {
                return selector.Substring(startIndex, endIndex - startIndex);
            }
            
            // Try without quotes for Index
            if (property == "Index")
            {
                startIndex = selector.IndexOf($"{property}=") + property.Length + 1;
                var spaceIndex = selector.IndexOf(" ", startIndex);
                endIndex = spaceIndex > 0 ? spaceIndex : selector.Length;
                if (startIndex > property.Length && endIndex > startIndex)
                {
                    return selector.Substring(startIndex, endIndex - startIndex);
                }
            }
            
            return "";
        }

        /// <summary>
        /// Returns a C# source-code fragment defining helper methods used by the generated script to locate UI elements.
        /// </summary>
        /// <returns>
        /// A string containing four private helper method definitions: 
        /// FindElementByAutomationId(UIA3Automation, string), FindElementByName(UIA3Automation, string), 
        /// FindElementByClassName(UIA3Automation, string, int), and FindElementByHelpText(UIA3Automation, string).
        /// Each helper returns an AutomationElement when found or null on failure or exception.
        /// </returns>
        private static string GenerateHelperMethods()
        {
            return @"        private static AutomationElement FindElementByAutomationId(UIA3Automation automation, string automationId)
        {
            try
            {
                var desktop = automation.GetDesktop();
                return desktop.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            }
            catch
            {
                return null;
            }
        }

        private static AutomationElement FindElementByName(UIA3Automation automation, string name)
        {
            try
            {
                var desktop = automation.GetDesktop();
                return desktop.FindFirstDescendant(cf => cf.ByName(name));
            }
            catch
            {
                return null;
            }
        }

        private static AutomationElement FindElementByClassName(UIA3Automation automation, string className, int index)
        {
            try
            {
                var desktop = automation.GetDesktop();
                var elements = desktop.FindAllDescendants(cf => cf.ByClassName(className));
                return index < elements.Length ? elements[index] : null;
            }
            catch
            {
                return null;
            }
        }

        private static AutomationElement FindElementByHelpText(UIA3Automation automation, string helpText)
        {
            try
            {
                var desktop = automation.GetDesktop();
                return desktop.FindFirstDescendant(cf => cf.ByHelpText(helpText));
            }
            catch
            {
                return null;
            }
        }";
        }

        /// <summary>
        /// Generates a C# FlaUI automation script from the given recording and writes it to the specified file path.
        /// </summary>
        /// <param name="recording">The action recording to convert into a generated script.</param>
        /// <param name="filePath">The output file path where the generated script will be written; existing files will be overwritten.</param>
        public static void SaveScriptToFile(ActionRecording recording, string filePath)
        {
            var script = GenerateScript(recording);
            File.WriteAllText(filePath, script);
        }
    }
}