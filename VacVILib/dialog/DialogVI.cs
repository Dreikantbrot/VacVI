using VacVI.Plugins;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;

namespace VacVI.Dialog
{
    /// <summary> A dialog node that is spoken by the VI.</summary>
    public class DialogVI : DialogBase
    {
        #region Regexes (readonly)
        /// <summary> Regex for filtering out word writings and pronunciations.</summary>
        private static readonly Regex WORD_PRONUNCIATION_REGEX = new Regex(@"<\s*(?<RawText>.+?)\s*-->\s*(?<SpokenText>.+?)\s*>");

        /// <summary> Regex for recognizing lower case letters, followed by a punctuation mark or ":".</summary>
        private static readonly Regex UPPERCASE_LETTER_SENTENCE_REGEX = new Regex(@"(?:^|[.:!?])\s*([a-z])");
        #endregion


        #region Variables
        private bool _waitUntilFinished;
        private DateTime _speechRegisteredInQueue;
        private Action<SpeechEngine.VISpeechStoppedEventArgs> _waitUntilFinishedHandler;
        #endregion


        #region Properties
        /// <summary> Returns the parsed dialog text (how it is spoken).
        /// </summary>
        public override string Text
        {
            get { return ResolvePronunciationSyntax(ParseRandomSentence(), true); }
        }


        /// <summary> Returns the parsed dialog text (how it is displayed).
        /// </summary>
        public override string DisplayedText
        {
            get { return ResolvePronunciationSyntax(ParseRandomSentence(), false); }
        }


        /// <summary> Returns the string to display on the GUI.
        /// </summary>
        internal override string DebugDisplayText
        {
            get { return "<...>"; }
        }


        /// <summary> Returns or sets whether the text should be spoken asynchronously or not.
        /// </summary>
        public bool WaitUntilFinished
        {
            get { return _waitUntilFinished; }
            set { _waitUntilFinished = value; }
        }


        /// <summary> Returns or sets the time when this node has been reigstered within SpeechEngine's queue.
        /// </summary>
        internal DateTime SpeechRegisteredInQueue
        {
            get { return _speechRegisteredInQueue; }
            set { _speechRegisteredInQueue = value; }
        }
        #endregion


        #region Constructor
        /// <summary> Creates a dalog node where the VI says a randomly generated arrangement of the specified text.
        /// <para>This node is triggered automatically, as soon as it is active.</para>
        /// </summary>
        /// <param name="pText">The text for the VI to say (see dialog text syntax).</param>
        /// <param name="pPriority">The node's priority.</param>
        /// <param name="pConditionFunction">The delegate function that checks for the fullfillment of the dialog node's condition.</param>
        /// <param name="pPluginToStart">The name of the plugin to start, when triggered.</param>
        /// <param name="pData">An object containing custom, user-defined data.</param>
        /// <param name="pFlags">The behaviour-flags, modifying the node's behaviour.</param>
        /// <param name="pWaituntilFinished">If set to true, the plugin will start after the text has been spoken, else it will be run asynchonously.</param>
        public DialogVI(
            string pText = " ",
            DialogPriority pPriority = DialogPriority.NORMAL,
            System.Func<bool> pConditionFunction = null,
            string pPluginToStart = null,
            object pData = null,
            DialogFlags pFlags = DialogFlags.NONE,
            bool pWaituntilFinished = false
        ) :
            base(pText, pPriority, pConditionFunction, pPluginToStart, pData, pFlags)
        {
            this._speaker = DialogSpeaker.VI;
            this._waitUntilFinished = pWaituntilFinished;
            this._waitUntilFinishedHandler = delegate(SpeechEngine.VISpeechStoppedEventArgs e)
            {
                SpeechEngine.OnVISpeechStopped -= this._waitUntilFinishedHandler;
                base.Trigger();
            };
        }
        #endregion


        #region Functions
        /// <summary> Parses a randomized composition of the text the VI should speak.
        /// </summary>
        public string ParseRandomSentence()
        {
            Random rndNr = new Random();
            string result = "";
            string[] sentences = _text.Split(';');
            string randBaseSentence = sentences[rndNr.Next(0, sentences.Length)];

            MatchCollection matches = CHOICES_REGEX.Matches(randBaseSentence);

            int currIndex = 0;

            if (matches.Count > 0)
            {
                for (int u = 0; u < matches.Count; u++)
                {
                    Match currMatch = matches[u];

                    // Append "fixed" text
                    string leadingText = randBaseSentence.Substring(currIndex, matches[u].Index - currIndex);

                    // Commas and semi-colons make recognition harder
                    result += leadingText;

                    // Append choices
                    if (currMatch.Groups["Choice"].Success)
                    {
                        string[] choices = matches[u].Groups["Choice"].Value.Split('|');
                        result += choices[rndNr.Next(0, choices.Length)];
                    }
                    else if (currMatch.Groups["OptChoice"].Success)
                    {
                        string[] choices = (matches[u].Groups["OptChoice"].Value + "|").Split('|');
                        result += choices[rndNr.Next(0, choices.Length)];
                    }

                    currIndex = matches[u].Index + currMatch.Length;
                }
            }

            result += randBaseSentence.Substring(currIndex);

            /* Beautify the resulting phrase */
            result = result.Trim();
            if (!String.IsNullOrEmpty(result))
            {
                result = Regex.Replace(result, @"(^|\w)\s*,", "$1,");   // trim spaces before commas
                result = Regex.Replace(result, @"(;|\s|,)\1+", "$1");   // remove excessive semi-colons, commas and whitespaces
                if (!Regex.IsMatch(result, @"[!.?]\s*$")) { result += "."; }    // Add punctuation mark
                result = UPPERCASE_LETTER_SENTENCE_REGEX.Replace(result, match => match.ToString().ToUpper());    // ToUpper 1st letter after a punctuation mark
            }
            return result;
        }


        /// <summary> Takes a syntax string and parses word pronunciations to the desired version.
        /// </summary>
        /// <param name="text">The syntax string.</param>
        /// <param name="parseToSpokenText">If true, parses the spoken text, else the displayed text.</param>
        /// <returns>The parsed text.</returns>
        public string ResolvePronunciationSyntax(string text, bool parseToSpokenText)
        {
            MatchCollection pronunciationMatches = WORD_PRONUNCIATION_REGEX.Matches(text);
            for (int u = 0; u < pronunciationMatches.Count; u++)
            {
                text = text.Replace(
                    pronunciationMatches[u].Value,
                    pronunciationMatches[u].Groups[parseToSpokenText ? "SpokenText" : "RawText"].Value
                );
            }

            return text;
        }
        #endregion


        #region Override Functions
        /// <summary> Sets this dialog node as the currently active one.
        /// </summary>
        public override void SetActive()
        {
            base.SetActive();
            
            // Auto-trigger if active
            if (IsActive) { Trigger(); }
        }


        /// <summary> Triggers the dialog node activating it's functions.
        /// </summary>
        public override void Trigger()
        {
            if (_waitUntilFinished)
            {
                // Subscribe to the OnSpeechFinished event handler and trigger from there
                SpeechEngine.OnVISpeechStopped += this._waitUntilFinishedHandler;
                new Thread(() => { SpeechEngine.Say(this, false); }).Start();
                return;
            }
            else
            {
                SpeechEngine.Say(this, true);
            }
            base.Trigger();

            // Next node selection will be called by the SpeechEngine automatically
        }
        #endregion
    }
}
