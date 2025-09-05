using FlaUI.Core.AutomationElements;
using System.Diagnostics;
using System.IO;

namespace Apposcope_beta
{
    public class ActionRecorder
    {
        private static ActionRecorder? _instance;
        public static ActionRecorder Instance => _instance ??= new ActionRecorder();

        private ActionRecording? _currentRecording;
        private bool _isRecording = false;

        public bool IsRecording => _isRecording;
        public ActionRecording? CurrentRecording => _currentRecording;

        private ActionRecorder() { }

        public void StartRecording(string description = "")
        {
            _currentRecording = new ActionRecording
            {
                Description = description,
                StartTime = DateTime.Now
            };
            _isRecording = true;
            
            Debug.WriteLine($"Recording started: {description}");
        }

        public void StopRecording()
        {
            _isRecording = false;
            Debug.WriteLine($"Recording stopped. Total actions: {_currentRecording?.Actions.Count ?? 0}");
        }

        public void RecordAction(string actionType, AutomationElement element, Dictionary<string, object>? parameters = null)
        {
            if (!_isRecording || _currentRecording == null || element == null)
                return;

            try
            {
                var identifier = ElementIdentifier.CreateFromElement(element);
                var action = new RecordedAction(actionType, identifier, parameters);
                
                _currentRecording.AddAction(action);
                
                Debug.WriteLine($"Action recorded: {action}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to record action: {ex.Message}");
            }
        }

        public void SaveCurrentRecording(string? filePath = null)
        {
            if (_currentRecording == null)
                return;

            filePath ??= GenerateDefaultFilePath();
            
            try
            {
                _currentRecording.SaveToFile(filePath);
                Debug.WriteLine($"Recording saved to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save recording: {ex.Message}");
            }
        }

        public ActionRecording? GetCurrentRecording()
        {
            return _currentRecording;
        }

        public void ClearRecording()
        {
            _currentRecording = null;
            _isRecording = false;
        }

        private string GenerateDefaultFilePath()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"recording_{timestamp}.json";
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
        }
    }
}