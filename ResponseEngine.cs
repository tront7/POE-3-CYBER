// Import base system types
using System;
// Import collection interfaces and types used for responses and topic lists
using System.Collections.Generic;

// Root namespace for bot components
namespace CybersecurityBot
{
    // XML documentation: what ResponseEngine provides
    /// <summary>
    /// Maps topic keywords to curated, randomised cybersecurity responses.
    /// Uses a delegate for response selection so the strategy can be swapped at runtime.
    /// </summary>
    // Static class exposing response lookup utilities to the UI
    public static class ResponseEngine
    {
        // Section marker: delegate definitions
        // ── Delegate ──────────────────────────────────────────────────────────

        // XML doc: delegate that chooses one entry from a list of responses
        /// <summary>Selects a single response from a candidate list.</summary>
        public delegate string ResponseSelector(IReadOnlyList<string> responses);

        // XML doc: default selection strategy (random)
        /// <summary>Default selector — returns a uniformly random entry.</summary>
        // Property holding the active response selection strategy
        public static ResponseSelector SelectResponse { get; set; } = list => list[_rng!.Next(list.Count)];

        // RNG instance used by the default selector
        private static readonly Random _rng = new();

        // Section marker: response dictionary mapping keywords to responses
        // ── Response dictionary ───────────────────────────────────────────────
        // Key  : keyword (matched via case-insensitive substring search)
        // Value: list of alternative responses (one is chosen at random)

        // Read-only dictionary of keyword -> list of response strings
        private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Responses =
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            // Conversational starters and small-talk
            // ── Conversational ────────────────────────────────────────────────
            ["how are you"] = new[]
            {
                // Variant 1 for 'how are you'
                "I'm fully operational and vigilant — always watching out for cyber threats! How can I help you today?",
                // Variant 2 for 'how are you'
                "Running at 100%! Ready to help you stay safe online. What would you like to know?",
                // Variant 3 for 'how are you'
                "All systems go! I'm here and ready to boost your cybersecurity knowledge.",
            },
            // Responses for questions about purpose
            ["what is your purpose"] = new[]
            {
                // Purpose variant 1
                "My purpose is to educate you about cybersecurity threats and best practices so you can stay safe online.",
                // Purpose variant 2
                "I'm here to be your personal cybersecurity guide — helping you understand threats and how to avoid them.",
                // Purpose variant 3
                "I exist to make cybersecurity simple and accessible for everyone. Ask me anything!",
            },
            // Greeting responses keyed by 'hello'
            ["hello"] = new[]
            {
                // Hello variant 1
                "Hello! Great to see you here. Ask me anything about staying safe online.",
                // Hello variant 2
                "Hey there! Ready to dive into cybersecurity? What would you like to know?",
                // Hello variant 3
                "Hi! I'm your Cybersecurity Awareness Bot. What can I help you with today?",
            },
            // Short greeting keyed by 'hi'
            ["hi"] = new[]
            {
                // Hi variant 1
                "Hi there! What cybersecurity topic can I help you with?",
                // Hi variant 2
                "Hello! Feel free to ask me anything about online safety.",
                // Hi variant 3
                "Hey! Great to have you here. What would you like to learn about?",
            },
            // Help prompt listing common topics
            ["help"] = new[]
            {
                // Help option 1
                "Sure! Try asking about: passwords, phishing, malware, safe browsing, privacy, 2FA, VPNs, ransomware, or social engineering.",
                // Help option 2
                "I can help with: passwords · phishing · malware · VPNs · ransomware · privacy · 2FA · safe browsing · social engineering. What interests you?",
                // Help option 3
                "Here are some topics to explore: passwords · phishing · malware · safe browsing · privacy · 2FA · VPN · encryption · data breaches. Just ask!",
            },

            // Password guidance group
            // ── Passwords ─────────────────────────────────────────────────────
            ["password"] = new[]
            {
                // Password safety block option 1 (multi-line string)
                "🔐 PASSWORD SAFETY\n" +
                "  • Use at least 12 characters — mix letters, numbers, and symbols.\n" +
                "  • Never reuse passwords across different accounts.\n" +
                "  • Use a password manager such as Bitwarden or 1Password.\n" +
                "  • Avoid birthdays, pet names, or dictionary words.\n" +
                "  • Change passwords immediately if a breach is suspected.",

                // Password strength block option 2
                "🔐 STRONG PASSWORDS\n" +
                "  • A passphrase like 'Coffee!Monkey$River7' is both strong and memorable.\n" +
                "  • A password manager removes the need to memorise dozens of passwords.\n" +
                "  • Use a different password for every account — no exceptions.\n" +
                "  • Never share your password, even with IT support.",

                // Password tips block option 3
                "🔐 PASSWORD TIPS\n" +
                "  • Length beats complexity — 16+ random characters is ideal.\n" +
                "  • Pair strong passwords with multi-factor authentication.\n" +
                "  • Visit haveibeenpwned.com to check if your credentials were leaked.\n" +
                "  • Review and rotate your most sensitive passwords every 6 months.",
            },

