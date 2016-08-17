using EvoVI.PluginContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EvoVI.classes.dialog
{
    public class DialogNodeVI : DialogNode
    {
        #region Properties
        /// <summary> Returns the parsed dialog text.
        /// </summary>
        public override string Text
        {
            get { return parseRandomSentence(); }
        }
        #endregion


        #region Private Functions
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


        public DialogNodeVI(string pText = " ", DialogImportance pImportance = DialogImportance.NORMAL, IPlugin pPluginToStart = null) : 
        base(pText, pImportance, pPluginToStart)
        {
            this._speaker = DialogSpeaker.VI;
        }
    }
}
