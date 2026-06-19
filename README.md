#  Cybersecurity Awareness Bot — Liam (WPF Edition)

A C# / WPF desktop application that educates users about cybersecurity best practices through an interactive, keyword-driven chat interface. Built across three parts, the final version includes a task assistant, quiz mini-game, NLP simulation, and a full activity log. Designed to be extensible, professionally structured, and easy to run on any Windows machine with .NET 9 installed.

---

##  Features

| Feature | Detail |
|---------|--------|
| **Interactive chat** | Continuous conversation loop with keyword matching across 18+ cybersecurity topics |
| **Sentiment detection** | Detects tone (positive, worried, confused, angry) and adapts replies empathetically |
| **Contextual memory** | Remembers mentioned device, browser, and security concerns across the session |
| **Follow-up detection** | Recognises phrases like "tell me more" and expands on the last topic |
| **Session tracking** | Live session timer, message counter, and task count in the status bar |
| **Topic chips** | Clickable buttons for all 18 cybersecurity topics plus quick-action buttons for tasks, quiz, and log |
| **Voice greeting** | Optional `.wav` playback at startup via `System.Media.SoundPlayer` |
| **Dark-themed UI** | GitHub‑style dark interface with colour-coded chat bubbles for bot, user, tasks, and quiz |
| **ASCII branding** | Terminal‑style logo displayed in the left sidebar |
| **Task Assistant** *(Part 3)* | Add, view, complete, and delete cybersecurity tasks with optional date reminders |
| **Quiz Mini-Game** *(Part 3)* | 12-question bank (10 per session, shuffled) with multiple-choice and true/false formats, scoring, and feedback |
| **NLP Simulation** *(Part 3)* | Keyword detection with flexible phrasing so natural language commands are understood |
| **Activity Log** *(Part 3)* | Timestamped log of all significant actions — viewable in chat on request |

---

##  Project Structure

```
CybersecurityBot/
│
├── App.xaml / App.xaml.cs          # WPF application host (StartupUri = MainWindow.xaml)
├── MainWindow.xaml                 # WPF UI layout (sidebar + chat panel)
├── MainWindow.xaml.cs              # Code-behind — bridges UI to all domain classes
│
├── CommandHelper.cs                # Exit / memory-recall command detection
├── ConversationContext.cs          # Memory, follow-up detection, activity event (delegate/event)
├── ResponseEngine.cs               # Keyword → response dictionary + delegate selector strategy
├── SentimentDetector.cs            # Tone detection and empathetic prefix selection
├── UserProfile.cs                  # Session data (name, start time, formatted name, time greeting)
├── VoiceGreeting.cs                # Optional .wav playback (System.Media.SoundPlayer)
│
├── TaskStore.cs          ★ NEW     # In-memory task DB: Add, GetAll, Complete, Delete (simulates MySQL)
├── QuizEngine.cs         ★ NEW     # 12-question quiz bank, shuffled sessions, scoring, feedback
├── NlpParser.cs          ★ NEW     # Intent detection via keyword/regex — flexible natural language
├── ActivityLogger.cs     ★ NEW     # Timestamped action log with summary view (last 10 entries)
│
├── CybersecurityBot.csproj         # .NET 9 WPF project configuration
├── greeting.wav                    # (Optional) placed in build output folder
└── README.md
```

> **Note:** This is a pure WPF application — no console‑based `Program.cs` exists. All interaction happens in the graphical window.

---

##  Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) (or higher)
- Windows OS (required for WPF and `System.Media.SoundPlayer`)
- Visual Studio 2022+ **or** VS Code with the C# extension

### Run with Visual Studio

1. Open `CybersecurityBot.csproj` in Visual Studio.
2. Press **F5** or click **Start**.
3. The WPF window will launch.

### Run with the .NET CLI

```bash
cd CybersecurityBot
dotnet restore
dotnet run

```

The application will start and display the main chat window.

---

##  Optional Voice Greeting

Place a file named `greeting.wav` in the build output folder:

```
bin/Debug/net9.0-windows/greeting.wav
```

If the file is absent, the application continues silently without error.

---

##  How to Use

### Chat
Type any cybersecurity question or topic naturally. Examples:
- `"How do I create a strong password?"`
- `"Tell me about phishing"`
- `"What is ransomware?"`
- `"Tell me more"` — expands on the last topic

### Task Assistant
Manage your cybersecurity to-do list with natural language:

| Command | Example |
|---------|---------|
| Add a task | `add task - Enable two-factor authentication` |
| Add with reminder | `add task - Update my passwords in 3 days` |
| Add (natural phrasing) | `remind me to review my privacy settings` |
| View all tasks | `view tasks` |
| Complete a task | `complete task 1` |
| Delete a task | `delete task 2` |
| Set a reminder | `set reminder for task 1 tomorrow` |

