using EvoVI.Engine;
using EvoVI.PluginContracts;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace EvoVI.Classes.Dialog
{
    public class DialogPlayer : DialogBase
    {
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


        /// <summary> Returns whether a node is ready and can be triggered.
        /// </summary>
        public override bool IsReady
        {
            get { return anyGrammarActive; }
        }


        /// <summary> Returns or sets whether the dialog node is disabled or not.
        /// </summary>
        public override bool Disabled
        {
            get { return _disabled; }
            set { _disabled = value; UpdateState(); }
        }
        #endregion


        #region Events
        /// <summary> Fires each time an answer within this dialog node has been recognized.
        /// Fires the bound command, if available.
        /// </summary>
        /// <param name="sender">The event sender object.</param>
        /// <param name="e">The speech recognized event arguments.</param>
        private void onDialogDone(object sender, SpeechRecognizedEventArgs e)
        {
            SetActive();
            Trigger();
            NextNode();
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
            _grammarList.Clear();
            _grammarStatusList.Clear();

            if (VALIDATION_REGEX.Match(_text).Success) { return; }

            string[] sentences = _text.Split(';');

            for (int i = 0; i < sentences.Length; i++)
            {
                GrammarBuilder builder = new GrammarBuilder();
                MatchCollection matches = CHOICES_REGEX.Matches(sentences[i]);

                int currIndex = 0;

                if (matches.Count > 0)
                {
                    for (int u = 0; u < matches.Count; u++)
                    {
                        Match currMatch = matches[u];

                        // Append "fixed" text
                        string leadingText = sentences[i].Substring(currIndex, matches[u].Index - currIndex);

                        // Commas, dots and semi-colons make recognition harder
                        leadingText = leadingText.Replace(".", "");
                        leadingText = leadingText.Replace(",", "");
                        leadingText = leadingText.Replace(";", "");
                        if (!VALIDATION_REGEX.Match(leadingText).Success) { builder.Append(leadingText); }

                        // Append choices
                        if (currMatch.Groups["Choice"].Success)
                        {
                            builder.Append(new Choices(matches[u].Groups["Choice"].Value.Split('|')));

                        }
                        else if (currMatch.Groups["OptChoice"].Success)
                        {
                            Choices choices = new Choices(matches[u].Groups["OptChoice"].Value.Split('|'));
                            choices.Add(" ");
                            builder.Append(choices);
                        }

                        currIndex = matches[u].Index + currMatch.Length;
                    }
                }

                string trailingText = sentences[i].Substring(currIndex);
                if (!VALIDATION_REGEX.Match(trailingText).Success) { builder.Append(trailingText); }

                Grammar resultGrammar = new Grammar(builder);
                resultGrammar.SpeechRecognized += onDialogDone;
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
        /// <param name="pImportance">The importance this node has over others.</param>
        /// <param name="pPluginToStart">The name of the plugin to start, when triggered.</param>
        /// <param name="pData">An object containing custom, user-defined data.</param>
        public DialogPlayer(string pText = " ", DialogImportance pImportance = DialogImportance.NORMAL, string pPluginToStart = null, object pData = null) :
            base(pText, pImportance, pPluginToStart, pData)
        {
            this._speaker = DialogSpeaker.PLAYER;

            this._grammarList = new List<Grammar>();
            this._grammarStatusList = new List<GrammarStatus>();

            parseAnswers();
        }
        #endregion


        #region Override Functions
        /// <summary> Updates the dialog node's status.
        /// </summary>
        public override void UpdateState()
        {
            if (
                (_importance < DialogImportance.CRITICAL) && 
                (_disabled || !this.IsNextInTurn)
            )
            {
                this.sleep();
            }
            else
            {
                this.wakeUp();
            }
        }
        #endregion
    }
}
