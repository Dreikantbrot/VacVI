using System;
using System.Collections.Generic;
using System.Speech.Synthesis;

namespace EvoVI.classes.dialog
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
                currStruct._node.RegisterTo((parentNode != null) ? parentNode : RootDialogNode);

                currStruct._node.UpdateState();

                BuildDialogTree(currStruct._node, currStruct._children);
            }
        }
        #endregion
    }
}
