// Import core system types used by this file
using System; // provides fundamental classes and base types

// Define the namespace for the bot's types
namespace CybersecurityBot // root namespace for this project
{ // start of namespace
    // Static helper class that contains command parsing utilities
    public static class CommandHelper // class for command-related helpers
    { // start of CommandHelper
        // Returns true when the input represents an exit command
        public static bool IsExit(string input) // check for exit intent
        { // start of IsExit
            // Normalize input by trimming whitespace and converting to lower-case
            string t = input.Trim().ToLowerInvariant(); // normalized input
            // Check for common exit keywords and return the boolean result
            return t is "exit" or "quit" or "bye"; // match exit words
        } // end of IsExit

        // Returns true when the input asks the bot to recall stored memory
        public static bool IsMemoryRecall(string input) // check for memory-recall intent
        { // start of IsMemoryRecall
            // Convert the input to lower-case for case-insensitive matching
            string lower = input.ToLowerInvariant(); // lower-cased input
            // Check whether the input contains any of the memory-related phrases
            return lower.Contains("what do you know about me") || // phrase variant 1
                   lower.Contains("what have you remembered") || // phrase variant 2
                   lower.Contains("what do you remember"); // phrase variant 3
        } // end of IsMemoryRecall
    } // end of CommandHelper

    // Static helper class for validating user input values
    public static class InputValidator // class for input validation utilities
    { // start of InputValidator
        // Minimum length allowed for a name value
        public const int MinNameLength = 2; // constant defining minimum name length
        // Returns true when the provided name is non-empty and meets the length requirement
        public static bool IsValidName(string? name) => // concise validation method
            !string.IsNullOrWhiteSpace(name) && name.Trim().Length >= MinNameLength; // validation expression
    } // end of InputValidator
} // end of namespace