            // Phishing guidance group
            // ── Phishing ──────────────────────────────────────────────────────
            ["phishing"] = new[]
            {
                // Phishing awareness option 1
                "🎣 PHISHING AWARENESS\n" +
                "  • Phishing emails impersonate trusted brands to steal your credentials.\n" +
                "  • Check for misspelled domains — e.g. 'amaz0n.com' or 'paypa1.com'.\n" +
                "  • Hover over links before clicking to inspect the real destination URL.\n" +
                "  • Legitimate organisations never ask for passwords via email.\n" +
                "  • When in doubt, contact the sender through their official website.",

                // Phishing spotting tips option 2
                "🎣 SPOTTING A PHISHING ATTACK\n" +
                "  • Be suspicious of urgent language like 'Act now or lose access!'.\n" +
                "  • Carefully check the sender's email address — scammers spoof real domains.\n" +
                "  • Never enter login details after clicking an email link — visit the site directly.\n" +
                "  • Phishing also arrives via SMS (smishing) and phone calls (vishing).",

                // Phishing protection option 3
                "🎣 PHISHING PROTECTION\n" +
                "  • Enable spam and phishing filters in your email client.\n" +
                "  • Chrome, Firefox, and Edge all include built-in phishing protection.\n" +
                "  • Report phishing emails to your provider — it helps protect others too.\n" +
                "  • Practice spotting attacks at phishingquiz.withgoogle.com.",
            },

            // Scam awareness group
            // ── Scams ─────────────────────────────────────────────────────────
            ["scam"] = new[]
            {
                // Scam awareness option 1
                "🎣 SCAM AWARENESS\n" +
                "  • If it sounds too good to be true, it almost certainly is.\n" +
                "  • Never send money or gift cards to someone you only know online.\n" +
                "  • Romance, tech-support, and lottery scams are all extremely common.\n" +
                "  • Verify any urgent request by calling the organisation directly.",

                // Avoiding scams option 2
                "🎣 AVOIDING SCAMS\n" +
                "  • Scammers manufacture urgency — slow down and verify before acting.\n" +
                "  • Never grant remote access to your device to an unsolicited caller.\n" +
                "  • Check reviews and domain age before making purchases from new websites.\n" +
                "  • Use a credit card (not debit) for online purchases for added fraud protection.",
            },

            // Safe browsing group
            // ── Safe browsing ─────────────────────────────────────────────────
            ["browsing"] = new[]
            {
                // Safe browsing option 1
                "🌐 SAFE BROWSING\n" +
                "  • Always look for HTTPS (the padlock icon) before entering personal data.\n" +
                "  • Use extensions like uBlock Origin to block malicious ads and trackers.\n" +
                "  • Avoid downloading files from untrusted websites.\n" +
                "  • Keep your browser and all extensions fully updated.\n" +
                "  • Use a VPN on public Wi-Fi.",

                // Browse safely option 2
                "🌐 BROWSE SAFELY\n" +
                "  • Use private/incognito mode on shared or public computers.\n" +
                "  • Clear cookies and browsing history regularly.\n" +
                "  • Pop-ups claiming your device is infected are almost always scams.\n" +
                "  • Verify URLs carefully before clicking — especially in search results.",

                // Browser security option 3
                "🌐 BROWSER SECURITY\n" +
                "  • Disable browser extensions you no longer use — they can track you.\n" +
                "  • Consider a privacy-focused browser like Firefox or Brave.\n" +
                "  • Enable DNS-over-HTTPS in your browser settings for extra protection.\n" +
                "  • Enable 2FA on any account linked to your browser profile.",
            },

