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

        /// <summary>
        /// Creates an ElementIdentifier for the given AutomationElement by selecting the most specific available locator.
        /// </summary>
        /// <remarks>
        /// Selection priority:
        /// 1. AutomationId (sets PrimaryId).
        /// 2. Name + ControlType (sets FallbackSelector to "Name='...' AND ControlType='...').  
        /// 3. ClassName + sibling index (sets FallbackSelector to "ClassName='...' AND Index=...").  
        /// 4. HelpText or AccessKey combined with ControlType, otherwise ControlType + sibling index as an absolute fallback.  
        /// In all cases ControlType and ParentPath are populated before returning.
        /// </remarks>
        /// <param name="element">The UI Automation element to derive an identifier from.</param>
        /// <returns>An ElementIdentifier populated using the best available locator information for the element.</returns>
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

        /// <summary>
        /// Returns the control type name for the given AutomationElement, or "Unknown" if the control type is not available.
        /// </summary>
        /// <param name="element">The AutomationElement to inspect.</param>
        /// <returns>The control type name as a string, or "Unknown" when unavailable.</returns>
        private static string GetControlTypeName(AutomationElement element)
        {
            return element.Properties.ControlType.TryGetValue(out var controlType) 
                ? controlType.ToString() 
                : "Unknown";
        }

        /// <summary>
        /// Builds a concise, slash-delimited path describing up to five ancestor elements of the given element.
        /// </summary>
        /// <param name="element">The element whose parent hierarchy will be inspected.</param>
        /// <returns>
        /// A string starting with '/' and containing up to five ancestor segments separated by '/'. 
        /// Each segment prefers an AutomationId-based form (<c>Type[@AutomationId='id']</c>), then a Name-based form (<c>Type[@Name='name']</c>), and falls back to just the control type (<c>Type</c>) if neither identifier is available.
        /// </returns>
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

        /// <summary>
        /// Gets the zero-based index of the given element among its parent's children that share the same control type.
        /// </summary>
        /// <param name="element">The element whose sibling index to compute.</param>
        /// <returns>
        /// The zero-based position of <paramref name="element"/> among siblings with the same control type.
        /// Returns 0 if the element has no parent or if the element is not found among the parent's children (note: 0 is also returned for a first-position match).
        /// </returns>
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

        /// <summary>
        /// Serializes this ElementIdentifier to an indented JSON string.
        /// </summary>
        /// <returns>A JSON-formatted string representing the current ElementIdentifier, with indentation for readability.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Deserializes a JSON string into an <see cref="ElementIdentifier"/>.
        /// </summary>
        /// <param name="json">A JSON representation of an ElementIdentifier.</param>
        /// <returns>
        /// The deserialized <see cref="ElementIdentifier"/> instance, or a new empty <see cref="ElementIdentifier"/> if deserialization yields null.
        /// </returns>
        public static ElementIdentifier FromJson(string json)
        {
            return JsonSerializer.Deserialize<ElementIdentifier>(json) ?? new ElementIdentifier();
        }

        /// <summary>
        /// Returns a concise human-readable representation of the identifier.
        /// </summary>
        /// <returns>
        /// If PrimaryId is set, returns "AutomationId: {PrimaryId} ({ControlType})"; otherwise returns "Fallback: {FallbackSelector}".
        /// </returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(PrimaryId))
                return $"AutomationId: {PrimaryId} ({ControlType})";
            else
                return $"Fallback: {FallbackSelector}";
        }
    }
}