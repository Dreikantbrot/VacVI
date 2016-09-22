using EvoVI.Dialog;
using EvoVI.Database;
using EvoVI.Plugins;
using System;
using System.Collections.Generic;
using EvoVI;

namespace Native
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
        private Dictionary<string, string> _parameter = new Dictionary<string, string>();
        #endregion


        #region Interface Functions
        public void Initialize()
        {

        }

        public void OnDialogAction(EvoVI.Dialog.DialogBase originNode)
        {

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
                    new DialogPlayer("Who are you?"),
                    new DialogTreeBranch(
                        new DialogVI(String.Format("I am a virtual intelligence and ship assistance software. $[My name is|You can call me|Please, call me|Call me] \"{0}\".", VI.PhoneticName))
                    )
                ),
                new DialogTreeBranch(
                    new DialogPlayer("What is your favourite food"),
                    new DialogTreeBranch(
                        new DialogVI("I like $(muffins|cornflakes|pizza|pancakes|small children for breakfast). What do you like most?"),
                        new DialogTreeBranch(
                            new DialogPlayer("I like $(soup|pizza|muffins|cornflakes)."),
                            new DialogTreeBranch(
                                new DialogVI("Good choice!")
                            )
                        ),
                        new DialogTreeBranch(
                            new DialogPlayer("I don't eat."),
                            new DialogTreeBranch(
                                new DialogVI("You should! Your $(parents|mother|father) will worry $[about you] otherwise!")
                            )
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
    }
}
