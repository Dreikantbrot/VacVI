using System;
using System.Collections.Generic;
using System.IO;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;

namespace EvoVI.Dialog
{
    /// <summary> A class representing a specific branch in the dialog tree.</summary>
    public struct DialogTreeBranch
    {
        #region Variables
        public DialogBase _node;
        public DialogTreeBranch[] _children;
        #endregion


        #region Constructor
        /// <summary> Creates a structure that holds a dialog node and a list of it's child nodes.
        /// </summary>
        /// <param name="pNode">The dialog node.</param>
        /// <param name="pChildren">The list with the node's children.</param>
        public DialogTreeBranch(DialogBase pNode, params DialogTreeBranch[] pChildren)
        {
            _node = pNode;
            _children = new DialogTreeBranch[pChildren.Length];

            for (int i = 0; i < pChildren.Length; i++) { _children[i] = pChildren[i]; }
        }
        #endregion
    }


    /// <summary> Builds the dialog tree and keeps a list of all dialog nodes.</summary>
    public static class DialogTreeBuilder
    {
        #region Variables
        private static DialogBase _dialogRoot = new DialogBase(" ", DialogBase.DialogPriority.VERY_LOW, null, null, null, (DialogBase.DialogFlags.IGNORE_VI_STATE | DialogBase.DialogFlags.INGORE_READY_STATE));
        private static Dictionary<string, DialogPlayer> _grammarLookupTable = new Dictionary<string, DialogPlayer>();
        private static List<DialogBase> _dialogNodes = new List<DialogBase>();
        #endregion


        #region Properties
        /// <summary> Returns the dialog tree's root.
        /// </summary>
        public static DialogBase DialogRoot
        {
            get { return DialogTreeBuilder._dialogRoot; }
        }
        #endregion


        #region Functions
        /// <summary> Builds the specified dialog tree and registers all nodes.
        /// </summary>
        /// <param name="dialogTree">The dialog tree structure.</param>
        /// <param name="parentNode">The parent node of the given tree structure.</param>
        public static void BuildDialogTree(DialogBase parentNode = null, params DialogTreeBranch[] dialogTree)
        {
            for (int i = 0; i < dialogTree.Length; i++)
            {
                DialogTreeBranch currStruct = dialogTree[i];

                if (currStruct._node == null) { continue; }

                currStruct._node.RegisterTo((parentNode != null) ? parentNode : _dialogRoot);
                currStruct._node.UpdateState();

                // Sort into lookup table
                switch(currStruct._node.Speaker)
                {
                    case DialogBase.DialogSpeaker.PLAYER:
                        _grammarLookupTable.Add(currStruct._node.GetHashCode().ToString(), (DialogPlayer)currStruct._node);
                        break;
                }

                _dialogNodes.Add(currStruct._node);
                BuildDialogTree(currStruct._node, currStruct._children);
            }
        }


        /// <summary> Gets the player dialog to which the given grammar rule belongs to.
        /// </summary>
        /// <param name="grammar">The grammar object of which to search for the player dialog node.</param>
        /// <returns>The player dialog node or null on failure.</returns>
        public static DialogPlayer GetPlayerDialog(Grammar grammar)
        {
            string grammarNameHash = grammar.Name;

            if (
                (_grammarLookupTable.ContainsKey(grammarNameHash)) &&
                (_grammarLookupTable[grammarNameHash].GrammarList.Contains(grammar))
            )
            { return _grammarLookupTable[grammarNameHash]; }

            return null;
        }


        /// <summary> Updates all dialog nodes in a "ready" or "listening" state.
        /// </summary>
        internal static void UpdateReadyNodes()
        {
            for (int i = 0; i < _dialogNodes.Count; i++)
            {
                if (_dialogNodes[i].IsReady) { _dialogNodes[i].UpdateState(); }
            }
        }


        /// <summary> Generates an HTML file with all registered dialog nodes and every phrase composition for each dialog node.
        /// </summary>
        /// <param name="targetFilePath">The target filepath.</param>
        /// <param name="playerCommandsOnly">Whether for the file to only include player dialog nodes.</param>
        internal static void GenerateHtmlOverview(string targetFilePath, bool playerCommandsOnly = false)
        {
            StringBuilder htmlFileBuilder = new StringBuilder();
            htmlFileBuilder.AppendLine("");
            htmlFileBuilder.AppendLine("\t<div style='border: 2px solid #555; padding: 15px; margin: 5px 5px 25px 5px;'>");
            htmlFileBuilder.AppendLine("\t\t<div class='PLAYER'>Player Nodes</div>");
            htmlFileBuilder.AppendLine("\t\t<div class='VI'>VI Nodes</div>");
            htmlFileBuilder.AppendLine("\t\t<div class='COMMAND'>Command Nodes</div>");
            htmlFileBuilder.AppendLine("\t</div>");
            htmlFileBuilder.AppendLine("\t<!-- ---------------------- -->");
            htmlFileBuilder.AppendLine("");


            for (int i = 0; i < _dialogRoot.ChildNodes.Count; i++)
            {
                htmlFileBuilder.Append(getDialogPhrasesHTML(_dialogRoot.ChildNodes[i], true));
            }

            using(System.IO.StreamWriter file = new System.IO.StreamWriter(targetFilePath))
            {
                string template = EvoVI.Properties.Resources.DialogTreeHTML_Template;
                file.WriteLine(template.Replace("</body>", htmlFileBuilder + "</body>"));
            }
        }


