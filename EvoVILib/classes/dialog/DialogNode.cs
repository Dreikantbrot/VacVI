using EvoVI.engine;
using EvoVI.PluginContracts;
using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Text.RegularExpressions;

// TODO: Break this class apart into 3 child-classes - 1 for each speaker

namespace EvoVI.classes.dialog
{
    [System.Diagnostics.DebuggerDisplay("{Speaker}: {Text}")]
    public class DialogNode
    {
        #region Regexes (readonly)
        protected readonly Regex VALIDATION_REGEX = new Regex(@"^(?:\s|\.|,|;|:)*$");
        protected readonly Regex CHOICES_REGEX = new Regex(@"(?:\{(?<Choice>.*?)\})|(?:\[(?<OptChoice>.*?)\])");
        #endregion


        #region Enums
        /// <summary> The person speaking this piece of dialog.
        /// </summary>
        public enum DialogSpeaker { PLAYER, VI, NOBODY, NULL };

        /// <summary> The importance of a line of dialog.
        /// </summary>
        public enum DialogImportance { LOW=0, NORMAL=1, HIGH=2, CRITICAL=3 };
        #endregion


        #region Variables
        protected bool _disabled;
        protected string _text;
        protected DialogNode _parentNode;
        protected List<DialogNode> _childNodes;
        protected DialogSpeaker _speaker;
        protected DialogImportance _importance;
        protected IPlugin _pluginToStart;
        #endregion


        #region Properties
        /// <summary> Returns or sets the line's importance.
        /// </summary>
        public DialogImportance Importance
        {
            get { return _importance; }
            set { _importance = value; }
        }


        /// <summary> Returns or sets whether the dialog node is disabled or not.
        /// </summary>
        public virtual bool Disabled
        {
            get { return _disabled; }
            set { _disabled = value; }
        }


        /// <summary> Returns the parsed dialog text.
        /// </summary>
        public virtual string Text
        {
            get { return _text; }
        }


        /// <summary> Returns the one speaking the dialog's text.
        /// </summary>
        public DialogSpeaker Speaker
        {
            get { return _speaker; }
        }


        /// <summary> Returns the node's parent.
        /// </summary>
        public DialogNode ParentNode
        {
            get { return _parentNode; }
        }


        /// <summary> Returns the node's children.
        /// </summary>
        public List<DialogNode> ChildNodes
        {
            get { return _childNodes; }
        }
        #endregion


       /// <summary> Creates a new instance for a node within a dialog tree.
        /// </summary>
        /// <param name="pText">The text to be spoken by the VI.</param>
        /// <param name="pImportance">The importance this line has over others.</param>
        public DialogNode(string pText = " ", DialogImportance pImportance = DialogImportance.NORMAL, IPlugin pPluginToStart = null)
        {
            this._text = pText;
            this._importance = pImportance;
            this._pluginToStart = pPluginToStart;

            this._disabled = false;
            this._speaker = DialogSpeaker.NULL;
            this._childNodes = new List<DialogNode>();
        }


        #region Public Functions
        /// <summary> Registers this dialog node as a child node to some other dialog node.
        /// </summary>
        /// <param name="targetParentNode">The node to register it to.</param>
        public void RegisterTo(DialogNode targetParentNode)
        {
            // Orphan node ...
            if (this._parentNode != null) { this._parentNode.RemoveChild(this); }

            // ... and adopt it
            this._parentNode = targetParentNode;
            this._parentNode.AddChild(this);
        }


        /// <summary> Adds a child to this dialog node.
        /// </summary>
        /// <param name="targetParentNode">The node to add.</param>
        public void AddChild(DialogNode targetNode)
        {
            if (!this._childNodes.Contains(targetNode)) { this._childNodes.Add(targetNode); }
        }


        /// <summary> Removes a child from this dialog node.
        /// </summary>
        /// <param name="targetParentNode">The node to remove.</param>
        public void RemoveChild(DialogNode targetNode)
        {
            if (this._childNodes.Contains(targetNode)) { this._childNodes.Remove(targetNode); }
        }


        /// <summary> Sets this dialog node as the currently active one.
        /// </summary>
        public virtual void SetActive()
        {
            if (_pluginToStart != null) { _pluginToStart.OnDialogAction(this); }

            VI.PreviousDialogNode = VI.CurrentDialogNode;
            VI.CurrentDialogNode = this;
        }


        /// <summary> Updates the dialog node's status.
        /// </summary>
        public virtual void Update()
        {
        }
        #endregion
    }
}
