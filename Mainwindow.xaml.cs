// Import core system types used throughout this file
using System;
// Import platform support attributes for OS-specific APIs
using System.Runtime.Versioning;
// Import WPF base types like Window, RoutedEventArgs
using System.Windows;
// Import WPF control types such as Button, TextBox, Border
using System.Windows.Controls;
// Import types for inline text elements used in chat bubbles
using System.Windows.Documents;
// Import input-related types like KeyEventArgs
using System.Windows.Input;
// Import media types for brushes, colors and fonts
using System.Windows.Media;
// Import the dispatcher for UI thread scheduling
using System.Windows.Threading;

// Declare the namespace for the application's UI classes
namespace CybersecurityBot
{
    /// <summary>
    /// Code-behind for the WPF main window.
    /// Handles UI event wiring, message rendering, and bridges between
    /// the WPF presentation layer and the shared domain classes.
    /// Part 3 additions: Task Assistant, Quiz Mini-Game, NLP Parser, Activity Log.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public partial class MainWindow : Window
    {
        // ── Session state ─────────────────────────────────────────────────────

        // User profile captured after name entry (nullable until set)
        private UserProfile?              _user;
        // Conversation context instance used to track topics and memory
        private readonly ConversationContext _context      = new();
        // Flag indicating whether the user's name has been entered
        private bool                      _nameEntered  = false;
        // Timer used to update session duration and message count display
        private readonly DispatcherTimer  _sessionTimer = new();

        // ── Part 3: new domain objects ────────────────────────────────────────

        // In-memory task store (simulates MySQL DB)
        private readonly TaskStore        _tasks        = new();
        // Quiz engine managing question flow and scoring
        private readonly QuizEngine       _quiz         = new();
        // Activity logger storing timestamped entries
        private readonly ActivityLogger   _actLog       = new();
        // Flag: waiting for the user to confirm adding a reminder to a new task
        private CyberTask?                _pendingReminderTask = null;

        // ── Design tokens (colour palette) ────────────────────────────────────

        private static readonly SolidColorBrush BotBubble  = Brush(22,  27,  34);
        private static readonly SolidColorBrush UserBubble = Brush(31,  41,  55);
        private static readonly SolidColorBrush BotText    = Brush(88,  166, 255);
        private static readonly SolidColorBrush UserText   = Brush(230, 237, 243);
        private static readonly SolidColorBrush SystemText = Brush(139, 148, 158);
        private static readonly SolidColorBrush QuizBubble = Brush(30,  40,  20);
        private static readonly SolidColorBrush QuizText   = Brush(100, 220, 100);
        private static readonly SolidColorBrush TaskBubble = Brush(20,  30,  50);
        private static readonly SolidColorBrush TaskText   = Brush(180, 200, 255);

        // Helper to create a SolidColorBrush from RGB bytes
        private static SolidColorBrush Brush(byte r, byte g, byte b)
            => new(Color.FromRgb(r, g, b));

        // ── Constructor ───────────────────────────────────────────────────────

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        // ── Initialisation ────────────────────────────────────────────────────

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PlayVoiceGreeting();
            RenderAsciiArt();
            PopulateTopicChips();
            WireActivityLog();
            StartSessionTimer();
            ShowWelcomeMessages();
            InputBox.Focus();
        }

        // ── Voice greeting ────────────────────────────────────────────────────

        private static void PlayVoiceGreeting() => VoiceGreeting.Play();

        // ── ASCII art ─────────────────────────────────────────────────────────

        private void RenderAsciiArt()
        {
            AsciiArt.Text =
                " ██████╗██╗   ██╗██████╗ \n" +
                "██╔════╝╚██╗ ██╔╝██╔══██╗\n" +
                "██║      ╚████╔╝ ██████╔╝\n" +
                "██║       ╚██╔╝  ██╔══██╗\n" +
                "╚██████╗   ██║   ██████╔╝\n" +
                " ╚═════╝   ╚═╝   ╚═════╝ \n" +
                "                         \n" +
                "██╗     ██╗ █████╗ ███╗  \n" +
                "██║     ██║██╔══██╗████╗ \n" +
                "██║     ██║███████║██╔██╗\n" +
                "██║     ██║██╔══██║██║╚██\n" +
                "███████╗██║██║  ██║██║ ╚═\n" +
                "╚══════╝╚═╝╚═╝  ╚═╝╚═╝  ";
        }