            // Internet safety group
            ["internet safety"] = new[]
            {
                // Internet safety option 1
                "🌐 INTERNET SAFETY\n" +
                "  • Think before you click — most attacks start with a single careless click.\n" +
                "  • Use strong, unique passwords on every account.\n" +
                "  • Enable 2FA wherever possible.\n" +
                "  • Keep all software and apps updated.\n" +
                "  • Back up your data regularly to multiple locations.",

                // Staying safe online option 2
                "🌐 STAYING SAFE ONLINE\n" +
                "  • Limit the personal information you share on social media.\n" +
                "  • Use end-to-end encrypted messaging apps like Signal for sensitive chats.\n" +
                "  • Avoid public Wi-Fi for banking or sensitive transactions.\n" +
                "  • Review your privacy settings on all social platforms regularly.",
            },

            // Malware group
            // ── Malware ───────────────────────────────────────────────────────
            ["malware"] = new[]
            {
                // Malware protection option 1
                "🦠 MALWARE PROTECTION\n" +
                "  • Install reputable antivirus software and keep its definitions updated.\n" +
                "  • Never open attachments from unknown or unexpected senders.\n" +
                "  • Avoid pirated software — it is a leading vector for malware.\n" +
                "  • Back up your data regularly to an external drive and the cloud.\n" +
                "  • Treat USB drives from unknown sources as potentially dangerous.",

                // Avoiding malware option 2
                "🦠 AVOIDING MALWARE\n" +
                "  • Download apps only from official stores (Google Play, Apple App Store).\n" +
                "  • Keep your operating system updated — patches close known vulnerabilities.\n" +
                "  • Run antivirus scans regularly, even if you believe you're safe.\n" +
                "  • Malware can arrive through ads, email attachments, and compromised websites.",
            },

            // Ransomware group
            // ── Ransomware ────────────────────────────────────────────────────
            ["ransomware"] = new[]
            {
                // Ransomware protection option 1
                "💰 RANSOMWARE PROTECTION\n" +
                "  • Maintain regular, tested backups stored offline or in the cloud.\n" +
                "  • Never pay the ransom — it doesn't guarantee your data is returned.\n" +
                "  • Keep your OS and software patched to close known vulnerabilities.\n" +
                "  • Restrict user permissions — run day-to-day tasks without admin rights.\n" +
                "  • Disable macros in Office documents received by email.",

                // Understanding ransomware option 2
                "💰 UNDERSTANDING RANSOMWARE\n" +
                "  • Ransomware encrypts your files and demands payment for the key.\n" +
                "  • Most ransomware spreads via phishing emails or unpatched vulnerabilities.\n" +
                "  • The 3-2-1 backup rule: 3 copies, 2 media types, 1 off-site location.\n" +
                "  • Contact a cybersecurity professional before attempting recovery.",
            },

            // Privacy group
            // ── Privacy ───────────────────────────────────────────────────────
            ["privacy"] = new[]
            {
                // Protecting your privacy option 1
                "🔏 PROTECTING YOUR PRIVACY\n" +
                "  • Read privacy policies before accepting — especially for free apps.\n" +
                "  • Limit app permissions to only what is strictly necessary.\n" +
                "  • Regularly audit which third-party apps have access to your accounts.\n" +
                "  • Use a VPN to mask your IP address and browsing activity.\n" +
                "  • Opt out of data-sharing schemes wherever the option is available.",

                // Digital privacy tips option 2
                "🔏 DIGITAL PRIVACY TIPS\n" +
                "  • Use encrypted, privacy-respecting alternatives to mainstream services.\n" +
                "  • Disable ad personalisation in Google, Facebook, and Apple settings.\n" +
                "  • Review and tighten social media privacy settings — check them quarterly.\n" +
                "  • Consider a private search engine like DuckDuckGo or Brave Search.",
            },

            // Social engineering group
            // ── Social engineering ────────────────────────────────────────────
            ["social engineering"] = new[]
            {
                // Social engineering explanation option 1
                "🎭 SOCIAL ENGINEERING\n" +
                "  • Social engineering manipulates people rather than exploiting technology.\n" +
                "  • Attackers may impersonate IT support, colleagues, or authority figures.\n" +
                "  • Always verify identities through a separate, trusted communication channel.\n" +
                "  • Never feel pressured to act immediately — legitimate requests can wait.\n" +
                "  • If something feels off, trust your instincts and verify.",

                // Common tactics option 2
                "🎭 COMMON SOCIAL ENGINEERING TACTICS\n" +
                "  • Pretexting: fabricating a scenario to extract sensitive information.\n" +
                "  • Baiting: leaving infected USB drives in public places.\n" +
                "  • Tailgating: following an authorised person through a secure door.\n" +
                "  • Quid pro quo: offering a service in exchange for credentials.\n" +
                "  • Training and awareness are the best defences against these attacks.",
            },

