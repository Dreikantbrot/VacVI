using EvoVI.Classes.Dialog;
using EvoVI.Database;
using EvoVI.Engine;
using EvoVI.PluginContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoVI.Plugins
{
    internal static class InternalPlugins
    {
        internal class CommandRepeater : IPlugin
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


            #region Functions
            public void Initialize()
            {
                DialogTreeBranch[] standardDialogs = new DialogTreeBranch[] {
                    new DialogTreeBranch(
                        new DialogPlayer("Yes", DialogBase.DialogImportance.CRITICAL, "Command Repeater", "yes")
                    ),
                    new DialogTreeBranch(
                        new DialogPlayer("No", DialogBase.DialogImportance.CRITICAL, "Command Repeater", "no"),
                        new DialogTreeBranch(
                            new DialogVI("What was it then?", DialogBase.DialogImportance.NORMAL, this.Name, "jump_back")
                        )
                    )
                };
            }

            public void OnDialogAction(Classes.Dialog.DialogBase originNode)
            {
                switch(originNode.Data.ToString())
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
                        if (_jumpBackNode != null) { _jumpBackNode.SetActive(); } else { DialogTreeReader.RootDialogNode.SetActive(); }
                        break;
                }

                // VI.Last***-reset is done automatically by the event afterward
            }

            public void OnGameDataUpdate()
            {

            }

            public List<PluginParameterDefault> GetDefaultPluginParameters()
            {
                return new List<PluginParameterDefault>();
            }
            #endregion
        }
    }
}
