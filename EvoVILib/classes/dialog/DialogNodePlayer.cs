using EvoVI.engine;
using EvoVI.PluginContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EvoVI.classes.dialog
{
    public class DialogNodePlayer : DialogNode
    {
        /// <summary> Contains information about a grammar rule's original enabled state, so it can be reset after being enabled or disabled.
        /// </summary>
        private class GrammarStatus
        {
            #region Variables
            Grammar _grammar;
            bool _originalEnabledStatus;
            #endregion


            #region Properties
            public bool OriginalEnabledStatus
            {
                get { return _originalEnabledStatus; }
                set { _originalEnabledStatus = value; }
            }
            #endregion


            public GrammarStatus(Grammar pGrammar)
            {
                this._grammar = pGrammar;
                this._originalEnabledStatus = this._grammar.Enabled;
            }


            #region Public Functions
            public void ResetStatus()
            {
                this._grammar.Enabled = this._originalEnabledStatus;
            }
            #endregion
        }

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
                for (int i = 0; i < _grammarStatusList.Count; i++) { if (_grammarStatusList[i].OriginalEnabledStatus) return true; }
                return false;
            }
        }


        /// <summary> Returns whether at all speech recognition grammar rules are active.
        /// </summary>
        private bool allGrammarsActive
        {
            get
            {
                for (int i = 0; i < _grammarStatusList.Count; i++) { if (!_grammarStatusList[i].OriginalEnabledStatus) return false; }
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
            get { return _disabled; }
            set { _disabled = value; toggleRecognition(!value); }
        }
        #endregion


        #region Private Functions
        /// <summary> Toggles voice recognition rules on or off.
        /// </summary>
        /// <param name="enable">Whether to enable the rules.</param>
        private void toggleRecognition(bool enable)
        {
            for (int i = 0; i < _grammarStatusList.Count; i++) { this._grammarStatusList[i].OriginalEnabledStatus = enable; }
        }


        /// <summary> Fires each time an answer within this dialog node has been recognized.
        /// Fires the bound command, if available.
        /// </summary>
        /// <param name="sender">The event sender object.</param>
        /// <param name="e">The speech recognized event arguments.</param>
        private void onDialogDone(object sender, SpeechRecognizedEventArgs e)
        {
            SetActive();
        }


        /// <summary> Parses the answer dialog and updates the node's grammar for the speech recognition engine.
        /// </summary>
        private void parseAnswers()
        {
            _grammarList.Clear();
            _grammarStatusList.Clear();

            if (VALIDATION_REGEX.Match(this._text).Success) { return; }

            string[] sentences = this._text.Split(';');

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


        public DialogNodePlayer(string pText = " ", DialogImportance pImportance = DialogImportance.NORMAL, IPlugin pPluginToStart = null) : 
        base(pText, pImportance, pPluginToStart)
        {
            this._speaker = DialogSpeaker.PLAYER;

            this._grammarList = new List<Grammar>();
            this._grammarStatusList = new List<GrammarStatus>();

            parseAnswers();
        }


        #region Public Functions
        /// <summary> Updates the dialog node's  status.
        /// </summary>
        public override void Update()
        {
            bool isEnabled = (
                (this == VI.CurrentDialogNode) ||
                ((this._parentNode != null) && (this._parentNode == VI.CurrentDialogNode))
            );

            this.toggleRecognition(isEnabled);
        }


        /// <summary> Sets this dialog node as the currently active one.
        /// </summary>
        public override void SetActive()
        {
            // Disable the recognized dialog node
            if (this.Importance <= DialogImportance.NORMAL) { this.toggleRecognition(false); }

            // Enable the recognized child nodes
            for (int i = 0; i < _childNodes.Count; i++)
            {
                if (_childNodes[i].Disabled) { continue; }

                if (_childNodes[i].Speaker == DialogSpeaker.VI)
                {
                    SpeechEngine.Say(_childNodes[i]);
                }
                else if (_childNodes[i].Speaker == DialogSpeaker.PLAYER)
                {
                    ((DialogNodePlayer)_childNodes[i]).toggleRecognition(true);
                }
            }

            base.SetActive();
        }
        #endregion
    }
}
