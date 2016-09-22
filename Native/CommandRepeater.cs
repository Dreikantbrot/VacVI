using VacVI.Dialog;
using VacVI.Database;
using VacVI.Plugins;
using System;
using System.Collections.Generic;

namespace Native
{
    public class CommandRepeater : IPlugin
    {
        #region Constants
        private const string I_DID_NOT_UNDERSTAND_YOU = "I did not understand that. Did you mean \"{0}\"?";
        #endregion


        #region Variables
        private Guid _guid = new Guid();
        private DialogBase _jumpBackNode;
        private DialogVI _dialg_didNotUnderstand = new DialogVI("I did not understand that");

        private DialogPlayer _lastMisunderstoodDialog;
        private DialogBase _previousDialogNode;
        #endregion


        #region Properties
        public Guid Id
        {
            get { return _guid; }
        }

        public string Name
        {
            get { return "Command Repeater"; }
        }

        public string Version
        {
            get { return "0.1"; }
        }

        public string Author
        {
            get { return "Scavenger4711"; }
        }

        public string Homepage
        {
            get { return ""; }
        }

        public string Description
        {
            get
            {
                return "Repeats the last command, if it has not been understood by the VI.\n" +
                    "Simply say \"Yes\" or \"No\", when the VI asks your whether you meant the speicifed command.";
            }
        }

        public GameMeta.SupportedGame CompatibilityFlags
        {
            get { return (~GameMeta.SupportedGame.NONE); }
        }
        #endregion


        #region Interface Functions
        public void Initialize()
        {
            SpeechEngine.OnVISpeechRejected +=SpeechEngine_OnVISpeechRejected;
            DialogBase.OnDialogNodeChanged += DialogBase_OnDialogNodeChanged;
        }

        public void OnDialogAction(VacVI.Dialog.DialogBase originNode)
        {
            switch (originNode.Data.ToString())
            {
                case "yes":
                    // Set the last dialog as active
                    if (_lastMisunderstoodDialog != null)
                    {
                        _lastMisunderstoodDialog.SetActive();
                        _lastMisunderstoodDialog.Trigger();
                        _lastMisunderstoodDialog.NextNode();

                        _lastMisunderstoodDialog = null;
                    }
                    break;

                case "no":
                    // Prepare jump-back node
                    _jumpBackNode = (_previousDialogNode != null) ? _previousDialogNode : null;
                    break;

                case "jump_back":
                    // Jumps back to the jumpback node
                    if (_jumpBackNode != null) { _jumpBackNode.SetActive(); } else { DialogTreeBuilder.DialogRoot.SetActive(); }

                    _lastMisunderstoodDialog = null;
                    break;

                default: return;
            }
        }

        public void OnGameDataUpdate()
        {

        }

        public List<PluginParameterDefault> GetDefaultPluginParameters()
        {
            return new List<PluginParameterDefault>();
        }

        public void BuildDialogTree()
        {
            DialogTreeBranch[] dialog = new DialogTreeBranch[] {
                new DialogTreeBranch(
                    _dialg_didNotUnderstand,
                    new DialogTreeBranch(
                        new DialogPlayer("Yes.", DialogBase.DialogPriority.CRITICAL, () => { return (_lastMisunderstoodDialog != null); }, this.Name, "yes", DialogBase.DialogFlags.ALWAYS_UPDATE)
                    ),
                    new DialogTreeBranch(
                        new DialogPlayer("No.", DialogBase.DialogPriority.CRITICAL, () => { return (_lastMisunderstoodDialog != null); }, this.Name, "no", DialogBase.DialogFlags.ALWAYS_UPDATE),
                        new DialogTreeBranch(
                            new DialogVI("$[Oh - I see. ]What $[did you need |was it ]then?", DialogBase.DialogPriority.NORMAL, null, this.Name, "jump_back")
                        )
                    )
                )
            };

            DialogTreeBuilder.BuildDialogTree(null, dialog);
        }

        public void OnProgramShutdown()
        {

        }
        #endregion


        #region Events
        void SpeechEngine_OnVISpeechRejected(SpeechEngine.VISpeechRejectedEventArgs obj)
        {
            // Remember the misunderstood node and start asking what the player meant
            _lastMisunderstoodDialog = obj.RejectedDialog;
            _dialg_didNotUnderstand.RawText = String.Format(I_DID_NOT_UNDERSTAND_YOU, obj.BestAlternative);
            _dialg_didNotUnderstand.SetActive();
        }


        void DialogBase_OnDialogNodeChanged(DialogBase.DialogChangedEventArgs obj)
        {
            // "Log" the last dialog, in order to jump back to it, but ignore our own dialog
            if (obj.PreviousDialog != _dialg_didNotUnderstand) { _previousDialogNode = obj.PreviousDialog; }
        }
        #endregion
    }
}