            // 2FA / MFA group
            // ── 2FA / MFA ─────────────────────────────────────────────────────
            ["2fa"] = new[]
            {
                // Two-factor authentication primer
                "🔑 TWO-FACTOR AUTHENTICATION (2FA)\n" +
                "  • 2FA adds a second verification step beyond your password.\n" +
                "  • Use an authenticator app (Google Authenticator, Authy) over SMS when possible.\n" +
                "  • Enable 2FA on email, banking, and social media accounts first.\n" +
                "  • Store backup codes securely — print them or keep them in a password manager.\n" +
                "  • Even if your password is stolen, 2FA can stop an attacker in their tracks.",

                // MFA best practices
                "🔑 MFA BEST PRACTICES\n" +
                "  • Hardware keys (YubiKey) are the most phishing-resistant form of MFA.\n" +
                "  • SMS 2FA is better than nothing, but is vulnerable to SIM-swapping attacks.\n" +
                "  • Passkeys are the future — adopt them where supported for a passwordless experience.\n" +
                "  • Never share a 2FA code with anyone — no legitimate service will ever ask for it.",
            },

            // MFA synonym entry
            ["mfa"] = new[]
            {
                // Multi-factor authentication explanation
                "🔑 MULTI-FACTOR AUTHENTICATION\n" +
                "  • MFA is one of the single most effective security controls you can enable.\n" +
                "  • It combines something you know (password) with something you have (phone/key).\n" +
                "  • Enable it on every account that supports it, prioritising email and banking.\n" +
                "  • Authenticator apps are more secure than SMS codes — use them where possible.",
            },

            // VPN group
            // ── VPN ───────────────────────────────────────────────────────────
            ["vpn"] = new[]
            {
                // VPN essentials option 1
                "🔒 VPN ESSENTIALS\n" +
                "  • A VPN encrypts your traffic and masks your IP address from third parties.\n" +
                "  • Always use a VPN on public or untrusted Wi-Fi networks.\n" +
                "• A VPN does not make you anonymous — it reduces exposure, not risk to zero.\n" +
                "  • Choose a reputable provider with a strict no-logs policy (Mullvad, ProtonVPN).\n" + 
                "  • Free VPNs often monetise your data — invest in a paid service.",

                // VPN best practices option 2
                "🔒 VPN BEST PRACTICES\n" +
                "  • Enable the kill switch feature — it cuts your internet if the VPN drops.\n" +
                "  • Check for DNS leaks at dnsleaktest.com after connecting.\n" +
                "  • A VPN is not a substitute for antivirus or strong passwords.\n" +
                "  • Use split-tunnelling to route only sensitive traffic through the VPN.",
            },

            // Wi-Fi group
            // ── Wi-Fi ─────────────────────────────────────────────────────────
            ["wi-fi"] = new[]
            {
                // Wi-Fi safety option 1
                "📡 WI-FI SAFETY\n" +
                "  • Use WPA3 encryption on your home network if your router supports it.\n" +
                "  • Change your router's default admin credentials immediately.\n" +
                "  • Disable WPS — it has well-documented vulnerabilities.\n" +
                "  • Create a separate guest network for smart-home and IoT devices.",

                // Home network tips option 2
                "📡 HOME NETWORK TIPS\n" +
                "  • Regularly review which devices are connected to your network.\n" +
                "  • Keep your router firmware updated — manufacturers release security patches.\n" +
                "  • Position your router centrally to minimise signal leakage outside your home.",
            },

            // Public Wi-Fi group
            ["public wifi"] = new[]
            {
                // Public Wi-Fi dangers option 1
                "📡 PUBLIC WI-FI DANGERS\n" +
                "  • Treat all public Wi-Fi as compromised — always use a VPN.\n" +
                "  • Never access banking or enter passwords on a public network.\n" +
                "  • Verify the network name with staff — fake hotspots are common.\n" +
                "  • Use your mobile data instead whenever possible.",

                // Man-in-the-middle explanation option 2
                "📡 MAN-IN-THE-MIDDLE ATTACKS\n" +
                "  • Attackers on public Wi-Fi can intercept unencrypted traffic.\n" +
                "  • Even HTTPS can be stripped by sophisticated attackers on open networks.\n" +
                "  • Disable auto-connect to open Wi-Fi networks in your device settings.",
            },

