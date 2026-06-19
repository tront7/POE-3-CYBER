// Import core system types
using System;
// Import Regex for pattern matching
using System.Text.RegularExpressions;

namespace CybersecurityBot
{
    // ── NlpIntent ─────────────────────────────────────────────────────────────
    // Discriminated union of all intent types the NLP layer can detect
    public enum NlpIntent
    {
        None,           // No special intent detected — fall through to ResponseEngine
        AddTask,        // User wants to add a cybersecurity task
        ViewTasks,      // User wants to see their task list
        CompleteTask,   // User wants to mark a task done
        DeleteTask,     // User wants to delete a task
        SetReminder,    // User wants to set/update a reminder on a task
        StartQuiz,      // User wants to start the quiz
        StopQuiz,       // User wants to exit the quiz mid-session
        ShowActivityLog,// User wants to see the activity log
        MemoryRecall,   // User wants to know what the bot remembers about them
        Exit,           // User wants to close the application
        FollowUp,       // User wants more info on the last topic
    }

    // ── ParsedCommand ─────────────────────────────────────────────────────────
    // Result returned by NlpParser.Parse — contains intent and any extracted data
    public sealed class ParsedCommand
    {
        // Detected intent
        public NlpIntent Intent      { get; init; } = NlpIntent.None;
        // Extracted task title / subject (if any)
        public string    TaskTitle   { get; init; } = string.Empty;
        // Extracted reminder timeframe text (e.g. "3 days", "tomorrow")
        public string    ReminderRaw { get; init; } = string.Empty;
        // Extracted task ID (for complete/delete operations)
        public int       TaskId      { get; init; } = -1;
        // The answer letter submitted during quiz (A/B/C/D)
        public string    QuizAnswer  { get; init; } = string.Empty;
    }

