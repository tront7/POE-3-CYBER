using System; // Import the System namespace for basic types and functionality
using System.IO; // Import System.IO for file path operations (Path class)
using System.Media; // Import System.Media for SoundPlayer class to play audio files
using System.Runtime.Versioning; // Import System.Runtime.Versioning for platform compatibility attributes

namespace CybersecurityBot // Define a namespace called CybersecurityBot to organize the code
{ // Start the namespace block

    [SupportedOSPlatform("windows")] // Attribute indicating this class is only supported on Windows OS
    public static class VoiceGreeting // Declare a public static class named VoiceGreeting (cannot be instantiated)
    { // Start the VoiceGreeting class body

        private const string FileName = "greeting.wav"; // Define a private constant string for the audio file name

        public static void Play() // Define a public static method named Play that returns void
        { // Start the Play method body

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName); // Combine the application's base directory with the file name to get the full file path

            if (!File.Exists(path)) // Check if the file does NOT exist at the calculated path
                return;  // Silently skip – no console dependency // If file doesn't exist, exit the method without doing anything (no error message)

            try // Start a try block to handle potential exceptions when playing the sound
            { // Start the try block body

                using var p = new SoundPlayer(path); // Declare a SoundPlayer instance with the file path, wrapped in a using statement for automatic disposal
                p.PlaySync(); // Play the audio synchronously (blocks until playback finishes)

            } // End the try block body
            catch (Exception) // Catch any exception that occurs during sound playback
            { // Start the catch block body
                // Ignore errors – we're in a WPF app, no console to write to // Intentionally do nothing; suppress errors silently
            } // End the catch block body

        } // End the Play method body

    } // End the VoiceGreeting class body

} // End the CybersecurityBot namespace block