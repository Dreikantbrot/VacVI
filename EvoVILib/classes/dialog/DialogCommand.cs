using EvoVI.PluginContracts;

namespace EvoVI.Classes.Dialog
{
    public class DialogCommand : DialogBase
    {
        #region Constructor
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