            // Encryption group
            // ── Encryption ────────────────────────────────────────────────────
            ["encrypt"] = new[]
            {
                // Encryption basics option 1
                "🔓 ENCRYPTION BASICS\n" +
                "  • Encryption scrambles your data so only authorised parties can read it.\n" +
                "  • Enable full-disk encryption: BitLocker (Windows) or FileVault (Mac).\n" +
                "  • Use end-to-end encrypted apps like Signal for sensitive conversations.\n" +
                "  • Always check for HTTPS before submitting personal data on any website.",

                // Why encryption matters option 2
                "🔓 WHY ENCRYPTION MATTERS\n" +
                "  • If your device is stolen, full-disk encryption protects all your data.\n" +
                "  • End-to-end encryption means not even the app provider can read your messages.\n" +
                "  • Encryption underpins all secure online communication — from banking to email.",
            },

            // Data breach group
            // ── Data breach ───────────────────────────────────────────────────
            ["data breach"] = new[]
            {
                // Data breach response option 1
                "🚨 DATA BREACH RESPONSE\n" +
                "  • Check haveibeenpwned.com to see if your email was exposed.\n" +
                "  • Change passwords for all affected accounts immediately.\n" +
                "  • Enable 2FA on every account as an additional layer of protection.\n" +
                "  • Monitor your bank statements for suspicious activity.\n" +
                "  • Be extra vigilant for phishing emails in the weeks following a breach.",

                // Breach prevention option 2
                "🚨 BREACH PREVENTION\n" +
                "  • Unique passwords per site ensure one breach doesn't cascade to others.\n" +
                "  • Enable breach-alert notifications in your password manager.\n" +
                "  • Consider a credit freeze if financial data was part of the breach.",
            },

            // Breach short-keyword mapping
            ["breach"] = new[]
            {
                // If you've been breached guidance
                "🚨 IF YOU'VE BEEN BREACHED\n" +
                "  • Act fast — change passwords and enable 2FA immediately.\n" +
                "  • Visit haveibeenpwned.com to understand the full scope of the exposure.\n" +
                "  • Notify your bank if any financial data was involved.\n" +
                "  • Monitor your accounts closely for suspicious activity over the next few months.",
            },

            // Hacking group
            // ── Hacking ───────────────────────────────────────────────────────
            ["hack"] = new[]
            {
                // If you think you've been hacked option 1
                "🔴 IF YOU THINK YOU'VE BEEN HACKED\n" +
                "  • Change all important passwords immediately from a clean, safe device.\n" +
                "  • Enable 2FA on every account without delay.\n" +
                "  • Review active sessions and revoke access for any unrecognised devices.\n" +
                "  • Contact your bank immediately if financial accounts may be affected.\n" +
                "  • Run a full antivirus and anti-malware scan.",

                // Preventing account compromise option 2
                "🔴 PREVENTING ACCOUNT COMPROMISE\n" +
                "  • Attackers often use credentials stolen in previous data breaches.\n" +
                "  • A password manager ensures every account has a unique, strong password.\n" +
                "  • Most successful hacks exploit known, patchable vulnerabilities — keep software updated.\n" +
                "  • Be suspicious of any unsolicited contact requesting information or access.",
            },

            // Updates group
            // ── Updates ───────────────────────────────────────────────────────
            ["update"] = new[]
            {
                // Software updates guidance option 1
                "🔄 SOFTWARE UPDATES\n" +
                "  • Updates patch security vulnerabilities — apply them promptly.\n" +
                "  • Enable automatic updates for your OS, applications, and browser.\n" +
                "  • Apply critical security patches the same day they are released.\n" +
                "  • Don't forget router firmware, smart TVs, and IoT devices — they need updates too.",

                // Why updates matter option 2
                "🔄 WHY UPDATES MATTER\n" +
                "  • The majority of successful attacks exploit known, unpatched vulnerabilities.\n" +
                "  • WannaCry infected 200,000 machines across 150 countries via an unpatched Windows flaw.\n" +
                "  • Enabling auto-updates is one of the simplest and most impactful security steps you can take.",
            },

            // Firewall group
            // ── Firewall ──────────────────────────────────────────────────────
            ["firewall"] = new[]
            {
                // Firewall basics option 1
                "🛡 FIREWALL BASICS\n" +
                "  • A firewall monitors and controls incoming and outgoing network traffic.\n" +
                "  • Windows Defender Firewall and macOS Firewall are both enabled by default — keep them on.\n" +
                "  • A hardware firewall at the router level adds a second layer of protection.\n" +
                "  • Firewalls are your first line of defence against network-based attacks.",

                // Firewall tips option 2
                "🛡 FIREWALL TIPS\n" +
                "  • Never disable your firewall for an application unless you fully understand the risk.\n" +
                "  • Review firewall rules periodically and remove outdated exceptions.\n" +
                "  • Enterprise networks should use next-generation firewalls (NGFW) with deep packet inspection.",
            },

