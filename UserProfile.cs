using System; // Import the System namespace for fundamental types like DateTime and string operations

namespace CybersecurityBot // Define a namespace called CybersecurityBot to organize the code
{ // Start the namespace block

    public sealed class UserProfile // Declare a sealed class (cannot be inherited) named UserProfile
    { // Start the UserProfile class body

        public string   Name         { get; } // Define a public read-only auto-property Name of type string (initialized in constructor)
        public DateTime SessionStart { get; } // Define a public read-only auto-property SessionStart of type DateTime (defaults to current time? Actually it's not set; must be set by constructor. But note: constructor only sets Name, not SessionStart. SessionStart will be default(DateTime) which is 1/1/0001. This might be a bug, but we comment as-is.)

        public string FormattedName => // Define a computed property that returns a formatted version of the user's name
            string.IsNullOrWhiteSpace(Name) // Check if Name is null, empty, or whitespace
                ? "User" // If true, return a generic "User" string
                : char.ToUpperInvariant(Name[0]) + Name[1..].ToLowerInvariant(); // Otherwise, capitalize first letter and lowercase the rest

        public string TimeGreeting => SessionStart.Hour switch // Define computed property for a time-based greeting using the hour from SessionStart
        { // Start switch expression block
            < 12 => "Good morning", // If hour is before 12 PM, return "Good morning"
            < 17 => "Good afternoon", // If hour is less than 5 PM (but >= 12), return "Good afternoon"
            _    => "Good evening", // For all other hours (>= 5 PM), return "Good evening"
        }; // End switch expression and property definition

        public string SessionDuration // Define a computed property that returns a human-readable session duration string
        { // Start property accessor block
            get // Define the get accessor for the property
            { // Start get accessor body
                var e = DateTime.Now - SessionStart; // Calculate the time difference between now and session start
                if (e.TotalSeconds < 60)  return "less than a minute"; // If less than 60 seconds, return a string indicating less than a minute
                if (e.TotalMinutes < 60)  return $"{(int)e.TotalMinutes} minute(s)"; // If less than 60 minutes, return the whole minutes
                return $"{(int)e.TotalHours} hour(s) and {e.Minutes} minute(s)"; // Otherwise, return hours and remaining minutes
            } // End get accessor body
        } // End property accessor block

        public UserProfile(string name) // Constructor that takes a name parameter
        { // Start constructor body
            if (string.IsNullOrWhiteSpace(name)) // Check if the provided name is null, empty, or whitespace
                throw new ArgumentException("Name cannot be empty.", nameof(name)); // If invalid, throw an exception with the parameter name
            Name = name.Trim(); // Otherwise, trim whitespace and assign to the read-only Name property
        } // End constructor body

    } // End UserProfile class body

} // End CybersecurityBot namespace block