    // ── NlpParser ─────────────────────────────────────────────────────────────
    // Stateless keyword-detection NLP layer.
    // Uses string.Contains() and Regex to identify user intent from free text.
    public static class NlpParser
    {
        // Parse the user input and return a ParsedCommand with the detected intent
        public static ParsedCommand Parse(string input, bool quizActive)
        {
            // Normalise for keyword detection
            string lower = input.Trim().ToLowerInvariant();

            // ── Exit ──────────────────────────────────────────────────────────
            if (lower is "exit" or "quit" or "bye")
                return new ParsedCommand { Intent = NlpIntent.Exit };

            // ── Quiz answer (when quiz is active) ─────────────────────────────
            // Accept "A", "B", "C", "D", "a)", "b)" etc. as quiz answers
            if (quizActive)
            {
                var answerMatch = Regex.Match(lower, @"^\s*([a-d])\)?\s*$");
                if (answerMatch.Success)
                    return new ParsedCommand
                    {
                        Intent     = NlpIntent.None, // handled by quiz pipeline
                        QuizAnswer = answerMatch.Groups[1].Value.ToUpperInvariant(),
                    };
            }

            // ── Activity log ──────────────────────────────────────────────────
            if (lower.Contains("show activity log")
             || lower.Contains("what have you done for me")
             || lower.Contains("what have you done")
             || lower.Contains("activity log")
             || lower.Contains("show log")
             || lower.Contains("recent actions"))
                return new ParsedCommand { Intent = NlpIntent.ShowActivityLog };

            // ── Memory recall ─────────────────────────────────────────────────
            if (lower.Contains("what do you know about me")
             || lower.Contains("what have you remembered")
             || lower.Contains("what do you remember"))
                return new ParsedCommand { Intent = NlpIntent.MemoryRecall };

            // ── Start quiz ────────────────────────────────────────────────────
            if (lower.Contains("start quiz")
             || lower.Contains("quiz me")
             || lower.Contains("take the quiz")
             || lower.Contains("begin quiz")
             || lower.Contains("play quiz")
             || (lower.Contains("quiz") && lower.Contains("start")))
                return new ParsedCommand { Intent = NlpIntent.StartQuiz };

            // ── Stop quiz ─────────────────────────────────────────────────────
            if (quizActive && (lower.Contains("stop quiz")
             || lower.Contains("exit quiz")
             || lower.Contains("end quiz")
             || lower.Contains("quit quiz")))
                return new ParsedCommand { Intent = NlpIntent.StopQuiz };

            // ── View tasks ────────────────────────────────────────────────────
            if (lower.Contains("view task")
             || lower.Contains("show task")
             || lower.Contains("list task")
             || lower.Contains("my task")
             || lower.Contains("see my tasks")
             || lower == "tasks")
                return new ParsedCommand { Intent = NlpIntent.ViewTasks };

            // ── Complete task ─────────────────────────────────────────────────
            var completeMatch = Regex.Match(lower, @"(complete|done|finish|mark.*done)\s+(?:task\s+)?#?(\d+)");
            if (completeMatch.Success && int.TryParse(completeMatch.Groups[2].Value, out int cid))
                return new ParsedCommand { Intent = NlpIntent.CompleteTask, TaskId = cid };

            // ── Delete task ───────────────────────────────────────────────────
            var deleteMatch = Regex.Match(lower, @"(delete|remove|cancel)\s+(?:task\s+)?#?(\d+)");
            if (deleteMatch.Success && int.TryParse(deleteMatch.Groups[2].Value, out int did))
                return new ParsedCommand { Intent = NlpIntent.DeleteTask, TaskId = did };

            // ── Add task ──────────────────────────────────────────────────────
            // Flexible phrases: "add task", "add a task", "create task", "new task",
            // "remind me to", "can you remind me to", "I need to"
            bool isAddTask = lower.Contains("add task")
                          || lower.Contains("add a task")
                          || lower.Contains("create task")
                          || lower.Contains("create a task")
                          || lower.Contains("new task")
                          || Regex.IsMatch(lower, @"remind me to .{3,}")
                          || Regex.IsMatch(lower, @"can you remind me to .{3,}")
                          || Regex.IsMatch(lower, @"i need to .{3,}")
                          || Regex.IsMatch(lower, @"set (?:a )?reminder (?:to|for) .{3,}");

            if (isAddTask)
            {
                string title = ExtractTaskTitle(input, lower);
                string reminder = ExtractReminderTime(lower);
                return new ParsedCommand
                {
                    Intent     = NlpIntent.AddTask,
                    TaskTitle  = title,
                    ReminderRaw = reminder,
                };
            }

            // ── Set reminder on existing task ──────────────────────────────────
            var reminderMatch = Regex.Match(lower,
                @"(?:set|add) (?:a )?reminder (?:for|on) (?:task\s+)?#?(\d+)");
            if (reminderMatch.Success && int.TryParse(reminderMatch.Groups[1].Value, out int rid))
            {
                string reminder = ExtractReminderTime(lower);
                return new ParsedCommand
                {
                    Intent     = NlpIntent.SetReminder,
                    TaskId     = rid,
                    ReminderRaw = reminder,
                };
            }

            // ── Follow-up ─────────────────────────────────────────────────────
            if (lower.Contains("tell me more")
             || lower.Contains("more info")
             || lower.Contains("explain more")
             || lower.Contains("another tip")
             || lower.Contains("more tips")
             || lower.Contains("go on")
             || lower.Contains("continue")
             || lower.Contains("what else")
             || lower.Contains("more please"))
                return new ParsedCommand { Intent = NlpIntent.FollowUp };

            // No special intent detected
            return new ParsedCommand { Intent = NlpIntent.None };
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        // Extract a task title from common command patterns
        private static string ExtractTaskTitle(string original, string lower)
        {
            // Try "add task: <title>" or "add task - <title>"
            var colonMatch = Regex.Match(original,
                @"(?:add|create|new)\s+(?:a\s+)?task\s*[-:]\s*(.+)",
                RegexOptions.IgnoreCase);
            if (colonMatch.Success) return colonMatch.Groups[1].Value.Trim();

            // Try "add task <title>"
            var plainMatch = Regex.Match(original,
                @"(?:add|create|new)\s+(?:a\s+)?task\s+(.+)",
                RegexOptions.IgnoreCase);
            if (plainMatch.Success) return CleanTitle(plainMatch.Groups[1].Value);

            // Try "remind me to <title>"
            var remindMatch = Regex.Match(original,
                @"remind me to\s+(.+)",
                RegexOptions.IgnoreCase);
            if (remindMatch.Success) return CleanTitle(remindMatch.Groups[1].Value);

            // Try "can you remind me to <title>"
            var crmMatch = Regex.Match(original,
                @"can you remind me to\s+(.+)",
                RegexOptions.IgnoreCase);
            if (crmMatch.Success) return CleanTitle(crmMatch.Groups[1].Value);

            // Try "I need to <title>"
            var needMatch = Regex.Match(original,
                @"i need to\s+(.+)",
                RegexOptions.IgnoreCase);
            if (needMatch.Success) return CleanTitle(needMatch.Groups[1].Value);

            // Try "set a reminder to/for <title>"
            var setMatch = Regex.Match(original,
                @"set (?:a )?reminder (?:to|for)\s+(.+)",
                RegexOptions.IgnoreCase);
            if (setMatch.Success) return CleanTitle(setMatch.Groups[1].Value);

            // Fallback: strip the command prefix and use the rest
            return original.Trim();
        }

        // Remove trailing reminder phrases from a title string
        private static string CleanTitle(string raw)
        {
            // Remove trailing timeframe phrases so they don't end up in the title
            raw = Regex.Replace(raw,
                @"\s+(?:in\s+\d+\s+days?|tomorrow|next week|on \w+day)\s*$",
                string.Empty, RegexOptions.IgnoreCase).Trim();
            // Capitalise the first letter
            if (raw.Length > 0)
                raw = char.ToUpperInvariant(raw[0]) + raw[1..];
            return raw;
        }

        // Extract a human-readable reminder timeframe from the input
        public static string ExtractReminderTime(string lower)
        {
            var m = Regex.Match(lower,
                @"(?:in\s+)?(\d+)\s+(day|days|week|weeks|hour|hours)");
            if (m.Success) return $"{m.Groups[1].Value} {m.Groups[2].Value}";

            if (lower.Contains("tomorrow"))   return "tomorrow";
            if (lower.Contains("next week"))  return "next week";
            if (lower.Contains("monday"))     return "Monday";
            if (lower.Contains("tuesday"))    return "Tuesday";
            if (lower.Contains("wednesday"))  return "Wednesday";
            if (lower.Contains("thursday"))   return "Thursday";
            if (lower.Contains("friday"))     return "Friday";

            return string.Empty; // no timeframe found
        }

        // Convert a human-readable timeframe string to a DateTime
        public static DateTime? ParseReminderDate(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;

            if (raw == "tomorrow")  return DateTime.Today.AddDays(1);
            if (raw == "next week") return DateTime.Today.AddDays(7);

            // "N days" / "N weeks" / "N hours"
            var m = Regex.Match(raw, @"(\d+)\s+(day|days|week|weeks|hour|hours)");
            if (m.Success)
            {
                int n = int.Parse(m.Groups[1].Value);
                return m.Groups[2].Value.StartsWith("week") ? DateTime.Today.AddDays(n * 7)
                     : m.Groups[2].Value.StartsWith("hour") ? DateTime.Now.AddHours(n)
                     : DateTime.Today.AddDays(n);
            }

            // Day names — find next occurrence
            string[] dayNames = { "monday","tuesday","wednesday","thursday","friday","saturday","sunday" };
            for (int i = 0; i < dayNames.Length; i++)
                if (raw.ToLowerInvariant() == dayNames[i])
                {
                    DayOfWeek target = (DayOfWeek)((i + 1) % 7);
                    int daysAhead = ((int)target - (int)DateTime.Today.DayOfWeek + 7) % 7;
                    if (daysAhead == 0) daysAhead = 7;
                    return DateTime.Today.AddDays(daysAhead);
                }

            return null;
        }
    }
}