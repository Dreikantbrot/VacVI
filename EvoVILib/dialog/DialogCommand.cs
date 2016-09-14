using EvoVI.Plugins;

namespace EvoVI.Dialog
{
    /// <summary> A type of dialog node that only executes a command (without anyone speaking).</summary>
    public class DialogCommand : DialogBase
    {
        #region Constructor
        /// <summary> Creates a dialog node used for simply triggering a plugin, wihtout any speech.
        /// <para>This node is triggered automatically, as soon as it is active.</para>
        /// </summary>
        /// <param name="pPluginToStart">The name of the plugin to start, when triggered.</param>
        /// <param name="pPriority">The node's priority.</param>
        /// <param name="pConditionFunction">The delegate function that checks for the fullfillment of the dialog node's condition.</param>
        /// <param name="pCommandDescr">An optional description of what this command does.</param>
        /// <param name="pData">An object containing custom, user-defined data.</param>
        /// <param name="pFlags">The behaviour-flags, modifying the node's behaviour.</param>
        public DialogCommand(
            string pCommandDescr = "",
            DialogPriority pPriority = DialogPriority.NORMAL,
            System.Func<bool> pConditionFunction = null, 
            string pPluginToStart = null, 
            object pData = null,
            DialogFlags pFlags = DialogFlags.NONE
        ) :
        base(
            "<COMMAND" + ((pCommandDescr.Trim().Length > 0) ? ": \"" + pCommandDescr.Trim() + "\"" : "") + ">",
            pPriority,
            pConditionFunction,
            pPluginToStart, 
            pData,
            pFlags
        )
        {
            this._speaker = DialogSpeaker.COMMAND;
        }
        #endregion


        #region Override Functions
        /// <summary> Sets this dialog node as the currently active one.
        /// </summary>
        public override void SetActive()
        {
            base.SetActive();

            // Auto-trigger if active
            if (IsActive) { Trigger(); }
        }


        /// <summary> Triggers the dialog node activating it's functions.
        /// </summary>
        public override void Trigger()
        {
            base.Trigger();
            
            // Jump to next node, when triggered
            NextNode();
        }
        #endregion
    }
}
