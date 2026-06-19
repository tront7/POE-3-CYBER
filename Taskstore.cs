// Import core system types
using System;
// Import generic collection types
using System.Collections.Generic;
// Import LINQ for querying collections
using System.Linq;

namespace CybersecurityBot
{
    // ── CyberTask ─────────────────────────────────────────────────────────────
    // Represents a single cybersecurity task with optional reminder
    public sealed class CyberTask
    {
        // Auto-incrementing identifier for each task
        public int      Id          { get; }
        // Short title of the task
        public string   Title       { get; }
        // Longer description auto-generated or user-supplied
        public string   Description { get; }
        // Optional date/time for a reminder (null = no reminder)
        public DateTime? ReminderAt  { get; set; }
        // Whether the task has been marked as completed
        public bool     IsCompleted { get; set; }
        // When the task was created
        public DateTime CreatedAt   { get; } = DateTime.Now;

        // Constructor requiring id, title, and description
        public CyberTask(int id, string title, string description)
        {
            Id          = id;
            Title       = title;
            Description = description;
        }

        // Returns a short one-line summary for display in the chat
        public string ToSummaryLine()
        {
            string status  = IsCompleted ? "✅" : "⏳";
            string reminder = ReminderAt.HasValue
                ? $"  🔔 {ReminderAt.Value:dd MMM yyyy}"
                : string.Empty;
            return $"{status} [{Id}] {Title}{reminder}";
        }
    }

    // ── TaskStore ──────────────────────────────────────────────────────────────
    // In-memory store acting as the "database" for tasks.
    // Simulates MySQL CRUD: Add, GetAll, Complete, Delete.
    public sealed class TaskStore
    {
        // Internal list acting as the DB table
        private readonly List<CyberTask> _tasks = new();
        // Auto-increment counter for task IDs
        private int _nextId = 1;

        // INSERT: add a new task and return it
        public CyberTask Add(string title, string description, DateTime? reminderAt = null)
        {
            var task = new CyberTask(_nextId++, title, description)
            {
                ReminderAt = reminderAt
            };
            _tasks.Add(task);
            return task;
        }

        // SELECT ALL: return a read-only snapshot of all tasks
        public IReadOnlyList<CyberTask> GetAll() => _tasks.AsReadOnly();

        // SELECT PENDING: return only incomplete tasks
        public IReadOnlyList<CyberTask> GetPending() =>
            _tasks.Where(t => !t.IsCompleted).ToList();

        // UPDATE: mark a task as completed by ID; returns false if not found
        public bool Complete(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task is null) return false;
            task.IsCompleted = true;
            return true;
        }

        // DELETE: remove a task by ID; returns false if not found
        public bool Delete(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task is null) return false;
            _tasks.Remove(task);
            return true;
        }

        // Returns how many tasks exist
        public int Count => _tasks.Count;
    }
}