// Import core system types
using System;
// Import collection types for dictionaries and lists
using System.Collections.Generic;

// Root namespace for bot components
namespace CybersecurityBot
{
    // Static class providing sentiment analysis utilities
    public static class SentimentDetector
    {
        // Enumeration of sentiment types that can be detected from user input
        public enum Sentiment { Positive, Negative, Worried, Confused, Angry, Neutral }

        // Dictionary mapping sentiment types to keyword lists for detection
        private static readonly IReadOnlyDictionary<Sentiment, string[]> Keywords =
            new Dictionary<Sentiment, string[]>
            {
                // Keywords indicating positive sentiment
                [Sentiment.Positive] = new[] { "great", "thanks", "thank you", "awesome", "love", "cool",
                                               "good", "helpful", "nice", "excellent", "perfect", "amazing",
                                               "brilliant", "fantastic", "wonderful", "appreciate", "glad" },
                // Keywords indicating negative/nervous sentiment
                [Sentiment.Negative] = new[] { "scared", "afraid", "fear", "terrified", "nervous",
                                               "stressed", "worried", "anxious", "unsafe", "vulnerable", "overwhelmed" },
                // Keywords indicating confusion
                [Sentiment.Confused] = new[] { "confused", "don't understand", "not sure", "what is",
                                               "what does", "explain", "how does", "help me understand",
                                               "lost", "unclear", "complicated", "i don't get", "what are" },
                // Keywords indicating anger/frustration
                [Sentiment.Angry]    = new[] { "angry", "frustrated", "annoyed", "hate", "stupid",
                                               "useless", "terrible", "worst", "awful", "ridiculous", "fed up" },
                // Keywords indicating worry/security concerns
                [Sentiment.Worried]  = new[] { "hacked", "attacked", "breached", "stolen", "leaked",
                                               "compromised", "victim", "my account", "someone got in", "suspicious" },
            };

        // Dictionary mapping sentiment types to optional response prefixes
        private static readonly IReadOnlyDictionary<Sentiment, string[]> Prefixes =
            new Dictionary<Sentiment, string[]>
            {
                // Prefixes for positive sentiments
                [Sentiment.Positive] = new[] { "I'm glad you're feeling positive! ",
                                               "Great to hear! ",
                                               "Love the enthusiasm! " },
                // Prefixes for negative/nervous sentiments
                [Sentiment.Negative] = new[] { "I understand this can feel overwhelming — you're not alone. ",
                                               "It's completely normal to feel nervous about this. ",
                                               "Don't worry, I'm here to help you through this. " },
                // Prefixes for confused sentiments
                [Sentiment.Confused] = new[] { "No worries — let me break this down simply for you. ",
                                               "Great question. Let me explain that clearly. ",
                                               "Happy to clarify. Here's a simple explanation: " },
                // Prefixes for angry/frustrated sentiments
                [Sentiment.Angry]    = new[] { "I hear your frustration — let me help sort this out. ",
                                               "I'm sorry you're feeling this way. Let me do my best to help. ",
                                               "I understand — let's tackle this together. " },
                // Prefixes for worried/security-concerned sentiments
                [Sentiment.Worried]  = new[] { "That sounds serious — let's address this right away. ",
                                               "I can hear your concern. Here's what you should do: ",
                                               "Don't panic — here are the steps to take immediately: " },
                // Neutral sentiment has empty prefixes
                [Sentiment.Neutral]  = new[] { "", "", "" },
            };

        // RNG instance for selecting random prefixes
        private static readonly Random _rng = new();

        // Analyze input string and return detected sentiment (or Neutral if no match)
        public static Sentiment Detect(string input)
        {
            // Normalize input to lower-case for case-insensitive matching
            string lower = input.ToLowerInvariant();
            // Iterate through each sentiment type and its associated keywords
            foreach (var (sentiment, words) in Keywords)
                // Check each keyword to see if it appears in the input
                foreach (var word in words)
                    if (lower.Contains(word, StringComparison.OrdinalIgnoreCase))
                        return sentiment;
            // No keyword matched, so return neutral sentiment
            return Sentiment.Neutral;
        }

        // Select and return a random prefix string for the given sentiment
        public static string GetPrefix(Sentiment s)
        {
            // Retrieve the list of prefixes available for this sentiment
            var opts = Prefixes[s];
            // Return a random entry from the list
            return opts[_rng.Next(opts.Length)];
        }

        // Return an emoji corresponding to the given sentiment type
        public static string GetEmoji(Sentiment s) => s switch
        {
            // Emoji for positive sentiment
            Sentiment.Positive => "😊",
            // Emoji for negative/nervous sentiment
            Sentiment.Negative => "😟",
            // Emoji for confused sentiment
            Sentiment.Confused => "🤔",
            // Emoji for angry sentiment
            Sentiment.Angry    => "😤",
            // Emoji for worried sentiment
            Sentiment.Worried  => "⚠️",
            // Default bot emoji for neutral or unknown sentiment
            _                  => "🤖",
        };
    }
}