        // ── Topic chips ───────────────────────────────────────────────────────

        private void PopulateTopicChips()
        {
            foreach (var topic in ResponseEngine.TopicList)
            {
                var chip = new Button
                {
                    Content = topic,
                    Style   = (Style)FindResource("TopicChip"),
                };

                chip.Click += (_, _) =>
                {
                    if (!_nameEntered)
                    {
                        AddSystemMessage("⚠  Please enter your name first before selecting a topic.");
                        InputBox.Focus();
                        return;
                    }
                    string keyword = topic.Length > 2 ? topic[2..].Trim() : topic;
                    InputBox.Text = keyword;
                    SendMessage();
                };

                TopicsPanel.Children.Add(chip);
            }

            // Add Part 3 quick-action chips
            AddActionChip("📋 Add Task",      "add task - ");
            AddActionChip("📂 View Tasks",    "view tasks");
            AddActionChip("🧩 Start Quiz",    "start quiz");
            AddActionChip("📜 Activity Log",  "show activity log");
        }

        // Helper to create a quick-action chip that pre-fills the input box
        private void AddActionChip(string label, string command)
        {
            var chip = new Button
            {
                Content    = label,
                Style      = (Style)FindResource("TopicChip"),
                Foreground = QuizText,
            };
            chip.Click += (_, _) =>
            {
                if (!_nameEntered)
                {
                    AddSystemMessage("⚠  Please enter your name first.");
                    InputBox.Focus();
                    return;
                }
                // If command ends with a space/dash, let the user complete it
                if (command.EndsWith("- ") || command.EndsWith(": "))
                {
                    InputBox.Text = command;
                    InputBox.CaretIndex = InputBox.Text.Length;
                    InputBox.Focus();
                }
                else
                {
                    InputBox.Text = command;
                    SendMessage();
                }
            };
            TopicsPanel.Children.Add(chip);
        }

        // ── Activity log wiring ───────────────────────────────────────────────

        private void WireActivityLog()
        {
            _context.OnActivity += entry =>
                Dispatcher.Invoke(() =>
                {
                    ActivityLog.Text += entry + "\n";
                    LogScroller.ScrollToEnd();
                });
        }

        // ── Session timer ─────────────────────────────────────────────────────

        private void StartSessionTimer()
        {
            _sessionTimer.Interval = TimeSpan.FromSeconds(1);
            _sessionTimer.Tick += (_, _) =>
            {
                if (_user is not null)
                    SessionLabel.Text =
                        $"⏱ {_user.SessionDuration}  |  💬 {_context.MessageCount} msgs  |  📋 {_tasks.Count} tasks";
            };
            _sessionTimer.Start();
        }

        // ── Welcome messages ──────────────────────────────────────────────────

