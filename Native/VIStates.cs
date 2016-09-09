using EvoVI.Classes.Dialog;
using EvoVI.Database;
using EvoVI.Engine;
using EvoVI.PluginContracts;
using EvoVI.Plugins;
using System;
using System.Collections.Generic;

namespace Native
{
    public class VIStates : IPlugin
    {
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
            get { return "VI States"; }
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


        #region Interface Functions
        public void Initialize()
        {
            DialogTreeBranch[] standardDialogs = new DialogTreeBranch[] {
                    new DialogTreeBranch(
                        new DialogPlayer("Take a nap;Go to sleep;Goodnight;Go on standby", DialogBase.DialogPriority.CRITICAL),
                        new DialogTreeBranch(  
                            new DialogVI("Goodnight;Nap time!;Wake me up if you need me.", DialogBase.DialogPriority.NORMAL, () => { return (VI.State >= VI.VIState.READY); }, this.Name, "sleep")
                        )
                    ),

                    new DialogTreeBranch(
                        new DialogPlayer(String.Format("$[Hey ]$[{0} ], $(wake up|I need you|I need your help)", VI.PhoneticName), DialogBase.DialogPriority.CRITICAL, null, null, null, (DialogBase.DialogFlags.IGNORE_VI_STATE)),
                        new DialogTreeBranch(  
                            new DialogVI("$[But] I was $[already] awake the whole time.", DialogBase.DialogPriority.CRITICAL, () => { return (VI.State >= VI.VIState.READY); })
                        ),
                        new DialogTreeBranch(  
                            new DialogVI("Hello world!;Systems online;Returning from standby.", DialogBase.DialogPriority.CRITICAL, () => { return (VI.State < VI.VIState.READY); }, this.Name, "wake_up", (DialogBase.DialogFlags.IGNORE_VI_STATE))
                        )
                    )
                };

            DialogTreeBuilder.BuildDialogTree(null, standardDialogs);
        }

        public void OnDialogAction(EvoVI.Classes.Dialog.DialogBase originNode)
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

        public void OnProgramShutdown()
        {

        }
        #endregion
    }
}