        /// <summary> Generates the HTML markup for aa single dialog node and it's children.
        /// </summary>
        /// <param name="node">The start node.</param>
        /// <param name="playerCommandsOnly">Whether to only process player dialog nodes.</param>
        /// <param name="level">The current level of indentation.</param>
        /// <returns>The HTML markup in a StringBuilder instance.</returns>
        private static StringBuilder getDialogPhrasesHTML(DialogBase node, bool playerCommandsOnly = false, int level = 0)
        {
            StringBuilder htmlFileBuilder = new StringBuilder();

            if (playerCommandsOnly && (node.Speaker != DialogBase.DialogSpeaker.PLAYER)) { return htmlFileBuilder; }

            htmlFileBuilder.Append('\t', level + 1);
            htmlFileBuilder.AppendLine("<div class='" + node.Speaker.ToString() + "' style='margin-left: " + (level * 40) + "px !important;'>");

            // Check each single sentence in the syntax individually
            string[] sentences = node.RawText.Split(';');
            for (int u = 0; u < sentences.Length; u++)
            {
                htmlFileBuilder.Append('\t', level + 2);
                htmlFileBuilder.AppendLine("<div>");
                htmlFileBuilder.Append('\t', level + 3);
                htmlFileBuilder.AppendLine("<table class='sentenceCompositions'><tr>");

                string currSentence = sentences[u].Replace("<", "&lt;").Replace(">", "&gt;");
                int currIndex = 0;
                MatchCollection matches = DialogBase.CHOICES_REGEX.Matches(currSentence);

                if (String.IsNullOrWhiteSpace(currSentence)) { continue; }

                if (matches.Count > 0)
                {
                    for (int j = 0; j < matches.Count; j++)
                    {
                        Match currMatch = matches[j];

                        // Append "fixed" text
                        string leadingText = currSentence;
                        leadingText = leadingText.Substring(currIndex, matches[j].Index - currIndex).Trim().Replace(" ", "&nbsp;");
                        if (!String.IsNullOrWhiteSpace(leadingText))
                        {
                            htmlFileBuilder.Append('\t', level + 4);
                            htmlFileBuilder.Append("<td class='static'>");
                            htmlFileBuilder.Append(leadingText);
                            htmlFileBuilder.AppendLine("</td>");
                        }

                        string choiceType = (
                            (currMatch.Groups["Choice"].Success) ? "Choice" : 
                            (currMatch.Groups["OptChoice"].Success) ? "OptChoice" : 
                            ""
                        );


                        // Append choices
                        if (currMatch.Groups[choiceType].Success)
                        {
                            htmlFileBuilder.Append('\t', level + 4);
                            htmlFileBuilder.Append("<td class='choice " + choiceType + "'>");
                            
                            string[] choices = matches[j].Groups[choiceType].Value.Split('|');
                            for (int k = 0; k < choices.Length; k++)
                            {
                                string choice = choices[k].Trim().Replace(" ", "&nbsp;");
                                if (String.IsNullOrWhiteSpace(choice)) { continue; }
                                if (k > 0) { htmlFileBuilder.Append("<hr />"); }
                                htmlFileBuilder.Append(choice);
                            }

                            htmlFileBuilder.AppendLine("</td>");
                        }

                        currIndex = matches[j].Index + currMatch.Length;
                    }
                }

                if (!String.IsNullOrWhiteSpace(currSentence.Substring(currIndex)))
                {
                    htmlFileBuilder.Append('\t', level + 4);
                    htmlFileBuilder.Append("<td class='static'>");
                    htmlFileBuilder.Append(currSentence.Substring(currIndex).Trim().Replace(" ", "&nbsp;"));
                    htmlFileBuilder.AppendLine("</td>");
                }

                htmlFileBuilder.Append('\t', level + 3);
                htmlFileBuilder.AppendLine("</tr></table>");

                htmlFileBuilder.Append('\t', level + 2);
                htmlFileBuilder.AppendLine("</div>");
            }

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                htmlFileBuilder.Append(getDialogPhrasesHTML(node.ChildNodes[i], playerCommandsOnly, level + 1));
            }
            htmlFileBuilder.Append('\t', level + 1);
            htmlFileBuilder.AppendLine("</div>");

            return htmlFileBuilder;
        }
        #endregion
    }
}