Each task is automatically given a helpful description based on the topic (passwords, 2FA, backups, etc.).

### Quiz Mini-Game
Test your cybersecurity knowledge with 10 randomised questions per session:

```
start quiz
```

- Answer with **A**, **B**, **C**, or **D** (or **A** / **B** for true/false questions)
- Immediate feedback and explanation after every answer
- Final score with personalised feedback (80%+ = cybersecurity pro!)
- Type `stop quiz` at any time to exit early

### Activity Log
View a timestamped summary of everything the bot has done:

```
show activity log
```

Also triggered by: `"what have you done for me?"` or `"recent actions"`

Displays the last 10 actions across four categories: **Task**, **Quiz**, **NLP**, and **Chat**.

### Example Session

```
User:    "Clinton"
Liam:    "Good morning, Clinton!  Great to meet you."

User:    "add task - Enable 2FA on all accounts"
Liam:    " Task added! [1] Enable 2FA on all accounts
          Would you like to set a reminder?"

User:    "in 3 days"
Liam:    " Got it! I'll remind you on 22 Jun 2026."

User:    "start quiz"
Liam:    " CYBERSECURITY QUIZ STARTED! Q1: What should
          you do if you receive an email asking for your
          password? A) Reply  B) Delete  C) Report  D) Ignore"

User:    "C"
Liam:    " Correct! Reporting phishing emails helps protect others."

User:    "show activity log"
Liam:    " Here are the last 3 actions:
          1. [09:14] Quiz: Quiz started
          2. [09:13] Task: Task added: 'Enable 2FA on all accounts'
          3. [09:13] Chat: Session started for Clinton"
```

---

##  Covered Topics

| Topic | Topic | Topic |
|-------|-------|-------|
|  Passwords |  Phishing |  Safe Browsing |
|  Malware |  Privacy |  Social Engineering |
|  2FA / MFA |  VPN |  Ransomware |
|  Wi‑Fi Security |  Encryption |  Data Breach |
|  Hacking |  Software Updates |  Firewalls |
|  Identity Theft |  Spam |  Scams |

Each topic has multiple randomised responses for variety.

---

##  Architecture Notes

### Design patterns used

| Pattern | Where |
|---------|-------|
| **Delegate + Event** | `ConversationContext.OnActivity` — decoupled activity logging |
| **Strategy (delegate)** | `ResponseEngine.SelectResponse` — swappable response-selection logic |
| **Auto-properties** | `UserProfile`, `ConversationContext`, `CyberTask` — clean state |
| **Sealed classes** | `UserProfile`, `ConversationContext`, `TaskStore`, `QuizEngine`, `ActivityLogger` |
| **Static helpers** | `CommandHelper`, `SentimentDetector`, `ResponseEngine`, `NlpParser` — stateless utilities |
| **LINQ** | `TaskStore.GetPending()`, `ActivityLogger.BuildSummary()` — collection querying |
| **Regex** | `NlpParser` — flexible intent and timeframe extraction from free text |

### Extending the response engine

Add a new entry to the `Responses` dictionary in `ResponseEngine.cs`:

```csharp
["your keyword"] = new[]
{
    "First alternative response.",
    "Second alternative response.",
},
```

Then add the display label to `TopicList` — the UI chip will appear automatically.

### Extending the quiz

Add questions to the `Bank` array in `QuizEngine.cs`:

```csharp
new QuizQuestion(
    "Your question text here?",
    new[] { "A) Option one", "B) Option two", "C) Option three", "D) Option four" },
    correctIndex: 2,   // zero-based, so C = index 2
    "Explanation shown after the user answers."
),
```

---

##  Technologies

| | |
|---|---|
| **Language** | C# 13 |
| **Framework** | .NET 9.0 (Windows) |
| **UI** | WPF (Windows Presentation Foundation) |
| **Audio** | `System.Media.SoundPlayer` |
| **NLP** | Keyword detection via `string.Contains()` and `System.Text.RegularExpressions` |
| **Data** | In-memory `List<CyberTask>` simulating a database (MySQL-compatible structure) |
| **CI** | GitHub Actions (`.github/workflows/dotnet.yml`) |

---

##  Part Breakdown

| Part | Features Added |
|------|---------------|
| **Part 1** | Core chatbot logic, keyword responses, sentiment detection, voice greeting |
| **Part 2** | WPF GUI, dark theme, topic chips, session timer, contextual memory, activity event |
| **Part 3** | Task assistant with reminders, quiz mini-game, NLP intent parser, activity log viewer |

---

##  Author

**Nemukongwe Oripfa Clinton**

Developed as a Cybersecurity Awareness Project to promote safe digital practices through accessible, interactive learning.
