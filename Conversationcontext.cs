// Import fundamental system types (DateTime, etc.) used in this class
using System;
// Import collection types like List and HashSet used here
using System.Collections.Generic;

// Declare the namespace that groups project types
namespace CybersecurityBot
{
    // XML doc: brief overview of what ConversationContext tracks
    /// <summary>
    /// Tracks conversation state: last matched topic, message count, contextual
    /// memory (device / browser / concern), follow-up detection, and an activity-log event.
    /// </summary>
    // Define a sealed class to hold conversation-specific state
    public sealed class ConversationContext
    {
        // Section marker: Delegate & event
        // ── Delegate & event ──────────────────────────────────────────────────

        // Delegate type for activity log callbacks
        public delegate void ActivityLogHandler(string message);
        // Event that external code can subscribe to for activity messages
        public event ActivityLogHandler? OnActivity;

        // Section marker: Properties
        // ── Properties ────────────────────────────────────────────────────────

        // Last matched topic name, read-only externally
        public string LastTopic    { get; private set; } = string.Empty;
        // Number of messages recorded in this conversation context
        public int    MessageCount { get; private set; } = 0;
        // Remembered user concern (if any)
        public string UserConcern  { get; private set; } = string.Empty;
        // Remembered user device (if any)
        public string UserDevice   { get; private set; } = string.Empty;
        // Remembered user browser (if any)
        public string UserBrowser  { get; private set; } = string.Empty;

        // Section marker: Follow-up phrases used to detect continuation requests
        // ── Follow-up phrases ─────────────────────────────────────────────────

        // Case-insensitive set of phrases that indicate the user asked for more
        private static readonly HashSet<string> FollowUpPhrases = new(StringComparer.OrdinalIgnoreCase)
        {
            "tell me more", "more info", "explain more", "give me more",
            "another tip",  "more tips", "elaborate",    "go on",
            "continue",     "what else", "anything else", "more please",
            "expand",       "keep going", "go deeper",
        };

        // Section marker: Public API methods
        // ── Public API ────────────────────────────────────────────────────────

        // Detects whether the input is a follow-up request
        public bool IsFollowUp(string input)
        {
            // Normalize input for comparison
            string lower = input.Trim().ToLowerInvariant();
            // Check each follow-up phrase for containment
            foreach (var phrase in FollowUpPhrases)
                if (lower.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                    return true; // return true on first match
            return false; // no follow-up phrase found
        }

        // Record a new user message and associated topic into context
        public void RecordMessage(string input, string topic)
        {
            MessageCount++; // increment the message counter
            LastTopic = topic; // update the last topic
            ExtractMemory(input); // attempt to extract contextual memory from input
            Fire($"User asked about: {topic}"); // emit an activity log event
        }

        // Public helper to log an arbitrary message
        public void Log(string message) => Fire(message);

        // Builds a short recap of the remembered memory pieces
        public string BuildMemoryRecap()
        {
            var parts = new List<string>(); // list to collect memory fragments
            if (!string.IsNullOrEmpty(UserDevice))  parts.Add($"you're on {UserDevice}");
            if (!string.IsNullOrEmpty(UserBrowser)) parts.Add($"you use {UserBrowser}");
            if (!string.IsNullOrEmpty(UserConcern)) parts.Add($"you're concerned about {UserConcern}");
            return parts.Count > 0 ? string.Join(", ", parts) : string.Empty; // join or return empty
        }

        // Section marker: Memory extraction logic from user input
        // ── Memory extraction ─────────────────────────────────────────────────

        // Inspect the input text and set device/browser/concern memory fields
        private void ExtractMemory(string input)
        {
            string lower = input.ToLowerInvariant(); // normalize to lower-case

            if      (lower.Contains("windows"))                          SetDevice("Windows");
            else if (lower.Contains("mac") || lower.Contains("macos"))  SetDevice("Mac");
            else if (lower.Contains("android"))                          SetDevice("Android");
            else if (lower.Contains("iphone") || lower.Contains("ios")) SetDevice("iPhone / iOS");
            else if (lower.Contains("linux"))                            SetDevice("Linux");

            if      (lower.Contains("chrome"))  SetBrowser("Chrome");
            else if (lower.Contains("firefox")) SetBrowser("Firefox");
            else if (lower.Contains("edge"))    SetBrowser("Edge");
            else if (lower.Contains("safari"))  SetBrowser("Safari");
            else if (lower.Contains("brave"))   SetBrowser("Brave");

            if      (lower.Contains("hacked"))     SetConcern("being hacked");
            else if (lower.Contains("scammed"))    SetConcern("being scammed");
            else if (lower.Contains("phishing"))   SetConcern("phishing");
            else if (lower.Contains("malware"))    SetConcern("malware");
            else if (lower.Contains("ransomware")) SetConcern("ransomware");
            else if (lower.Contains("password"))   SetConcern("password security");
            else if (lower.Contains("privacy"))    SetConcern("privacy");
        }

        // Helper to set device and emit an activity only when it changes
        private void SetDevice(string v)  { if (UserDevice  != v) { UserDevice  = v; Fire($"Remembered: device → {v}");  } }
        // Helper to set browser and emit an activity only when it changes
        private void SetBrowser(string v) { if (UserBrowser != v) { UserBrowser = v; Fire($"Remembered: browser → {v}"); } }
        // Helper to set concern and emit an activity only when it changes
        private void SetConcern(string v) { if (UserConcern != v) { UserConcern = v; Fire($"Remembered: concern → {v}"); } }

        // Emit the OnActivity event with a timestamped message if subscribed
        private void Fire(string message) =>
            OnActivity?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}