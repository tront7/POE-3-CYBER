// Import core system types
using System;
// Import generic collection types
using System.Collections.Generic;
// Import LINQ for querying collections
using System.Linq;
// Import SQLite data access
using Microsoft.Data.Sqlite;
// Import IO for database file path building
using System.IO;

namespace CybersecurityBot
{
    // ── CyberTask ─────────────────────────────────────────────────────────────
    // Represents a single cybersecurity task with optional reminder
    public sealed class CyberTask
    {
        // Unique identifier assigned by the database
        public int       Id          { get; set; }
        // Short title of the task
        public string    Title       { get; set; } = string.Empty;
        // Longer description auto-generated or user-supplied
        public string    Description { get; set; } = string.Empty;
        // Optional date/time for a reminder (null = no reminder)
        public DateTime? ReminderAt  { get; set; }
        // Whether the task has been marked as completed
        public bool      IsCompleted { get; set; }
        // When the task was created
        public DateTime  CreatedAt   { get; set; } = DateTime.Now;

        // Returns a short one-line summary for display in the chat
        public string ToSummaryLine()
        {
            string status   = IsCompleted ? "✅" : "⏳";
            string reminder = ReminderAt.HasValue
                ? $"  🔔 {ReminderAt.Value:dd MMM yyyy}"
                : string.Empty;
            return $"{status} [{Id}] {Title}{reminder}";
        }
    }

    // ── TaskStore ──────────────────────────────────────────────────────────────
    // SQLite-backed task store.
    // The database file (markus_tasks.db) is saved next to the running executable
    // so tasks persist between application sessions.
    public sealed class TaskStore : IDisposable
    {
        // ── Connection ────────────────────────────────────────────────────────

        // Path to the SQLite database file in the app's output directory
        private static readonly string DbPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "markus_tasks.db");

        // Shared persistent connection (opened once, reused throughout the session)
        private readonly SqliteConnection _conn;

        // ── Constructor ───────────────────────────────────────────────────────

        public TaskStore()
        {
            // Open (or create) the database file on disk
            _conn = new SqliteConnection($"Data Source={DbPath}");
            _conn.Open();
            // Ensure the Tasks table exists (safe to call every startup)
            CreateTable();
        }

        // ── Schema ────────────────────────────────────────────────────────────

        // Creates the Tasks table on first run; ignored on subsequent runs
        private void CreateTable()
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title       TEXT    NOT NULL,
                    Description TEXT    NOT NULL,
                    ReminderAt  TEXT,
                    IsCompleted INTEGER NOT NULL DEFAULT 0,
                    CreatedAt   TEXT    NOT NULL
                );";
            cmd.ExecuteNonQuery();
        }

        // ── CRUD operations ───────────────────────────────────────────────────

        // INSERT: add a new task and return it with its generated ID
        public CyberTask Add(string title, string description, DateTime? reminderAt = null)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Tasks (Title, Description, ReminderAt, IsCompleted, CreatedAt)
                VALUES ($title, $desc, $reminder, 0, $created);
                SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("$title",   title);
            cmd.Parameters.AddWithValue("$desc",    description);
            cmd.Parameters.AddWithValue("$reminder",
                reminderAt.HasValue ? (object)reminderAt.Value.ToString("o") : DBNull.Value);
            cmd.Parameters.AddWithValue("$created", DateTime.Now.ToString("o"));

            // Retrieve the auto-generated row ID
            long newId = (long)(cmd.ExecuteScalar() ?? 0L);

            return new CyberTask
            {
                Id          = (int)newId,
                Title       = title,
                Description = description,
                ReminderAt  = reminderAt,
                IsCompleted = false,
                CreatedAt   = DateTime.Now,
            };
        }

        // SELECT ALL: load every task from the database ordered by ID
        public IReadOnlyList<CyberTask> GetAll()
        {
            var list = new List<CyberTask>();

            using var cmd = _conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id, Title, Description, ReminderAt, IsCompleted, CreatedAt " +
                "FROM Tasks ORDER BY Id;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CyberTask
                {
                    Id          = reader.GetInt32(0),
                    Title       = reader.GetString(1),
                    Description = reader.GetString(2),
                    ReminderAt  = reader.IsDBNull(3)
                                    ? null
                                    : DateTime.Parse(reader.GetString(3)),
                    IsCompleted = reader.GetInt32(4) == 1,
                    CreatedAt   = DateTime.Parse(reader.GetString(5)),
                });
            }

            return list;
        }

        // SELECT PENDING: return only incomplete tasks
        public IReadOnlyList<CyberTask> GetPending() =>
            GetAll().Where(t => !t.IsCompleted).ToList();

        // UPDATE: mark a task as completed by ID; returns false if not found
        public bool Complete(int id)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = "UPDATE Tasks SET IsCompleted = 1 WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        // UPDATE: set or update the reminder date for a task by ID
        public bool SetReminder(int id, DateTime reminderAt)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = "UPDATE Tasks SET ReminderAt = $reminder WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$reminder", reminderAt.ToString("o"));
            cmd.Parameters.AddWithValue("$id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        // DELETE: remove a task by ID; returns false if not found
        public bool Delete(int id)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Tasks WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        // Returns how many tasks currently exist in the database
        public int Count
        {
            get
            {
                using var cmd = _conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Tasks;";
                return (int)(long)(cmd.ExecuteScalar() ?? 0L);
            }
        }

        // ── IDisposable ───────────────────────────────────────────────────────

        // Close the database connection cleanly when the app shuts down
        public void Dispose() => _conn.Dispose();
    }
}