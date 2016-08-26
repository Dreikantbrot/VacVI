using EvoVI.Classes.Dialog;
using EvoVI.Database;
using EvoVI.Engine;
using EvoVI.PluginContracts;
using EvoVI.Plugins;
using System;
using System.Collections.Generic;

namespace ShipSystemControl
{
    public class ShipSystemControl : IPlugin
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
                return "Enables the VI to control basic ship functions like switching weapons and initiating jumps.";
            }
        }

        public Guid Id
        {
            get { return GUID; }
        }

        public string Name
        {
            get { return "Ship System Control"; }
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


        #region Interface Functions
        public void Initialize()
        {
            // TODO: Build dialog tree
            DialogTreeBranch[] dialogOptions = new DialogTreeBranch[] {
                new DialogTreeBranch(
                    new DialogPlayer("[Open ]fire;Shoot them up!", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogCommand("Let's light'em up", DialogBase.DialogImportance.NORMAL, this.Name, "open_fire")
                    )
                ),
                new DialogTreeBranch(
                    new DialogPlayer("Initiate [fulcrum ]jump", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI("Initiating [fulcrum ]jump;Jumping;Aye aye!", DialogBase.DialogImportance.NORMAL, this.Name, "init_jump")
                    )
                ),
                new DialogTreeBranch(
                    new DialogPlayer("[Initiate ]emergency jump;Get {me|us} out of here", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI("Aye aye!;At once!;On it!", DialogBase.DialogImportance.NORMAL, this.Name, "init_emergency_jump")
                    )
                )
            };

            DialogTreeReader.BuildDialogTree(null, dialogOptions);
        }

        public void OnDialogAction(DialogBase originNode)
        {
            switch(originNode.Data.ToString())
            {
                case "open_fire":
                    Interactor.PressKey(VKeyCodes.VK_KEY_T, Interactor.KeyPressMode.KEY_PRESS, 1500);
                    break;

                case "init_jump":
                    Interactor.PressKey(VKeyCodes.VK_F2, Interactor.KeyPressMode.KEY_PRESS, 300);
                    break;

                case "init_emergency_jump":
                    Interactor.PressKey(VKeyCodes.VK_LMENU, Interactor.KeyPressMode.KEY_DOWN);
                    Interactor.PressKey(VKeyCodes.VK_F2, Interactor.KeyPressMode.KEY_PRESS, 300);
                    Interactor.PressKey(VKeyCodes.VK_LMENU, Interactor.KeyPressMode.KEY_UP);
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
