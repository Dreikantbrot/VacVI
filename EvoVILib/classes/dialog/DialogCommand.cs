using EvoVI.PluginContracts;

namespace EvoVI.Classes.Dialog
{
    public class DialogCommand : DialogBase
    {
        #region Constructor
        /// <summary> Creates a dialog node used for simply triggering a plugin, wihtout any speech.
        /// <para>This node is triggered automatically, as soon as it is active.</para>
        /// </summary>
        /// <param name="pPluginToStart">The name of the plugin to start, when triggered.</param>
        /// <param name="pImportance">The importance this node has over others.</param>
        /// <param name="pCommandDescr">An optional description of what this command does.</param>
        public DialogCommand(string pPluginToStart = null, DialogImportance pImportance = DialogImportance.NORMAL, string pCommandDescr = "") : 
        base("<COMMAND" + ((pCommandDescr.Trim().Length > 0) ? ": \"" + pCommandDescr.Trim() + "\"" : "") + ">", pImportance, pPluginToStart)
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

            // Auto-trigger when active
            Trigger();
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
