using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System.Text.Json;
using CoreControlType = FlaUI.Core.Definitions.ControlType;

namespace Apposcope_beta
{
    public class ElementIdentifier
    {
        public string PrimaryId { get; set; } = string.Empty;
        public string FallbackSelector { get; set; } = string.Empty;
        public string ParentPath { get; set; } = string.Empty;
        public string ControlType { get; set; } = string.Empty;

        public static ElementIdentifier CreateFromElement(AutomationElement element)
        {
            var identifier = new ElementIdentifier();
            
            // 1. Versuche AutomationId (beste Wahl)
            var automationId = element.Properties.AutomationId.TryGetValue(out var autoId) ? autoId : null;
            if (!string.IsNullOrEmpty(automationId))
            {
                identifier.PrimaryId = automationId;
                identifier.ControlType = GetControlTypeName(element);
                identifier.ParentPath = BuildParentPath(element);
                return identifier;
            }

            // 2. Fallback auf Name + ControlType
            var name = element.Properties.Name.TryGetValue(out var elementName) ? elementName : null;
            var controlType = GetControlTypeName(element);
            
            if (!string.IsNullOrEmpty(name))
            {
                identifier.FallbackSelector = $"Name='{name}' AND ControlType='{controlType}'";
                identifier.ControlType = controlType;
                identifier.ParentPath = BuildParentPath(element);
                return identifier;
            }

            // 3. Fallback auf ClassName + Index
            var className = element.Properties.ClassName.TryGetValue(out var elementClass) ? elementClass : null;
            if (!string.IsNullOrEmpty(className))
            {
                var index = GetElementIndexInParent(element);
                identifier.FallbackSelector = $"ClassName='{className}' AND Index={index}";
                identifier.ControlType = controlType;
                identifier.ParentPath = BuildParentPath(element);
                return identifier;
            }

            // 4. Letzte Option: HelpText oder AccessKey
            var helpText = element.Properties.HelpText.TryGetValue(out var help) ? help : null;
            var accessKey = element.Properties.AccessKey.TryGetValue(out var access) ? access : null;
            
            if (!string.IsNullOrEmpty(helpText))
            {
                identifier.FallbackSelector = $"HelpText='{helpText}' AND ControlType='{controlType}'";
            }
            else if (!string.IsNullOrEmpty(accessKey))
            {
                identifier.FallbackSelector = $"AccessKey='{accessKey}' AND ControlType='{controlType}'";
            }
            else
            {
                // Absoluter Notfall: Parent-Path + Index
                var index = GetElementIndexInParent(element);
                identifier.FallbackSelector = $"ControlType='{controlType}' AND Index={index}";
            }
            
            identifier.ControlType = controlType;
            identifier.ParentPath = BuildParentPath(element);
            return identifier;
        }

        private static string GetControlTypeName(AutomationElement element)
        {
            return element.Properties.ControlType.TryGetValue(out var controlType) 
                ? controlType.ToString() 
                : "Unknown";
        }

        private static string BuildParentPath(AutomationElement element)
        {
            var pathParts = new List<string>();
            var parent = element.Parent;
            
            while (parent != null && pathParts.Count < 5)
            {
                var parentId = parent.Properties.AutomationId.TryGetValue(out var id) ? id : null;
                var parentName = parent.Properties.Name.TryGetValue(out var name) ? name : null;
                var parentType = GetControlTypeName(parent);
                
                string pathPart;
                if (!string.IsNullOrEmpty(parentId))
                {
                    pathPart = $"{parentType}[@AutomationId='{parentId}']";
                }
                else if (!string.IsNullOrEmpty(parentName))
                {
                    pathPart = $"{parentType}[@Name='{parentName}']";
                }
                else
                {
                    pathPart = $"{parentType}";
                }
                
                pathParts.Insert(0, pathPart);
                parent = parent.Parent;
            }
            
            return "/" + string.Join("/", pathParts);
        }

        private static int GetElementIndexInParent(AutomationElement element)
        {
            var parent = element.Parent;
            if (parent == null) return 0;
            
            var siblings = parent.FindAllChildren();
            var elementControlType = GetControlTypeName(element);
            
            var index = 0;
            foreach (var sibling in siblings)
            {
                if (GetControlTypeName(sibling) == elementControlType)
                {
                    if (sibling.Equals(element))
                        return index;
                    index++;
                }
            }
            
            return 0;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }

        public static ElementIdentifier FromJson(string json)
        {
            return JsonSerializer.Deserialize<ElementIdentifier>(json) ?? new ElementIdentifier();
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(PrimaryId))
                return $"AutomationId: {PrimaryId} ({ControlType})";
            else
                return $"Fallback: {FallbackSelector}";
        }
    }
}