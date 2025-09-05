using System.Text;
using System.IO;

namespace Apposcope_beta
{
    public class FlaUIScriptGenerator
    {
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

        public static void SaveScriptToFile(ActionRecording recording, string filePath)
        {
            var script = GenerateScript(recording);
            File.WriteAllText(filePath, script);
        }
    }
}