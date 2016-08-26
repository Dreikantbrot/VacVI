using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace EvoVI.Classes.Dialog
{
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

    public static class DialogTreeReader
    {
        #region Variables
        public static DialogBase RootDialogNode = new DialogBase();
        private static Dictionary<string, DialogPlayer> _grammarLookupTable = new Dictionary<string, DialogPlayer>();
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

                currStruct._node.RegisterTo((parentNode != null) ? parentNode : RootDialogNode);
                currStruct._node.UpdateState();

                // Sort into lookup table
                switch(currStruct._node.Speaker)
                {
                    case DialogBase.DialogSpeaker.PLAYER:
                        _grammarLookupTable.Add(currStruct._node.GetHashCode().ToString(), (DialogPlayer)currStruct._node);
                        break;
                }

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
        #endregion
    }
}
