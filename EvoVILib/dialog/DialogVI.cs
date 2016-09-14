using EvoVI.Plugins;
using System;
using System.Text.RegularExpressions;

namespace EvoVI.Dialog
{
    public class DialogVI : DialogBase
    {
        #region Variables
        private bool _waitUntilFinished;
        private DateTime _speechRegisteredInQueue;
        #endregion


        #region Properties
        /// <summary> Returns the parsed dialog text.
        /// </summary>
        public override string Text
        {
            get { return parseRandomSentence(); }
        }


        /// <summary> Returns the string to display on the GUI.
        /// </summary>
        internal override string GUIDisplayText
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
        }
        #endregion


        #region Functions
        /// <summary> Parses a randomized composition of the text the VI should speak.
        /// </summary>
        private string parseRandomSentence()
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
                        result += choices[rndNr.Next(0, choices.Length)].Trim();
                    }
                    else if (currMatch.Groups["OptChoice"].Success)
                    {
                        string[] choices = (matches[u].Groups["OptChoice"].Value + "|").Split('|');
                        result += choices[rndNr.Next(0, choices.Length)].Trim();
                    }

                    currIndex = matches[u].Index + currMatch.Length;
                }
            }

            result += randBaseSentence.Substring(currIndex);

            result = Regex.Replace(result, @"(^|\w)\s*,", "$1,");
            result = Regex.Replace(result, @"(;|\s|,)\1+", "$1");
            return result;
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
            SpeechEngine.Say(this, !_waitUntilFinished);
            base.Trigger();

            // Next node is being selected via the onSpeechFinished event handler
        }
        #endregion
    }
}
