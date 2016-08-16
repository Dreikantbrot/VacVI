using EvoVI.engine;
using EvoVI.PluginContracts;
using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace EvoVI.classes.dialog
{
    public class DialogNode
    {
        #region Regexes (readonly)
        readonly Regex VALIDATION_REGEX = new Regex(@"^(\s|;|,|.)$");
        readonly Regex CHOICES_REGEX = new Regex(@"(?:\{(?<Choice>.*?)\})|(?:\[(?<OptChoice>.*?)\])");
        #endregion


        #region Enums
        /// <summary> The person speaking this piece of dialog.
        /// </summary>
        public enum DialogSpeaker { PLAYER, VI, NOBODY };

        /// <summary> The importance of a line of dialog.
        /// </summary>
        public enum DialogImportance { LOW, NORMAL, HIGH, CRITICAL };
        #endregion


        #region Variables
        private string _text;
        private DialogSpeaker _speaker;
        private DialogImportance _importance;
        private List<Grammar> _grammarList;
        private IPlugin _pluginToStart;
        #endregion


        #region Properties
        /// <summary> Returns or sets the line's importance.
        /// </summary>
        public DialogImportance Importance
        {
            get { return _importance; }
            set { _importance = value; }
        }


        /// <summary> Returns the parsed dialog text.
        /// </summary>
        public string Text
        {
            get { return ((_speaker == DialogSpeaker.VI) ? parseVIText() : _text); }
        }


        /// <summary> Returns the one speaking the dialog's text.
        /// </summary>
        public DialogSpeaker Speaker
        {
            get { return _speaker; }
        }


        /// <summary> Returns the node's list of grammar instances ("sentences").
        /// </summary>
        public List<Grammar> GrammarList
        {
            get { return _grammarList; }
        }
        #endregion


        #region Private Functions
        /// <summary> Fires each time an answer within this dialog node has been recognized.
        /// Fires the bound command, if available.
        /// </summary>
        /// <param name="sender">The event sender object.</param>
        /// <param name="e">The speech recognized event arguments.</param>
        private void onAnswerRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (_pluginToStart != null) { _pluginToStart.OnDialogAction(sender, e, this); }
        }


        /// <summary> Parses a randomized composition of the text the VI should speak.
        /// </summary>
        private string parseVIText()
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


        /// <summary> Parses the answer dialog and updates the node's grammar for the speech recognition engine.
        /// </summary>
        private void parseAnswers()
        {
            _grammarList.Clear();
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

                        // Commas and semi-colons make recognition harder
                        leadingText = leadingText.Replace(",", "");
                        leadingText = leadingText.Replace(";", "");
                        builder.Append(leadingText);

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

                builder.Append(sentences[i].Substring(currIndex));

                Grammar resultGrammar = new Grammar(builder);
                resultGrammar.SpeechRecognized += onAnswerRecognized;
                _grammarList.Add(resultGrammar);
            }
        }
        #endregion


        /// <summary> Creates a new instance for a node within a dialog tree.
        /// </summary>
        /// <param name="pText">The text to be spoken by the VI.</param>
        /// <param name="pImportance">The importance this line has over others.</param>
        public DialogNode(string pText=" ", DialogSpeaker pSpeaker = DialogSpeaker.NOBODY, DialogImportance pImportance = DialogImportance.NORMAL)
        {
            this._text = pText;
            this._speaker = pSpeaker;
            this._importance = pImportance;
            this._grammarList = new List<Grammar>();

            switch(this._speaker)
            {
                case DialogSpeaker.PLAYER: parseAnswers(); break;
                case DialogSpeaker.VI: break;

                default: break;
            }
        }
    }
}
