using EvoVI.Engine;
using EvoVI.PluginContracts;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// TODO: Clean up!

namespace EvoVI.Classes.Dialog
{
    [System.Diagnostics.DebuggerDisplay("{Speaker}: {Text}")]
    public class DialogBase
    {
        #region Regexes (readonly)
        protected readonly Regex VALIDATION_REGEX = new Regex(@"^(?:\s|\.|,|;|:)*$");
        protected readonly Regex CHOICES_REGEX = new Regex(@"(?:\$\{(?<Choice>.*?)\})|(?:\$\[(?<OptChoice>.*?)\])");
        #endregion


        #region Enums
        /// <summary> The person speaking this piece of dialog.
        /// </summary>
        public enum DialogSpeaker { PLAYER, VI, COMMAND, NULL };

        /// <summary> The importance of a line of dialog.
        /// </summary>
        public enum DialogImportance { LOW=0, NORMAL=1, HIGH=2, CRITICAL=3 };
        #endregion


        #region Variables
        protected bool _disabled;
        protected string _text;
        protected DialogBase _parentNode;
        protected List<DialogBase> _childNodes;
        protected DialogSpeaker _speaker;
        protected DialogImportance _importance;
        protected string _pluginToStart;
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
        public DialogBase ParentNode
        {
            get { return _parentNode; }
        }


        /// <summary> Returns the node's children.
        /// </summary>
        public List<DialogBase> ChildNodes
        {
            get { return _childNodes; }
        }


        /// <summary> Returns whether a node is ready and can be triggered.
        /// </summary>
        public virtual bool IsReady
        {
            get { return true; }
        }


        /// <summary> Returns whether this node is currently active within the dialog tree.
        /// </summary>
        public bool IsActive
        {
            get { return (this == VI.CurrentDialogNode); }
        }


        /// <summary> Returns whether it's parent is currently active within the dialog tree.
        /// </summary>
        public bool IsNextInTurn
        {
            get
            {
                return (
                    (this != VI.CurrentDialogNode) &&
                    ((_parentNode != null) && (_parentNode == VI.CurrentDialogNode))
                );
            }
        }


        /// <summary> Returns the string to display on the GUI.
        /// </summary>
        public virtual string GUIDisplayText
        {
            get { return this._text; }
        }
        #endregion


        #region Constructor
        /// <summary> Creates a new instance for a node within a dialog tree.
        /// </summary>
        /// <param name="pText">The text to be spoken by the VI.</param>
        /// <param name="pImportance">The importance this line has over others.</param>
        public DialogBase(string pText = " ", DialogImportance pImportance = DialogImportance.NORMAL, string pPluginToStart = null)
        {
            this._text = pText;
            this._importance = pImportance;
            this._pluginToStart = pPluginToStart;

            this._disabled = false;
            this._speaker = DialogSpeaker.NULL;
            this._childNodes = new List<DialogBase>();
        }
        #endregion


        #region Functions
        /// <summary> Registers this dialog node as a child node to some other dialog node.
        /// </summary>
        /// <param name="targetParentNode">The node to register it to.</param>
        public void RegisterTo(DialogBase targetParentNode)
        {
            // Orphan node ...
            if (_parentNode != null) { _parentNode.RemoveChild(this); }

            // ... and adopt it
            _parentNode = targetParentNode;
            _parentNode.AddChild(this);
        }


        /// <summary> Adds a child to this dialog node.
        /// </summary>
        /// <param name="targetParentNode">The node to add.</param>
        public void AddChild(DialogBase targetNode)
        {
            if (!_childNodes.Contains(targetNode)) { _childNodes.Add(targetNode); }
        }


        /// <summary> Removes a child from this dialog node.
        /// </summary>
        /// <param name="targetParentNode">The node to remove.</param>
        public void RemoveChild(DialogBase targetNode)
        {
            if (_childNodes.Contains(targetNode)) { _childNodes.Remove(targetNode); }
        }
        #endregion


        #region Override Functions
        /// <summary> Sets this dialog node as the currently active one.
        /// </summary>
        public virtual void SetActive()
        {
            VI.PreviousDialogNode = VI.CurrentDialogNode;
            VI.CurrentDialogNode = this;

            // Update status of the dalog node + siblings
            for (int i = 0; i < _childNodes.Count; i++) { _childNodes[i].UpdateState(); }
            if (_parentNode != null)
            {
                for (int i = 0; i < _parentNode._childNodes.Count; i++) { _parentNode._childNodes[i].UpdateState(); }
            }
        }


        /// <summary> Triggers the dialog node activating it's functions.
        /// </summary>
        public virtual void Trigger()
        {
            IPlugin plugin = PluginManager.GetPlugin(_pluginToStart);
            if (plugin != null) { plugin.OnDialogAction(this); }
        }


        /// <summary> Finds and activates the next node within the dialog tree.
        /// </summary>
        public virtual void NextNode()
        {
            if (!this.IsActive) { return; }

            List<DialogBase> candidatePhrases = new List<DialogBase>();

            // Collect candidate phrases and determine highest priority
            DialogImportance highestPriority = DialogImportance.LOW;
            for (int i = 0; i < _childNodes.Count; i++)
            {
                // Filter disabled nodes or nodes that already do not meet the maximum importance
                if (
                    (_childNodes[i].Disabled) ||
                    (_childNodes[i].Importance < highestPriority)
                )
                { continue; }

                // Update highest priority
                if (_childNodes[i].Importance > highestPriority) { highestPriority = _childNodes[i].Importance; }

                // TODO: Filter out nodes that do not meet their conditions

                candidatePhrases.Add(_childNodes[i]);
            }

            // Filter out the rest of the collected nodes by importance
            for (int i = 0; i < candidatePhrases.Count; i++) { if (candidatePhrases[i].Importance < highestPriority) candidatePhrases.RemoveAt(i); }

            // TODO: Filter out mixed types - go by dominant type
            Dictionary<DialogSpeaker, int> typeDominance = new Dictionary<DialogSpeaker, int>();
            for (int i = 0; i < candidatePhrases.Count; i++)
            {
                if (!typeDominance.ContainsKey(candidatePhrases[i].Speaker))
                {
                    typeDominance.Add(candidatePhrases[i].Speaker, 1);
                }
                else
                {
                    typeDominance[candidatePhrases[i].Speaker]++;
                }
            }
            
            int highestDominance = 0;
            DialogSpeaker dominantType = DialogSpeaker.NULL;
            foreach (KeyValuePair<DialogSpeaker, int> keyVal in typeDominance)
            {
                if (keyVal.Value > highestDominance)
                {
                    highestDominance = keyVal.Value;
                    dominantType = keyVal.Key;
                }
            }
            for (int i = 0; i < candidatePhrases.Count; i++) { if (candidatePhrases[i].Speaker != dominantType) candidatePhrases.RemoveAt(i); }

            // Only set next node active if NOT waiting for player input
            if (dominantType != DialogSpeaker.PLAYER)
            {
                // Select a winner phrase
                if (candidatePhrases.Count > 0)
                {
                    // Choose random element from the candidates (in case there are multiple ones)
                    System.Random randNr = new System.Random();
                    candidatePhrases[randNr.Next(0, candidatePhrases.Count)].SetActive();
                }
                else
                {
                    // No "winner" phrase - back to root
                    DialogTreeReader.RootDialogNode.SetActive();
                }
            }
        }


        /// <summary> Updates the dialog node's status.
        /// </summary>
        public virtual void UpdateState()
        {
        }
        #endregion
    }
}
