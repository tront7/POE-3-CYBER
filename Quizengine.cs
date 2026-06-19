// Import core system types
using System;
// Import generic collection types
using System.Collections.Generic;

namespace CybersecurityBot
{
    // ── QuizQuestion ──────────────────────────────────────────────────────────
    // Represents a single quiz question with choices and a correct answer
    public sealed class QuizQuestion
    {
        // The question text shown to the user
        public string   Text          { get; }
        // Available answer options (A, B, C, D — or just A, B for true/false)
        public string[] Options       { get; }
        // Zero-based index into Options for the correct answer
        public int      CorrectIndex  { get; }
        // Brief explanation shown after the user answers
        public string   Explanation   { get; }

        // Constructor for a fully specified question
        public QuizQuestion(string text, string[] options, int correctIndex, string explanation)
        {
            Text         = text;
            Options      = options;
            CorrectIndex = correctIndex;
            Explanation  = explanation;
        }
    }

    // ── QuizEngine ────────────────────────────────────────────────────────────
    // Manages quiz state: question bank, current progress, and scoring
    public sealed class QuizEngine
    {
        // ── Question bank ─────────────────────────────────────────────────────
        // 12 questions covering phishing, passwords, safe browsing, social engineering
        private static readonly QuizQuestion[] Bank = new[]
        {
            // Multiple-choice questions
            new QuizQuestion(
                "What should you do if you receive an email asking for your password?",
                new[] { "A) Reply with your password", "B) Delete the email",
                        "C) Report the email as phishing", "D) Ignore it" },
                2,
                "✅ Correct answer: C — Report phishing emails to help protect others too."),

            new QuizQuestion(
                "Which of these is the strongest password?",
                new[] { "A) password123", "B) MyDog2010",
                        "C) Tr0ub4dor&3", "D) 123456" },
                2,
                "✅ Correct answer: C — A mix of upper/lower case, numbers, and symbols makes it strongest."),

            new QuizQuestion(
                "What does '2FA' stand for?",
                new[] { "A) Two-Factor Authentication", "B) Two-File Access",
                        "C) Two-Form Application", "D) Two-Firewall Alert" },
                0,
                "✅ Correct answer: A — 2FA adds a second verification step beyond your password."),

            new QuizQuestion(
                "Which type of attack tricks users into revealing personal info by pretending to be trustworthy?",
                new[] { "A) Brute Force", "B) Phishing",
                        "C) SQL Injection", "D) Ransomware" },
                1,
                "✅ Correct answer: B — Phishing impersonates trusted entities to steal credentials."),

            new QuizQuestion(
                "What is ransomware?",
                new[] { "A) Software that speeds up your PC",
                        "B) Malware that encrypts your files and demands payment",
                        "C) A type of firewall",
                        "D) A secure backup tool" },
                1,
                "✅ Correct answer: B — Ransomware locks your files until a ransom is paid."),

            new QuizQuestion(
                "Which is the safest way to connect to a public Wi-Fi network?",
                new[] { "A) Connect directly without any precautions",
                        "B) Use a VPN to encrypt your traffic",
                        "C) Disable your firewall first",
                        "D) Use incognito mode" },
                1,
                "✅ Correct answer: B — A VPN encrypts your connection even on unsecured networks."),

            new QuizQuestion(
                "What is social engineering in cybersecurity?",
                new[] { "A) Building social media platforms",
                        "B) Using algorithms to manage networks",
                        "C) Manipulating people into revealing confidential information",
                        "D) Engineering secure software" },
                2,
                "✅ Correct answer: C — Social engineering exploits human psychology, not software flaws."),

            new QuizQuestion(
                "How often should you ideally update your passwords for sensitive accounts?",
                new[] { "A) Never — one strong password is fine forever",
                        "B) Every 6–12 months or after any suspected breach",
                        "C) Only when the system forces you",
                        "D) Every day" },
                1,
                "✅ Correct answer: B — Regular rotation limits damage if a breach occurs."),

            // True / False questions
            new QuizQuestion(
                "TRUE or FALSE: It is safe to use the same password for multiple accounts as long as it is strong.",
                new[] { "A) True", "B) False" },
                1,
                "✅ Correct answer: B (False) — Reusing passwords means one breach exposes all accounts."),

            new QuizQuestion(
                "TRUE or FALSE: HTTPS in a URL always means a website is completely safe and legitimate.",
                new[] { "A) True", "B) False" },
                1,
                "✅ Correct answer: B (False) — HTTPS only encrypts the connection; phishing sites can also use HTTPS."),

            new QuizQuestion(
                "TRUE or FALSE: Antivirus software alone is sufficient to protect you from all cyber threats.",
                new[] { "A) True", "B) False" },
                1,
                "✅ Correct answer: B (False) — Good security requires layered defences: antivirus + updates + safe behaviour."),

            new QuizQuestion(
                "TRUE or FALSE: You should verify unexpected requests for sensitive information, even from known senders.",
                new[] { "A) True", "B) False" },
                0,
                "✅ Correct answer: A (True) — Accounts can be compromised; always verify unusual requests through another channel."),
        };

