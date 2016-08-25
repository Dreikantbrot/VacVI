﻿using EvoVI.Engine;
using EvoVI.PluginContracts;
using System;
using System.Text.RegularExpressions;

namespace EvoVI.Classes.Dialog
{
    public class DialogVI : DialogBase
    {
        #region Properties
        /// <summary> Returns the parsed dialog text.
        /// </summary>
        public override string Text
        {
            get { return parseRandomSentence(); }
        }


        /// <summary> Returns the string to display on the GUI.
        /// </summary>
        public override string GUIDisplayText
        {
            get { return "<...>"; }
        }
        #endregion


        #region Constructor
        /// <summary> Creates a dalog node where the VI says a randomly generated arrangement of the specified text.
        /// <para>This node is triggered automatically, as soon as it is active.</para>
        /// </summary>
        /// <param name="pText">The text for the VI to say (see dialog text syntax).</param>
        /// <param name="pImportance">The importance this node has over others.</param>
        /// <param name="pPluginToStart">The name of the plugin to start, when triggered.</param>
        public DialogVI(string pText = " ", DialogImportance pImportance = DialogImportance.NORMAL, string pPluginToStart = null) : 
        base(pText, pImportance, pPluginToStart)
        {
            this._speaker = DialogSpeaker.VI;
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
            
            // Auto-trigger when active
            Trigger();
        }


        /// <summary> Triggers the dialog node activating it's functions.
        /// </summary>
        public override void Trigger()
        {
            SpeechEngine.Say(this);
            base.Trigger();
        }
        #endregion
    }
}
