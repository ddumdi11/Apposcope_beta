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

        /// <summary>
/// Initializes a new instance of <see cref="ActionRecorder"/>.
/// </summary>
/// <remarks>
/// The constructor is private to enforce the singleton pattern; use <see cref="Instance"/> to access the shared recorder.
/// </remarks>
private ActionRecorder() { }

        /// <summary>
        /// Begins a new recording session by creating and storing a new ActionRecording and marking the recorder as active.
        /// </summary>
        /// <param name="description">Optional human-readable description for the recording (defaults to empty string).</param>
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

        /// <summary>
        /// Stops the current recording session.
        /// </summary>
        /// <remarks>
        /// Sets the recorder state to not recording. Does not modify or clear the current recording; callers should save or clear the recording as needed. A debug message is written with the total number of recorded actions.
        /// </remarks>
        public void StopRecording()
        {
            _isRecording = false;
            Debug.WriteLine($"Recording stopped. Total actions: {_currentRecording?.Actions.Count ?? 0}");
        }

        /// <summary>
        /// Records a user action by creating a RecordedAction from the given UI element and appending it to the active ActionRecording.
        /// </summary>
        /// <param name="actionType">A string identifying the type of action (e.g., "Click", "SetText").</param>
        /// <param name="element">The AutomationElement that was interacted with; used to create an ElementIdentifier stored with the action.</param>
        /// <param name="parameters">Optional additional action-specific data to store with the recorded action.</param>
        /// <remarks>
        /// This method is a no-op if recording is not active, there is no current recording, or <paramref name="element"/> is null.
        /// Errors encountered while constructing or adding the action are caught and logged; they do not propagate to the caller.
        /// </remarks>
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

        /// <summary>
        /// Saves the active recording to disk. If <paramref name="filePath"/> is null, a timestamped file on the user's Desktop is created and used.
        /// If there is no current recording this method does nothing.
        /// </summary>
        /// <param name="filePath">Optional full path to save the recording file. When omitted, a default Desktop path like <c>recording_yyyyMMdd_HHmmss.json</c> is generated.</param>
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

        /// <summary>
        /// Gets the current active ActionRecording, or null if no recording exists.
        /// </summary>
        /// <returns>The active ActionRecording instance, or null when there is no current recording.</returns>
        public ActionRecording? GetCurrentRecording()
        {
            return _currentRecording;
        }

        /// <summary>
        /// Clears any in-memory recording and resets the recorder state to not recording.
        /// </summary>
        /// <remarks>
        /// Discards the current ActionRecording (if any) and sets the recorder as not recording.
        /// This does not persist or delete any previously saved recording files.
        /// </remarks>
        public void ClearRecording()
        {
            _currentRecording = null;
            _isRecording = false;
        }

        /// <summary>
        /// Generates a timestamped default file path on the current user's Desktop for saving a recording.
        /// </summary>
        /// <returns>The full path to a JSON file named "recording_yyyyMMdd_HHmmss.json" located on the user's Desktop.</returns>
        private string GenerateDefaultFilePath()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"recording_{timestamp}.json";
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
        }
    }
}