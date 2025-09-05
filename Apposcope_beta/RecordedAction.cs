using System.Text.Json;
using System.IO;

namespace Apposcope_beta
{
    public class RecordedAction
    {
        public string ActionType { get; set; } = string.Empty;
        public ElementIdentifier Element { get; set; } = new ElementIdentifier();
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
/// Initializes a new RecordedAction with default values.
/// </summary>
/// <remarks>
/// Defaults: <see cref="ActionType"/> is an empty string, <see cref="Element"/> is a new <see cref="ElementIdentifier"/> instance,
/// <see cref="Parameters"/> is an empty dictionary, and <see cref="Timestamp"/> is set to the current time.
/// </remarks>
public RecordedAction() { }

        /// <summary>
        /// Initializes a new RecordedAction with the specified action type and target element.
        /// </summary>
        /// <param name="actionType">The action type (e.g., "click", "input").</param>
        /// <param name="element">The element identifier targeted by the action.</param>
        /// <param name="parameters">Optional key/value parameters for the action; if null an empty dictionary is used.</param>
        public RecordedAction(string actionType, ElementIdentifier element, Dictionary<string, object>? parameters = null)
        {
            ActionType = actionType;
            Element = element;
            Parameters = parameters ?? new Dictionary<string, object>();
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Serializes this instance to an indented JSON string.
        /// </summary>
        /// <returns>An indented JSON representation of this object.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Deserializes the provided JSON into a RecordedAction.
        /// </summary>
        /// <param name="json">JSON string representing a RecordedAction.</param>
        /// <returns>The deserialized RecordedAction, or a new RecordedAction if deserialization produces null.</returns>
        public static RecordedAction FromJson(string json)
        {
            return JsonSerializer.Deserialize<RecordedAction>(json) ?? new RecordedAction();
        }

        /// <summary>
        /// Returns a concise, human-readable representation of this RecordedAction.
        /// </summary>
        /// <remarks>
        /// The string is formatted as "<ActionType> on <Element>" and includes " with key=value, ..." when Parameters contains entries.
        /// </remarks>
        /// <returns>A formatted string describing the action, target element, and optional parameters.</returns>
        public override string ToString()
        {
            var paramString = Parameters.Count > 0 
                ? $" with {string.Join(", ", Parameters.Select(p => $"{p.Key}={p.Value}"))}"
                : "";
            return $"{ActionType} on {Element}{paramString}";
        }
    }

    public class ActionRecording
    {
        public List<RecordedAction> Actions { get; set; } = new List<RecordedAction>();
        public DateTime StartTime { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Adds a RecordedAction to this recording's action list.
        /// </summary>
        /// <param name="action">The RecordedAction to append to the recording.</param>
        public void AddAction(RecordedAction action)
        {
            Actions.Add(action);
        }

        /// <summary>
        /// Creates a RecordedAction from the provided data and appends it to the recording's Actions list.
        /// </summary>
        /// <param name="actionType">The type or name of the action performed (e.g., "click", "input").</param>
        /// <param name="element">The target element identifier that the action applies to.</param>
        /// <param name="parameters">Optional additional data for the action; may be null or an empty dictionary.</param>
        public void AddAction(string actionType, ElementIdentifier element, Dictionary<string, object>? parameters = null)
        {
            var action = new RecordedAction(actionType, element, parameters);
            Actions.Add(action);
        }

        /// <summary>
        /// Serializes this ActionRecording to indented JSON and writes it to the specified file path.
        /// </summary>
        /// <param name="filePath">Filesystem path to write the JSON to. The file will be created or overwritten with the serialized content.</param>
        public void SaveToFile(string filePath)
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Loads an ActionRecording from the specified JSON file.
        /// </summary>
        /// <param name="filePath">Path to the JSON file containing the recording.</param>
        /// <returns>The deserialized ActionRecording, or a new empty ActionRecording if deserialization yields null.</returns>
        public static ActionRecording LoadFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<ActionRecording>(json) ?? new ActionRecording();
        }

        /// <summary>
        /// Serializes this instance to an indented JSON string.
        /// </summary>
        /// <returns>An indented JSON representation of this object.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Returns a concise summary of the recording including the number of recorded actions and the start time.
        /// </summary>
        /// <returns>A formatted string of the form "Recording with {Actions.Count} actions (Started: {StartTime:HH:mm:ss})".</returns>
        public override string ToString()
        {
            return $"Recording with {Actions.Count} actions (Started: {StartTime:HH:mm:ss})";
        }
    }
}