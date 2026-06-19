// Import core system types
using System;
// Import collection types
using System.Collections.Generic;
// Import LINQ for querying
using System.Linq;
// Import StringBuilder for efficient string building
using System.Text;

namespace CybersecurityBot
{
    // ── ActivityEntry ─────────────────────────────────────────────────────────
    // One timestamped log entry
    public sealed class ActivityEntry
    {
        // When this action occurred
        public DateTime Timestamp { get; } = DateTime.Now;
        // Human-readable description of what happened
        public string   Message   { get; }
        // Category tag (Task / Quiz / NLP / System)
        public string   Category  { get; }

        public ActivityEntry(string message, string category = "System")
        {
            Message  = message;
            Category = category;
        }

        // Format for display in the chat bubble
        public string ToDisplayLine(int index) =>
            $"{index}. [{Timestamp:HH:mm}] {Category}: {Message}";
    }

    // ── ActivityLogger ────────────────────────────────────────────────────────
    // Stores up to MaxEntries log entries and provides summary views.
    public sealed class ActivityLogger
    {
        // Maximum total entries to keep in memory
        private const int MaxEntries = 50;

        // Internal log storage
        private readonly List<ActivityEntry> _log = new();

        // Add a new entry (trims oldest if over capacity)
        public void Log(string message, string category = "System")
        {
            _log.Add(new ActivityEntry(message, category));
            if (_log.Count > MaxEntries)
                _log.RemoveAt(0);
        }

        // Convenience helpers for each category
        public void LogTask(string msg)  => Log(msg, "Task");
        public void LogQuiz(string msg)  => Log(msg, "Quiz");
        public void LogNlp(string msg)   => Log(msg, "NLP");
        public void LogChat(string msg)  => Log(msg, "Chat");

        // Returns the last `count` entries as a formatted chat-ready string
        public string BuildSummary(int count = 10)
        {
            if (_log.Count == 0)
                return "No actions recorded yet. Start chatting, add a task, or try the quiz!";

            var recent = _log
                .Skip(Math.Max(0, _log.Count - count))
                .Reverse()
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"📋 Here are the last {recent.Count} actions:\n");
            for (int i = 0; i < recent.Count; i++)
                sb.AppendLine(recent[i].ToDisplayLine(i + 1));

            return sb.ToString().TrimEnd();
        }

        // Total entries logged so far
        public int Count => _log.Count;
    }
}