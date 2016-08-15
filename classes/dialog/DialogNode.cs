using Evo_VI.engine;
using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace Evo_VI.classes.dialog
{
    public class DialogNode
    {
        #region Regexes (readonly)
        readonly Regex VALIDATION_REGEX = new Regex(@"^(\s|;|,|.)$");
        readonly Regex CHOICES_REGEX = new Regex(@"(?:\{(?<Choice>.*?)\})|(?:\[(?<OptChoice>.*?)\])");
        #endregion


        #region Enums
        /// <summary> The importance of a line of dialog.
        /// </summary>
        public enum DialogImportance { LOW, NORMAL, HIGH, CRITICAL };
        #endregion


        #region Variables
        private string _viText;
        private string _answerText;
        private DialogImportance _importance;
        private List<Grammar> _grammarList;
        private ICommand _command;
        #endregion


        #region Properties
        /// <summary> Returns or sets the line's importance.
        /// </summary>
        public DialogImportance Importance
        {
            get { return _importance; }
            set { _importance = value; }
        }


        /// <summary> Returns or sets the raw answer text.
        /// </summary>
        public string AnswerText
        {
            get { return _answerText; }
            set { _answerText = value; }
        }


        /// <summary> Returns or sets the VI's raw dialog line.
        /// </summary>
        public string VIText
        {
            get { return _viText; }
            set { _viText = value; }
        }


        /// <summary> Returns the node's list of grammar instances ("sentences").
        /// </summary>
        public List<Grammar> GrammarList
        {
            get { return _grammarList; }
        }
        #endregion


        #region Private Variables
        /// <summary> Fires each time an answer within this dialog node has been recognized.
        /// Fires the bound command, if available.
        /// </summary>
        /// <param name="sender">The event sender object.</param>
        /// <param name="e">The speech recognized event arguments.</param>
        private void onAnswerRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (_command != null) { _command.Action(sender, e, this); }
        }
        #endregion


        #region Public Functions
        /// <summary> Creates a new instance for a node within a dialog tree.
        /// </summary>
        /// <param name="pText">The text to be spoken by the VI.</param>
        /// <param name="pImportance">The importance this line has over others.</param>
        public DialogNode(string pText, DialogImportance pImportance = DialogImportance.NORMAL)
        {
            this._answerText = pText;
            this._importance = pImportance;
            this._grammarList = new List<Grammar>();

            ParseAnswers();
        }


        /// <summary> Parses a randomized composition of the text the VI should speak.
        /// </summary>
        public string ParseVIText(string txt)
        {
            Random rndNr = new Random();
            string result = "";
            string[] sentences = txt.Split(';');
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
        public void ParseAnswers()
        {
            _grammarList.Clear();
            if (VALIDATION_REGEX.Match(this._answerText).Success) { return; }

            string[] sentences = this._answerText.Split(';');

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
    }
}
