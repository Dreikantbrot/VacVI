using EvoVI.PluginContracts;
using System;
using EvoVI.Classes.Dialog;
using EvoVI.Engine;
using EvoVI.Database;
using System.Collections.Generic;
using EvoVI.Plugins;

namespace Smalltalk
{
    public class Smalltalk : IPlugin
    {
        #region GUID (readonly)
        readonly Guid GUID = Guid.NewGuid();
        #endregion


        #region Plugin Info
        public GameMeta.SupportedGame CompatibilityFlags
        {
            get { return (GameMeta.SupportedGame.EVOCHRON_MERCENARY | GameMeta.SupportedGame.EVOCHRON_LEGACY); }
        }

        public string Description
        {
            get
            {
                return "This Plugin enables the VI to engage in smalltalk with the player.\n" +
                    "The conversations themselves don't affect the ship's operations and serve primarily to build the " +
                    "relationship between the player and the VI";
            }
        }

        public Guid Id
        {
            get { return GUID; }
        }

        public string Name
        {
            get { return "Smalltalk"; }
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
        #endregion


        #region Variables
        private Dictionary<string, string> _parameter = new Dictionary<string,string>();
        #endregion


        #region Interface Functions
        public void Initialize()
        {
            DialogTreeBranch[] dialogOptions = new DialogTreeBranch[] {
                new DialogTreeBranch(
                    new DialogPlayer("Who are you?"),
                    new DialogTreeBranch(
                        new DialogVI(String.Format("I am a virtual intelligence and ship assistance software. $[My name is|You can call me|Please, call me|Call me] {0}", VI.PhoneticName))
                    )
                )
            };

            DialogTreeBuilder.BuildDialogTree(null, dialogOptions);
        }

        public void OnDialogAction(EvoVI.Classes.Dialog.DialogBase originNode)
        {

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