            // Identity theft group
            // ── Identity theft ────────────────────────────────────────────────
            ["identity theft"] = new[]
            {
                // Identity theft protection option 1
                "🪪 IDENTITY THEFT PROTECTION\n" +
                "  • Never share your ID number, bank details, or passwords online or by phone.\n" +
                "  • Shred all documents containing personal information before disposal.\n" +
                "  • Place a fraud alert with credit bureaus if you suspect your identity has been stolen.\n" +
                "  • Review your credit report regularly for accounts or enquiries you don't recognise.",

                // If your identity is stolen option 2
                "🪪 IF YOUR IDENTITY IS STOLEN\n" +
                "  • Report it to your bank and credit bureaus immediately.\n" +
                "  • File a police report — you will need this for insurance and dispute resolution.\n" +
                "  • Request a credit freeze to prevent new accounts being opened in your name.\n" +
                "  • Change passwords and enable 2FA on all accounts as a precaution.",
            },

            // Spam group
            // ── Spam ──────────────────────────────────────────────────────────
            ["spam"] = new[]
            {
                // Dealing with spam option 1
                "📧 DEALING WITH SPAM\n" +
                "  • Never click links or open attachments in unsolicited emails.\n" +
                "  • Mark messages as junk to train your email client's spam filter.\n" +
                "  • Don't unsubscribe from obvious spam — it confirms your address is active.\n" +
                "  • Use a dedicated email address for online sign-ups and marketing.",

                // Reducing spam option 2
                "📧 REDUCING SPAM\n" +
                "  • Use email aliases (SimpleLogin, AnonAddy) to protect your real address.\n" +
                "  • Never publicly post your email address on websites or forums.\n" +
                "  • Enable your provider's advanced spam filter if available.\n" +
                "  • Report phishing spam to your local cyber authority or Action Fraud.",
            },
        };

        // Section: list of topic labels used to populate UI chips
        // ── Topic list for UI chips ───────────────────────────────────────────

        // XML doc: labels shown on the topic buttons in the left panel
        /// <summary>Display labels used to populate topic-chip buttons in the UI.</summary>
        public static readonly IReadOnlyList<string> TopicList = new[]
        {
            // A compact list of topic display strings (emoji + label)
            "🔐 passwords",       "🎣 phishing",        "🌐 safe browsing",
            "🦠 malware",         "🔏 privacy",          "🎭 social engineering",
            "🔑 2FA",             "🔒 VPN",              "💰 ransomware",
            "📡 WiFi security",   "🔓 encryption",       "🚨 data breach",
            "🔴 hacking",         "🔄 software updates", "🛡 firewall",
            "🪪 identity theft",  "📧 spam",             "🎣 scams",
        };

        // Section: public API methods for response lookup
        // ── Public API ───────────────────────────────────────────────────────-

        // XML doc: GetResponse searches the input for a matching keyword and returns a chosen response
        /// <summary>
        /// Searches <paramref name="input"/> for a known keyword and returns
        /// a randomly selected response string, or <c>null</c> if nothing matches.
        /// </summary>
        public static string? GetResponse(string input)
        {
            // Return null for empty/whitespace input
            if (string.IsNullOrWhiteSpace(input)) return null;

            // Iterate over defined responses and return the first matching selection
            foreach (var (key, responses) in Responses)
                if (input.Contains(key, StringComparison.OrdinalIgnoreCase))
                    return SelectResponse(responses);

            // No match found
            return null;
        }

        // XML doc: GetMatchedTopicKey returns the keyword that matched the input
        /// <summary>
        /// Returns the first matching keyword found in <paramref name="input"/>,
        /// or <c>null</c> when no keyword is recognised.
        /// Used to populate <see cref="ConversationContext.LastTopic"/>.
        /// </summary>
        public static string? GetMatchedTopicKey(string input)
        {
            // Guard for empty input
            if (string.IsNullOrWhiteSpace(input)) return null;

            // Return the first key that appears as a substring in the input
            foreach (var key in Responses.Keys)
                if (input.Contains(key, StringComparison.OrdinalIgnoreCase))
                    return key;

            // No keyword matched
            return null;
        }
    }
}