        private void ShowWelcomeMessages()
        {
            AddBotMessage("👋 Welcome to the Cybersecurity Awareness Bot — Liam!");
            AddBotMessage("I'm here to help you stay informed and protected in the digital world. 🛡");
            AddSystemMessage(
                "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                "To get started, type your name below and press Enter or Send.\n" +
                "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }

        // ── Message dispatch ──────────────────────────────────────────────────

        private void SendMessage()
        {
            string input = InputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            InputBox.Clear();

            // ── Phase 0: Name capture ─────────────────────────────────────────
            if (!_nameEntered)
            {
                if (!InputValidator.IsValidName(input))
                {
                    AddSystemMessage("⚠  Name must be at least 2 characters. Please try again.");
                    return;
                }

                _user        = new UserProfile(input);
                _nameEntered = true;

                _context.Log($"Session started for {_user.FormattedName}");
                _actLog.LogChat($"Session started for {_user.FormattedName}");

                AddUserMessage(input);
                AddBotMessage($"{_user.TimeGreeting}, {_user.FormattedName}! 👋 Great to meet you.");
                AddBotMessage(
                    "Here's what I can do for you:\n\n" +
                    "💬 CHAT  — Ask me anything about cybersecurity\n" +
                    "📋 TASKS — 'add task - Enable 2FA'  |  'view tasks'  |  'complete task 1'\n" +
                    "🧩 QUIZ  — 'start quiz' to test your knowledge\n" +
                    "📜 LOG   — 'show activity log' to see recent actions\n\n" +
                    "💡 Other commands:\n" +
                    "  • 'tell me more'              — more on the last topic\n" +
                    "  • 'what do you know about me' — memory recap\n" +
                    "  • 'help'                      — list all topics\n" +
                    "  • 'exit', 'quit', or 'bye'    — close the app");

                ShowTopicGrid();
                StatusLabel.Text = $"Chatting with {_user.FormattedName}";
                return;
            }

            // ── Phase 1: Pending reminder confirmation ────────────────────────
            if (_pendingReminderTask is not null)
            {
                HandleReminderConfirmation(input);
                return;
            }

            // ── Phase 2: Quiz answer handling ─────────────────────────────────
            if (_quiz.IsActive)
            {
                string lower2 = input.Trim().ToLowerInvariant();
                // Allow stop-quiz commands even during quiz
                if (lower2.Contains("stop quiz") || lower2.Contains("exit quiz") || lower2.Contains("end quiz"))
                {
                    AddUserMessage(input);
                    _quiz.Stop();
                    AddBotMessage("Quiz stopped. Come back any time to try again! Type 'start quiz' to begin.");
                    _actLog.LogQuiz("Quiz stopped by user");
                    _context.Log("Quiz stopped");
                    return;
                }

                AddUserMessage(input);
                HandleQuizAnswer(input);
                return;
            }

            // Display user message for normal flow
            AddUserMessage(input);

            // ── Phase 3: NLP intent detection ────────────────────────────────
            var cmd = NlpParser.Parse(input, _quiz.IsActive);

            switch (cmd.Intent)
            {
                case NlpIntent.Exit:
                    HandleExit(); return;

                case NlpIntent.ShowActivityLog:
                    HandleShowActivityLog(); return;

                case NlpIntent.MemoryRecall:
                    HandleMemoryRecall(); return;

                case NlpIntent.StartQuiz:
                    HandleStartQuiz(); return;

                case NlpIntent.AddTask:
                    HandleAddTask(cmd.TaskTitle, cmd.ReminderRaw); return;

                case NlpIntent.ViewTasks:
                    HandleViewTasks(); return;

                case NlpIntent.CompleteTask:
                    HandleCompleteTask(cmd.TaskId); return;

                case NlpIntent.DeleteTask:
                    HandleDeleteTask(cmd.TaskId); return;

                case NlpIntent.SetReminder:
                    HandleSetReminder(cmd.TaskId, cmd.ReminderRaw); return;

                case NlpIntent.FollowUp:
                    HandleFollowUp(input); return;
            }

            // ── Phase 4: Standard keyword topic response ──────────────────────
            var sentiment = SentimentDetector.Detect(input);
            string prefix = SentimentDetector.GetPrefix(sentiment);
            string emoji  = SentimentDetector.GetEmoji(sentiment);

            string? response = ResponseEngine.GetResponse(input);
            string? topicKey = ResponseEngine.GetMatchedTopicKey(input);

            if (topicKey is not null)
            {
                _context.RecordMessage(input, topicKey);
                _actLog.LogNlp($"Topic matched: {topicKey}");
            }

            if (response is not null)
            {
                string full = string.IsNullOrEmpty(prefix) ? response : $"{prefix}\n{response}";
                AddBotMessage($"{emoji} {full}");

                // Periodic memory recap hint (every 4 messages)
                string recap = _context.BuildMemoryRecap();
                if (!string.IsNullOrEmpty(recap) && _context.MessageCount % 4 == 0)
                    AddSystemMessage($"💭 Remembered: {recap}");
            }
            else
            {
                AddBotMessage(
                    $"I didn't quite catch that, {_user!.FormattedName}. 🤔\n\n" +
                    "Try asking about a topic from the panel, or type:\n" +
                    "  • 'add task - Enable 2FA'\n" +
                    "  • 'view tasks'\n" +
                    "  • 'start quiz'\n" +
                    "  • 'show activity log'\n" +
                    "  • 'how do I create a strong password?'\n\n" +
                    "Type 'help' to see all available topics.");
                _context.Log("Unrecognised input");
                _actLog.LogNlp("Unrecognised input");
            }
        }

        // ── Task Assistant Handlers ───────────────────────────────────────────

        // Handle the intent to add a new task
        private void HandleAddTask(string titleRaw, string reminderRaw)
        {
            string title = string.IsNullOrWhiteSpace(titleRaw) ? "Cybersecurity task" : titleRaw;

            // Auto-generate a helpful description based on common keywords
            string description = GenerateTaskDescription(title);

            // Parse reminder date if already provided inline
            DateTime? reminderDate = NlpParser.ParseReminderDate(reminderRaw);

            var task = _tasks.Add(title, description, reminderDate);
            _actLog.LogTask($"Task added: '{task.Title}'" +
                (reminderDate.HasValue ? $" (reminder: {reminderDate.Value:dd MMM yyyy})" : string.Empty));
            _context.Log($"Task added: {task.Title}");

            string msg = $"📋 Task added!\n\n" +
                         $"  [{task.Id}] {task.Title}\n" +
                         $"  📝 {task.Description}";

            if (reminderDate.HasValue)
            {
                msg += $"\n  🔔 Reminder set for {reminderDate.Value:dd MMM yyyy}";
                AddTaskMessage(msg);
            }
            else
            {
                AddTaskMessage(msg);
                // Ask if they want a reminder
                _pendingReminderTask = task;
                AddBotMessage($"Would you like to set a reminder for this task?\nType a timeframe (e.g. 'in 3 days', 'tomorrow', 'next week') or 'no' to skip.");
            }
        }

        // Handle reminder confirmation for a newly added task
        private void HandleReminderConfirmation(string input)
        {
            AddUserMessage(input);
            string lower = input.Trim().ToLowerInvariant();

            if (lower == "no" || lower == "skip" || lower == "n")
            {
                AddBotMessage($"No reminder set for '{_pendingReminderTask!.Title}'. Task saved! ✅");
                _pendingReminderTask = null;
                return;
            }

            // Try to parse a timeframe from the reply
            string raw = NlpParser.ExtractReminderTime(lower);
            if (string.IsNullOrEmpty(raw)) raw = lower; // use raw input as fallback

            DateTime? date = NlpParser.ParseReminderDate(raw);
            if (date.HasValue)
            {
                _pendingReminderTask!.ReminderAt = date;
                _actLog.LogTask($"Reminder set for '{_pendingReminderTask.Title}' on {date.Value:dd MMM yyyy}");
                _context.Log($"Reminder set: {_pendingReminderTask.Title}");
                AddBotMessage($"🔔 Got it! I'll remind you about '{_pendingReminderTask.Title}' on {date.Value:dd MMM yyyy}.");
            }
            else
            {
                AddBotMessage("I couldn't understand that timeframe. No reminder has been set. Type 'set reminder for task " +
                              $"{_pendingReminderTask!.Id} in 3 days' to add one later.");
            }

            _pendingReminderTask = null;
        }

        // Auto-generate a helpful description for common cybersecurity task keywords
        private static string GenerateTaskDescription(string title)
        {
            string lower = title.ToLowerInvariant();

            if (lower.Contains("2fa") || lower.Contains("two-factor") || lower.Contains("mfa"))
                return "Enable two-factor authentication on your accounts to add an extra layer of security.";
            if (lower.Contains("password"))
                return "Review and update your passwords. Use a password manager and ensure each account has a unique, strong password.";
            if (lower.Contains("privacy") || lower.Contains("settings"))
                return "Review account privacy settings to ensure your personal data is protected and only shared appropriately.";
            if (lower.Contains("backup"))
                return "Back up your important data to a secure location using the 3-2-1 rule: 3 copies, 2 media types, 1 off-site.";
            if (lower.Contains("update") || lower.Contains("patch"))
                return "Apply the latest security updates and patches to your operating system and all installed software.";
            if (lower.Contains("vpn"))
                return "Set up and use a trusted VPN, especially when connecting to public or untrusted Wi-Fi networks.";
            if (lower.Contains("antivirus") || lower.Contains("malware"))
                return "Install and update antivirus/anti-malware software and run a full system scan.";
            if (lower.Contains("phishing") || lower.Contains("email"))
                return "Learn to identify phishing emails and report suspicious messages. Never click unverified links.";

            // Generic description
            return $"Complete the cybersecurity task: {title}.";
        }

        // Handle viewing all tasks
        private void HandleViewTasks()
        {
            var all = _tasks.GetAll();
            if (all.Count == 0)
            {
                AddTaskMessage("📋 You have no tasks yet.\n\nTry: 'add task - Enable 2FA' or 'add task - Update my passwords'");
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"📋 Your Tasks ({all.Count} total)\n");
            foreach (var t in all)
            {
                sb.AppendLine(t.ToSummaryLine());
                sb.AppendLine($"   📝 {t.Description}");
                if (t.ReminderAt.HasValue)
                    sb.AppendLine($"   🔔 Reminder: {t.ReminderAt.Value:dd MMM yyyy}");
                sb.AppendLine();
            }
            sb.AppendLine("────────────────────────────────────");
            sb.AppendLine("Commands: 'complete task 1'  |  'delete task 2'");

            AddTaskMessage(sb.ToString().TrimEnd());
            _actLog.LogTask($"Viewed {all.Count} task(s)");
        }

        // Handle completing a task by ID
        private void HandleCompleteTask(int id)
        {
            if (id <= 0)
            {
                AddBotMessage("Please specify a task ID, e.g. 'complete task 1'. Type 'view tasks' to see all IDs.");
                return;
            }
            bool ok = _tasks.Complete(id);
            if (ok)
            {
                AddTaskMessage($"✅ Task [{id}] marked as completed! Great work on your cybersecurity efforts.");
                _actLog.LogTask($"Task [{id}] completed");
                _context.Log($"Task {id} completed");
            }
            else
            {
                AddBotMessage($"❌ Task [{id}] not found. Type 'view tasks' to see available IDs.");
            }
        }

        // Handle deleting a task by ID
        private void HandleDeleteTask(int id)
        {
            if (id <= 0)
            {
                AddBotMessage("Please specify a task ID, e.g. 'delete task 1'. Type 'view tasks' to see all IDs.");
                return;
            }
            bool ok = _tasks.Delete(id);
            if (ok)
            {
                AddTaskMessage($"🗑 Task [{id}] deleted.");
                _actLog.LogTask($"Task [{id}] deleted");
                _context.Log($"Task {id} deleted");
            }
            else
            {
                AddBotMessage($"❌ Task [{id}] not found. Type 'view tasks' to see available IDs.");
            }
        }

        // Handle setting a reminder on an existing task
        private void HandleSetReminder(int id, string reminderRaw)
        {
            var all = _tasks.GetAll();
            var task = System.Linq.Enumerable.FirstOrDefault(all, t => t.Id == id);
            if (task is null)
            {
                AddBotMessage($"❌ Task [{id}] not found. Type 'view tasks' to see available IDs.");
                return;
            }
            if (string.IsNullOrEmpty(reminderRaw))
            {
                AddBotMessage($"When would you like to be reminded about '{task.Title}'? (e.g. 'in 3 days', 'tomorrow')");
                _pendingReminderTask = task;
                return;
            }
            DateTime? date = NlpParser.ParseReminderDate(reminderRaw);
            if (date.HasValue)
            {
                task.ReminderAt = date;
                AddTaskMessage($"🔔 Reminder set for '{task.Title}' on {date.Value:dd MMM yyyy}.");
                _actLog.LogTask($"Reminder updated for task [{id}]: {date.Value:dd MMM yyyy}");
            }
            else
            {
                AddBotMessage("I couldn't understand that timeframe. Try 'in 5 days' or 'tomorrow'.");
            }
        }

        // ── Quiz Handlers ─────────────────────────────────────────────────────

        // Start a new quiz session
        private void HandleStartQuiz()
        {
            var firstQ = _quiz.Start();
            _actLog.LogQuiz("Quiz started");
            _context.Log("Quiz started");

            AddQuizMessage(
                "🧩 CYBERSECURITY QUIZ STARTED!\n\n" +
                $"You'll get {_quiz.TotalQuestions} questions. Type A, B, C, or D to answer.\n" +
                "Type 'stop quiz' at any time to exit.\n\n" +
                $"────────────────────────────────────\n" +
                FormatQuestion(firstQ, 1));
        }

        // Process a quiz answer
        private void HandleQuizAnswer(string raw)
        {
            var (correct, explanation, next) = _quiz.SubmitAnswer(raw);

            string feedback = correct ? "✅ Correct!" : "❌ Incorrect.";
            string msg = $"{feedback}\n{explanation}";

            if (next is null)
            {
                // Quiz complete
                string finalFeedback = _quiz.GetFinalFeedback();
                AddQuizMessage($"{msg}\n\n────────────────────────────────────\n🏁 Quiz Complete!\n{finalFeedback}");
                _actLog.LogQuiz($"Quiz completed — score: {_quiz.Score}/{_quiz.TotalQuestions}");
                _context.Log($"Quiz finished: {_quiz.Score}/{_quiz.TotalQuestions}");
            }
            else
            {
                // Next question
                int qNum = _quiz.CurrentIndex; // already incremented in SubmitAnswer
                AddQuizMessage($"{msg}\n\n────────────────────────────────────\n{FormatQuestion(next, qNum)}");
            }
        }

        // Format a question for display
        private static string FormatQuestion(QuizQuestion q, int number)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Q{number}: {q.Text}\n");
            foreach (var opt in q.Options)
                sb.AppendLine($"  {opt}");
            return sb.ToString().TrimEnd();
        }

        // ── Activity Log Handler ──────────────────────────────────────────────

        private void HandleShowActivityLog()
        {
            string summary = _actLog.BuildSummary(10);
            AddBotMessage("📜 " + summary);
            _actLog.LogChat("Activity log viewed by user");
            _context.Log("Activity log shown");
        }

        // ── Follow-up handler ─────────────────────────────────────────────────

        private void HandleFollowUp(string input)
        {
            var sentiment = SentimentDetector.Detect(input);
            string prefix = SentimentDetector.GetPrefix(sentiment);
            string emoji  = SentimentDetector.GetEmoji(sentiment);

            if (!string.IsNullOrEmpty(_context.LastTopic))
            {
                string? followUp = ResponseEngine.GetResponse(_context.LastTopic);
                if (followUp is not null)
                {
                    string message = string.IsNullOrEmpty(prefix)
                        ? $"Here's more on '{_context.LastTopic}':\n\n{followUp}"
                        : $"{prefix}Here's more on '{_context.LastTopic}':\n\n{followUp}";
                    AddBotMessage($"{emoji} {message}");
                    _actLog.LogNlp($"Follow-up on: {_context.LastTopic}");
                    _context.Log($"Follow-up delivered for: {_context.LastTopic}");
                    return;
                }
            }
            AddBotMessage("What topic would you like more information about? Try asking about passwords, phishing, VPNs, or any other cybersecurity topic.");
        }

        // ── Memory recall handler ─────────────────────────────────────────────

        private void HandleMemoryRecall()
        {
            string recap = _context.BuildMemoryRecap();
            AddBotMessage(string.IsNullOrEmpty(recap)
                ? $"I haven't learned much about you yet, {_user!.FormattedName}!\n" +
                  "Mention your device, browser, or a security concern and I'll remember it."
                : $"Based on our conversation, I know that {recap}.\n" +
                  "Is there anything specific I can help you with regarding these?");
            _context.Log("Memory recall requested");
            _actLog.LogChat("Memory recall requested");
        }

        // ── Topic grid summary ────────────────────────────────────────────────

        private void ShowTopicGrid()
        {
            string summary = string.Join("    ", ResponseEngine.TopicList);
            AddSystemMessage(summary);
        }

        // ── Chat bubble builders ──────────────────────────────────────────────

        // Standard bot bubble (blue tones)
        private void AddBotMessage(string text)
            => AddChatBubble($"🤖  Liam\n{text}", BotBubble, BotText, HorizontalAlignment.Left);

        // User bubble (right-aligned)
        private void AddUserMessage(string text)
        {
            string label = _user?.FormattedName ?? "You";
            AddChatBubble($"👤  {label}\n{text}", UserBubble, UserText, HorizontalAlignment.Right);
        }

        // Task bubble (blue-tinted, left-aligned)
        private void AddTaskMessage(string text)
            => AddChatBubble($"📋  Tasks\n{text}", TaskBubble, TaskText, HorizontalAlignment.Left);

        // Quiz bubble (green-tinted, left-aligned)
        private void AddQuizMessage(string text)
            => AddChatBubble($"🧩  Quiz\n{text}", QuizBubble, QuizText, HorizontalAlignment.Left);

        // Centered italic system message
        private void AddSystemMessage(string text)
        {
            var block = new TextBlock
            {
                Text                = text,
                Foreground          = SystemText,
                FontFamily          = new FontFamily("Segoe UI"),
                FontSize            = 11,
                FontStyle           = FontStyles.Italic,
                TextWrapping        = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin              = new Thickness(0, 6, 0, 6),
            };
            ChatPanel.Children.Add(block);
            ScrollToBottom();
        }

        // Core bubble builder used by all typed message methods
        private void AddChatBubble(
            string text,
            SolidColorBrush background,
            SolidColorBrush foreground,
            HorizontalAlignment alignment)
        {
            bool isRight = alignment == HorizontalAlignment.Right;

            var border = new Border
            {
                Background          = background,
                CornerRadius        = new CornerRadius(10),
                Padding             = new Thickness(14, 10, 14, 10),
                Margin              = new Thickness(isRight ? 80 : 0, 4, isRight ? 0 : 80, 4),
                HorizontalAlignment = alignment,
                MaxWidth            = 620,
            };

            var block = new TextBlock
            {
                Foreground   = foreground,
                FontFamily   = new FontFamily("Consolas"),
                FontSize     = 12.5,
                TextWrapping = TextWrapping.Wrap,
                LineHeight   = 19,
            };

            var parts = text.Split('\n', 2);
            if (parts.Length == 2)
            {
                block.Inlines.Add(new Run(parts[0] + "\n")
                {
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize   = 10,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = SystemText,
                });
                block.Inlines.Add(new Run(parts[1]));
            }
            else
            {
                block.Text = text;
            }

            border.Child = block;
            ChatPanel.Children.Add(border);
            ScrollToBottom();
        }

        // Scroll the chat view to the bottom asynchronously
        private void ScrollToBottom()
            => Dispatcher.InvokeAsync(
                () => ChatScroller.ScrollToEnd(),
                DispatcherPriority.Background);

        // ── Graceful exit ─────────────────────────────────────────────────────

        private async void HandleExit()
        {
            string name = _user is not null ? $", {_user.FormattedName}" : string.Empty;
            AddBotMessage(
                $"👋 Goodbye{name}! Stay safe out there. 🛡\n" +
                "The application will close in 3 seconds…");
            _actLog.LogChat($"Session ended{name}");
            _context.Log($"Session ended{name}");

            await System.Threading.Tasks.Task.Delay(3_000);
            Application.Current.Shutdown();
        }

        // ── UI event handlers ─────────────────────────────────────────────────

        private void SendButton_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            _context.Log("Chat cleared by user");
            _actLog.LogChat("Chat cleared by user");

            if (_nameEntered && _user is not null)
                AddBotMessage(
                    $"Chat cleared! What else can I help you with, {_user.FormattedName}?\n\n" +
                    "Type 'help' for topics, 'view tasks' for your task list, or 'start quiz' for the quiz.");
            else
                ShowWelcomeMessages();
        }
    }
}