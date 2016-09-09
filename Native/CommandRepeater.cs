using EvoVI.Classes.Dialog;
using EvoVI.Database;
using EvoVI.Engine;
using EvoVI.PluginContracts;
using EvoVI.Plugins;
using System;
using System.Collections.Generic;

namespace Native
{
    public class CommandRepeater : IPlugin
    {
        #region Variables
        private Guid _guid = new Guid();
        private DialogBase _jumpBackNode;
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
            DialogTreeBranch[] standardDialogs = new DialogTreeBranch[] {
                    new DialogTreeBranch(
                        new DialogPlayer("Yes", DialogBase.DialogPriority.CRITICAL, () => { return (VI.LastMisunderstoodDialogNode != null); }, this.Name, "yes")
                    ),
                    new DialogTreeBranch(
                        new DialogPlayer("No", DialogBase.DialogPriority.CRITICAL, () => { return (VI.LastMisunderstoodDialogNode != null); }, this.Name, "no"),
                        new DialogTreeBranch(
                            new DialogVI("$[Oh - I see.] What $[did you need|was it] then?", DialogBase.DialogPriority.NORMAL, null, this.Name, "jump_back")
                        )
                    )
                };

            DialogTreeBuilder.BuildDialogTree(null, standardDialogs);
        }

        public void OnDialogAction(EvoVI.Classes.Dialog.DialogBase originNode)
        {
            switch (originNode.Data.ToString())
            {
                case "yes":
                    if (VI.LastMisunderstoodDialogNode != null)
                    {
                        VI.LastMisunderstoodDialogNode.SetActive();
                        VI.LastMisunderstoodDialogNode.Trigger();
                        VI.LastMisunderstoodDialogNode.NextNode();
                    }
                    break;

                case "no":
                    _jumpBackNode = (VI.PreviousDialogNode != null) ? (DialogBase)VI.PreviousDialogNode : null;
                    break;

                case "jump_back":
                    if (_jumpBackNode != null) { _jumpBackNode.SetActive(); } else { DialogTreeBuilder.RootDialogNode.SetActive(); }
                    break;

                default: return;
            }

            // VI.Last<***> property reset and dialog disabling is done automatically by the SpeechEngine.onSpeechRecognized-event afterward
        }

        public void OnGameDataUpdate()
        {

        }

        public List<PluginParameterDefault> GetDefaultPluginParameters()
        {
            return new List<PluginParameterDefault>();
        }

        public void OnProgramShutdown()
        {

        }
        #endregion
    }
}
