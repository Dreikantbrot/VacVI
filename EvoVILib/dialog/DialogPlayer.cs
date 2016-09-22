using EvoVI.Plugins;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace EvoVI.Dialog
{
    /// <summary> The type of dialog node that is spoken by the player.</summary>
    public class DialogPlayer : DialogBase
    {
        #region Regexes (readonly)
        /// <summary> Regex used to identify characters that make speech recognition harder.</summary>
        private static readonly Regex CHARACTER_CROP_REGEX = new Regex(@"[.,;!?]");
        #endregion


        #region Private Structs
        /// <summary> Contains information about a grammar rule's original enabled state, so it can be reset after being enabled or disabled.
        /// </summary>
        private struct GrammarStatus
        {
            #region Variables
            Grammar _grammar;
            bool _originalEnabledStatus;
            #endregion


            #region Properties
            /// <summary> Returns or the grammar object's original status.
            /// </summary>
            public bool OriginalEnabledStatus
            {
                get { return _originalEnabledStatus; }
            }
            #endregion


            #region Constructor
            /// <summary> Creates a grammar status object containing the grammar's original enabled state.
            /// </summary>
            /// <param name="pGrammar">The instance of the grammar object.</param>
            public GrammarStatus(Grammar pGrammar)
            {
                this._grammar = pGrammar;
                this._originalEnabledStatus = this._grammar.Enabled;

                Save();
            }
            #endregion


            #region Public Functions
            /// <summary> Resets the grammar's enabled status to the previously saved one.
            /// </summary>
            public void Restore()
            {
                _grammar.Enabled = _originalEnabledStatus;
            }


            /// <summary> Sets the grammar's current status (without saving).
            /// </summary>
            public void Set(bool enableStatus)
            {
                _grammar.Enabled = enableStatus;
            }


            /// <summary> Saves the grammar's current status.
            /// </summary>
            public void Save()
            {
                _originalEnabledStatus = _grammar.Enabled;
            }
            #endregion
        }
        #endregion


        #region Variables
        private List<Grammar> _grammarList;
        private List<GrammarStatus> _grammarStatusList;
        #endregion


        #region Properties
        /// <summary> Returns whether at least one speech recognition grammar rule is active.
        /// </summary>
        private bool anyGrammarActive
        {
            get
            {
                for (int i = 0; i < _grammarList.Count; i++) { if (_grammarList[i].Enabled) return true; }
                return false;
            }
        }


        /// <summary> Returns whether at all speech recognition grammar rules are active.
        /// </summary>
        private bool allGrammarsActive
        {
            get
            {
                for (int i = 0; i < _grammarList.Count; i++) { if (!_grammarList[i].Enabled) return false; }
                return true;
            }
        }


        /// <summary> Returns the node's list of grammar instances ("sentences").
        /// </summary>
        public List<Grammar> GrammarList
        {
            get { return _grammarList; }
        }


        /// <summary> Returns or sets whether the dialog node is disabled or not.
        /// </summary>
        public override bool Disabled
        {
            get { return _disabled || !anyGrammarActive; }
            set { _disabled = value; UpdateState(); }
        }


        /// <summary> Returns or sets the unparsed, raw dialog text.
        /// </summary>
        public override string RawText
        {
            get { return _text; }
            set { _text = value; parseAnswers(); }
        }
        #endregion


        #region Functions
        /// <summary> Deactivates all voice recognition rules, saving their current status in advance.
        /// </summary>
        private void sleep()
        {
            for (int i = 0; i < _grammarStatusList.Count; i++)
            {
                _grammarStatusList[i].Save();
                _grammarStatusList[i].Set(false);
            }
        }


        /// <summary> Restores all voice recognition rules to their saved state.
        /// </summary>
        private void wakeUp()
        {
            for (int i = 0; i < _grammarStatusList.Count; i++)
            {
                _grammarStatusList[i].Restore();
            }
        }


        /// <summary> Forceably activates or deactivates all grammar rules.
        /// </summary>
        /// <param name="enable">Whether to enable the rules.</param>
        /// <param name="saveNewStatus">Whether to save the newly set state for each grammar.</param>
        private void setRecognitionStatusAll(bool enable, bool saveNewStatus = false)
        {
            for (int i = 0; i < _grammarStatusList.Count; i++)
            {
                _grammarStatusList[i].Set(enable);

                if (saveNewStatus) { _grammarStatusList[i].Save(); }
            }
        }


        /// <summary> Parses the answer dialog and updates the node's grammar for the speech recognition engine.
        /// </summary>
        private void parseAnswers()
        {
            setRecognitionStatusAll(false, true);
            SpeechEngine.DeregisterPlayerDialogNode(this);

            _grammarList.Clear();
            _grammarStatusList.Clear();

            if (INVALIDATION_REGEX.Match(_text).Success) { return; }

            string[] sentences = _text.Split(';');

            for (int i = 0; i < sentences.Length; i++)
            {
                GrammarBuilder builder = new GrammarBuilder();
                builder.Culture = SpeechEngine.Culture;
                MatchCollection matches = CHOICES_REGEX.Matches(sentences[i]);

                int currIndex = 0;

                if (matches.Count > 0)
                {
                    for (int u = 0; u < matches.Count; u++)
                    {
                        Match currMatch = matches[u];

                        // Append "fixed" text
                        string leadingText = sentences[i].Substring(currIndex, matches[u].Index - currIndex);

                        // Remove characters that make recognition harder
                        leadingText = CHARACTER_CROP_REGEX.Replace(leadingText, "");

                        // Check if the leading text is a valid phrase
                        if (!INVALIDATION_REGEX.Match(leadingText).Success) { builder.Append(leadingText); }

                        // Append choices
                        if (currMatch.Groups["Choice"].Success)
                        {
                            string match = matches[u].Groups["Choice"].Value;

                            // Remove characters that make recognition harder
                            match = CHARACTER_CROP_REGEX.Replace(match, "");
                            builder.Append(new Choices(match.Split('|')));

                        }
                        else if (currMatch.Groups["OptChoice"].Success)
                        {
                            string match = matches[u].Groups["OptChoice"].Value;

                            // Remove characters that make recognition harder
                            match = CHARACTER_CROP_REGEX.Replace(match, "");
                            Choices choices = new Choices(match.Split('|'));
                            choices.Add(" ");
                            builder.Append(choices);
                        }

                        currIndex = matches[u].Index + currMatch.Length;
                    }
                }

                string trailingText = sentences[i].Substring(currIndex);

                // Remove characters that make recognition harder
                trailingText = CHARACTER_CROP_REGEX.Replace(trailingText, "");

                if (!INVALIDATION_REGEX.Match(trailingText).Success) { builder.Append(trailingText); }

                Grammar resultGrammar = new Grammar(builder);
                resultGrammar.Name = this.GetHashCode().ToString();
                _grammarList.Add(resultGrammar);
                _grammarStatusList.Add(new GrammarStatus(resultGrammar));
            }

            SpeechEngine.RegisterPlayerDialogNode(this);
        }
        #endregion


        #region Constructor
        /// <summary> Creates a dialog node used for the player to say the specified text.
        /// <para>This node is only triggered, if a valid phrase has been recognized.</para>
        /// </summary>
        /// <param name="pText">The text to say (see dialog text syntax).</param>
        /// <param name="pPriority">The node's priority.</param>
        /// <param name="pConditionFunction">The delegate function that checks for the fullfillment of the dialog node's condition.</param>
        /// <param name="pPluginToStart">The name of the plugin to start, when triggered.</param>
        /// <param name="pData">An object containing custom, user-defined data.</param>
        /// <param name="pFlags">The behaviour-flags, modifying the node's behaviour.</param>
        public DialogPlayer(
            string pText = " ",
            DialogPriority pPriority = DialogPriority.NORMAL, 
            System.Func<bool> pConditionFunction = null, 
            string pPluginToStart = null,
            object pData = null,
            DialogFlags pFlags = DialogFlags.NONE
        ) :
            base(pText, pPriority, pConditionFunction, pPluginToStart, pData, pFlags)
        {
            this._speaker = DialogSpeaker.PLAYER;

            this._grammarList = new List<Grammar>();
            this._grammarStatusList = new List<GrammarStatus>();

            parseAnswers();
        }
        #endregion


        #region Override Functions
        /// <summary> Sets the dialog node as active and triggers it.
        /// </summary>
        public override void SetActive()
        {
            base.SetActive();

            Trigger();
            NextNode();
        }


        /// <summary> Updates the dialog node's status.
        /// </summary>
        public override void UpdateState()
        {
            if (
                (DialogTreeBuilder.DialogsActive) &&
                (
                    (IsReady) ||
                    ((_flags & DialogFlags.INGORE_READY_STATE) == DialogFlags.INGORE_READY_STATE)
                ) &&
                (!_disabled) &&
                (CheckCondition())
            )
            {
                this.wakeUp();
            }
            else
            {
                this.sleep();
            }
        }
        #endregion
    }
}
