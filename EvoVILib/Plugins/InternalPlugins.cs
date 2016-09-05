using EvoVI.Classes.Dialog;
using EvoVI.Database;
using EvoVI.Engine;
using EvoVI.PluginContracts;
using System;
using System.Collections.Generic;

namespace EvoVI.Plugins
{
    internal static class InternalPlugins
    {
        internal class CommandRepeater : IPlugin
        {
            #region Static Vars
            internal const string PLUGIN_NAME = "Command Repeater";
            #endregion


            #region Variables
            private Guid _guid = new Guid();
            private DialogBase _jumpBackNode;
            private DialogPlayer _dialg_yes;
            private DialogPlayer _dialg_no;
            #endregion


            #region Properties
            public Guid Id
            {
                get { return _guid; }
            }

            public string Name
            {
                get { return CommandRepeater.PLUGIN_NAME; }
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
                _dialg_yes = new DialogPlayer("Yes", DialogBase.DialogImportance.CRITICAL, null, this.Name, "yes");
                _dialg_no = new DialogPlayer("No", DialogBase.DialogImportance.CRITICAL, null, this.Name, "no");
                ToggleOnOff(false);

                DialogTreeBranch[] standardDialogs = new DialogTreeBranch[] {
                    new DialogTreeBranch(
                        _dialg_yes
                    ),
                    new DialogTreeBranch(
                        _dialg_no,
                        new DialogTreeBranch(
                            new DialogVI("$[Oh - I see. ]What $[did you need|was it] then?", DialogBase.DialogImportance.NORMAL, null, this.Name, "jump_back")
                        )
                    )
                };

                DialogTreeBuilder.BuildDialogTree(null, standardDialogs);
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

            public void ToggleOnOff(bool on)
            {
                _dialg_yes.Disabled = !on;
                _dialg_no.Disabled = !on;
            }

            public bool IsDisabled()
            {
                return (_dialg_yes.Disabled && _dialg_no.Disabled);
            }
            #endregion
        }

        internal class VIStates : IPlugin
        {
            #region Static Vars
            internal const string PLUGIN_NAME = "VI States";
            #endregion


            #region Variables
            private Guid _guid = new Guid();
            #endregion


            #region Properties
            public Guid Id
            {
                get { return _guid; }
            }

            public string Name
            {
                get { return VIStates.PLUGIN_NAME; }
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
                    return "A plugin for retrieving and setting the VI's state and certain properties.";
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
                        new DialogPlayer("Take a nap;Go to sleep;Goodnight;Go on standby", DialogBase.DialogImportance.CRITICAL, null, this.Name, "sleep"),
                        new DialogTreeBranch(  
                            new DialogVI("Goodnight;Nap time!;Wake me up if you need me.", DialogBase.DialogImportance.NORMAL, () => { return (VI.State >= VI.VIState.READY); }, this.Name, "sleep")
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogPlayer(String.Format("$[Hey ]$[{0} ], $(wake up|I need you|I need your help)", VI.PhoneticName), DialogBase.DialogImportance.CRITICAL),
                        new DialogTreeBranch(  
                            new DialogVI("$[But] I was $[already] awake the whole time.", DialogBase.DialogImportance.NORMAL, () => { return (VI.State >= VI.VIState.READY); })
                        ),
                        new DialogTreeBranch(  
                            new DialogVI("Hello world!;Systems online;Returning from standby.", DialogBase.DialogImportance.NORMAL, () => { return (VI.State < VI.VIState.READY); }, this.Name, "wake_up")
                        )
                    )
                };

                DialogTreeBuilder.BuildDialogTree(null, standardDialogs);
            }

            public void OnDialogAction(Classes.Dialog.DialogBase originNode)
            {
                switch (originNode.Data.ToString())
                {
                    case "sleep":
                        VI.State = VI.VIState.SLEEPING;
                        break;

                    case "wake_up":
                        VI.State = VI.VIState.READY;
                        break;
                }
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
