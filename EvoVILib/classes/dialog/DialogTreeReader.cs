using System;
using System.Collections.Generic;
using System.Speech.Synthesis;

namespace EvoVI.classes.dialog
{
    public struct DialogTreeStruct
    {
        public DialogNode _node;
        public DialogTreeStruct[] _children;

        public DialogTreeStruct(DialogNode pNode, DialogTreeStruct[] pChildren)
        {
            _node = pNode;
            _children = pChildren;
        }
    }

    public static class DialogTreeReader
    {
        public static DialogNode RootDialogNode = new DialogNode();

        public static void BuildDialogTree(DialogTreeStruct[] dialogTree, DialogNode parentNode = null)
        {
            for (int i = 0; i < dialogTree.Length; i++)
            {
                DialogTreeStruct currStruct = dialogTree[i];
                currStruct._node.RegisterTo((parentNode != null) ? parentNode : RootDialogNode);

                currStruct._node.Update();

                BuildDialogTree(currStruct._children, currStruct._node);
            }
        }
    }
}