        // ── State ─────────────────────────────────────────────────────────────

        // Whether a quiz is currently in progress
        public bool IsActive       { get; private set; }
        // Index of the current question within the shuffled list
        public int  CurrentIndex   { get; private set; }
        // Number of correct answers so far
        public int  Score          { get; private set; }
        // Total number of questions in this session
        public int  TotalQuestions => _sessionQuestions.Count;

        // Questions for this quiz session (shuffled subset)
        private List<QuizQuestion> _sessionQuestions = new();
        // RNG for shuffling
        private static readonly Random _rng = new();

        // ── Public API ────────────────────────────────────────────────────────

        // Start a new quiz session; shuffles the bank and takes first 10
        public QuizQuestion Start()
        {
            // Shuffle a copy of the bank
            var shuffled = new List<QuizQuestion>(Bank);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            // Take up to 10 questions
            _sessionQuestions = shuffled.GetRange(0, Math.Min(10, shuffled.Count));
            CurrentIndex = 0;
            Score        = 0;
            IsActive     = true;
            return _sessionQuestions[0];
        }

        // Returns the current question (null if quiz is not active)
        public QuizQuestion? CurrentQuestion =>
            IsActive && CurrentIndex < _sessionQuestions.Count
                ? _sessionQuestions[CurrentIndex]
                : null;

        // Submit an answer letter (A/B/C/D) or full option text.
        // Returns (isCorrect, explanation, nextQuestion or null if finished)
        public (bool isCorrect, string explanation, QuizQuestion? next) SubmitAnswer(string raw)
        {
            if (!IsActive || CurrentQuestion is null)
                return (false, "No active quiz.", null);

            var q = CurrentQuestion;

            // Normalise: accept "A", "a", "A)", "a)" or the full option text
            string trimmed = raw.Trim().ToUpperInvariant().TrimEnd(')');
            int answerIndex = trimmed switch
            {
                "A" => 0,
                "B" => 1,
                "C" => 2,
                "D" => 3,
                _   => -1,
            };

            // If not a letter, try matching option text
            if (answerIndex == -1)
            {
                for (int i = 0; i < q.Options.Length; i++)
                    if (q.Options[i].Contains(raw, StringComparison.OrdinalIgnoreCase))
                    { answerIndex = i; break; }
            }

            bool correct = answerIndex == q.CorrectIndex;
            if (correct) Score++;

            CurrentIndex++;

            // Quiz complete?
            if (CurrentIndex >= _sessionQuestions.Count)
            {
                IsActive = false;
                return (correct, q.Explanation, null);
            }

            return (correct, q.Explanation, _sessionQuestions[CurrentIndex]);
        }

        // Returns score summary feedback string
        public string GetFinalFeedback()
        {
            double pct = (double)Score / TotalQuestions * 100;
            return pct >= 80
                ? $"🏆 Outstanding! You scored {Score}/{TotalQuestions} ({pct:0}%). You're a cybersecurity pro!"
                : pct >= 60
                    ? $"👍 Good effort! You scored {Score}/{TotalQuestions} ({pct:0}%). Keep learning to stay safe online!"
                    : $"📚 You scored {Score}/{TotalQuestions} ({pct:0}%). Keep learning — cybersecurity knowledge is power!";
        }

        // Force-stop the quiz (e.g. user types 'exit quiz')
        public void Stop() => IsActive = false;
    }
}