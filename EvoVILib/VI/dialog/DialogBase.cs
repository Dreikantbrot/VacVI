﻿using EvoVI.Engine;
using EvoVI.PluginContracts;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EvoVI.Classes.Dialog
{
    [System.Diagnostics.DebuggerDisplay("{Speaker}: {Text}")]
    public class DialogBase
    {
        #region Regexes (readonly)
        protected readonly Regex VALIDATION_REGEX = new Regex(@"^(?:\s|\.|,|;|:)*$");
        protected readonly Regex CHOICES_REGEX = new Regex(@"(?:\$\((?<Choice>.*?)\))|(?:\$\[(?<OptChoice>.*?)\])");
        #endregion


        #region Delegates
        public delegate bool DialogConditionFnc();
        #endregion


        #region Enums
        /// <summary> The person speaking this piece of dialog.
        /// </summary>
        public enum DialogSpeaker { PLAYER, VI, COMMAND, NULL };

        /// <summary> The importance of a line of dialog.
        /// </summary>
        public enum DialogPriority { VERY_LOW=0, LOW=1, NORMAL=2, HIGH=3, VERY_HIGH=4, CRITICAL=5 };

        /// <summary> A collection of flags determining the node's behaviour.
        /// </summary>
        [Flags]
        public enum DialogFlags { NONE=0, ALWAYS_UPDATE=1, INGORE_READY_STATE=2, IGNORE_VI_STATE=4 };
        #endregion


        #region Variables
        protected bool _disabled;
        protected string _text;
        protected DialogBase _parentNode;
        protected List<DialogBase> _childNodes;
        protected DialogSpeaker _speaker;
        protected DialogPriority _priority;
        protected string _pluginToStart;
        protected Func<bool> _conditionFunction;
        protected object _data;
        protected DialogFlags _flags;
        #endregion


        #region Properties
        /// <summary> Returns or sets the line's importance.
        /// </summary>
        public DialogPriority Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }


        /// <summary> Returns or sets whether the dialog node is disabled or not.
        /// </summary>
        public virtual bool Disabled
        {
            get { return _disabled; }
            set { _disabled = value; }
        }


        /// <summary> Returns the unparsed, raw dialog text.
        /// </summary>
        public string RawText
        {
            get { return _text; }
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


        /// <summary> Returns or sets the delegate function that checks for the fullfillment of the dialog node's condition.
        /// </summary>
        public Func<bool> ConditionFunction
        {
            get { return _conditionFunction; }
            set { _conditionFunction = value; }
        }


        /// <summary> Returns or sets the custom data contained within the dialog node.
        /// </summary>
        public object Data
        {
            get { return _data; }
            set { _data = value; }
        }


        /// <summary> Returns or sets the dialog node's behaviour flags.
        /// </summary>
        protected DialogFlags Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }


        /// <summary> Returns whether a node is ready and can be activated or triggered.
        /// </summary>
        public virtual bool IsReady
        {
            get
            {
                return (
                    (
                        (VI.State > VI.VIState.SLEEPING) ||
                        ((_flags & DialogFlags.IGNORE_VI_STATE) == DialogFlags.IGNORE_VI_STATE)
                    ) &&
                    (
                        (IsActive || IsNextInTurn) ||
                        (
                            (_priority >= DialogPriority.CRITICAL) ||
                            ((_flags & DialogFlags.INGORE_READY_STATE) == DialogFlags.INGORE_READY_STATE)
                        )
                    )
                ); 
            }
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
        internal virtual string GUIDisplayText
        {
            get { return this._text; }
        }
        #endregion


        #region Constructor
        /// <summary> Creates a new instance for a node within a dialog tree.
        /// </summary>
        /// <param name="pText">The text to be spoken by the VI.</param>
        /// <param name="pPriority">The node's priority.</param>
        /// <param name="pConditionFunction">The delegate function that checks for the fullfillment of the dialog node's condition.</param>
        /// <param name="pPluginToStart">The name of the plugin to start, when triggered.</param>
        /// <param name="pData">An object containing custom, user-defined data.</param>
        /// <param name="pFlags">The behaviour-flags, modifying the node's behaviour.</param>
        public DialogBase(
            string pText = " ",
            DialogPriority pPriority = DialogPriority.NORMAL,
            Func<bool> pConditionFunction = null,
            string pPluginToStart = null,
            object pData = null,
            DialogFlags pFlags = DialogFlags.NONE
        )
        {
            this._text = pText;
            this._priority = pPriority;
            this._pluginToStart = pPluginToStart;
            this._data = pData;
            this._conditionFunction = pConditionFunction;
            this._flags = pFlags;

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


        /// <summary> Checks whether the node's condition has been fullfilled.
        /// </summary>
        /// <returns>Whether the node's condition has been fullfilled</returns>
        public bool CheckCondition()
        {
            return (
                (_conditionFunction == null) ||
                (_conditionFunction())
            );
        }


        /// <summary> Finds and activates the next node within the dialog tree.
        /// </summary>
        public void NextNode()
        {
            if (!IsActive) { return; }

            List<DialogBase> candidatePhrases = new List<DialogBase>();

            /* Collect candidate phrases and determine highest priority */
            DialogPriority highestPriority = DialogPriority.VERY_LOW;
            for (int i = 0; i < _childNodes.Count; i++)
            {
                // Filter disabled nodes or nodes that already do not meet the maximum importance or their condition
                if (
                    (_childNodes[i].Disabled) ||
                    (_childNodes[i].Priority < highestPriority) ||
                    (!_childNodes[i].IsReady) ||
                    (!_childNodes[i].CheckCondition())
                )
                { continue; }

                // Update highest priority
                if (_childNodes[i].Priority > highestPriority) { highestPriority = _childNodes[i].Priority; }

                candidatePhrases.Add(_childNodes[i]);
            }


            /* Filter out the rest of the collected nodes by importance */
            for (int i = 0; i < candidatePhrases.Count; i++) { if (candidatePhrases[i].Priority < highestPriority) candidatePhrases.RemoveAt(i); }


            /* Filter out mixed types - go by dominant type */
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


            /* Only set next node active if NOT waiting for player input */
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
                    DialogTreeBuilder.DialogRoot.SetActive();
                }
            }
        }
        #endregion


        #region Override Functions
        /// <summary> Sets this dialog node as the currently active one.
        /// </summary>
        public virtual void SetActive()
        {
            if (!IsReady) { return; }

            VI.PreviousDialogNode = VI.CurrentDialogNode;
            VI.CurrentDialogNode = this;

            DialogTreeBuilder.UpdateReadyNodes();
        }


        /// <summary> Triggers the dialog node activating it's functions.
        /// </summary>
        public virtual void Trigger()
        {
            if (!IsReady) { return; }

            IPlugin plugin = PluginManager.GetPlugin(_pluginToStart);
            VI.CurrCommand = plugin;
            if (plugin != null) { plugin.OnDialogAction(this); }
        }


        /// <summary> Updates the dialog node's status.
        /// </summary>
        public virtual void UpdateState()
        {
        }
        #endregion
    }
}