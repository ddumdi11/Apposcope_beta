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

        public RecordedAction() { }

        public RecordedAction(string actionType, ElementIdentifier element, Dictionary<string, object>? parameters = null)
        {
            ActionType = actionType;
            Element = element;
            Parameters = parameters ?? new Dictionary<string, object>();
            Timestamp = DateTime.Now;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }

        public static RecordedAction FromJson(string json)
        {
            return JsonSerializer.Deserialize<RecordedAction>(json) ?? new RecordedAction();
        }

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

        public void AddAction(RecordedAction action)
        {
            Actions.Add(action);
        }

        public void AddAction(string actionType, ElementIdentifier element, Dictionary<string, object>? parameters = null)
        {
            var action = new RecordedAction(actionType, element, parameters);
            Actions.Add(action);
        }

        public void SaveToFile(string filePath)
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public static ActionRecording LoadFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<ActionRecording>(json) ?? new ActionRecording();
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }

        public override string ToString()
        {
            return $"Recording with {Actions.Count} actions (Started: {StartTime:HH:mm:ss})";
        }
    }
}