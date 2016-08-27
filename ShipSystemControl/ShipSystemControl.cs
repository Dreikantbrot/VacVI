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


        #region Constants
        const string VI_COMMAND_ACK = "Aye aye;Acknowledged;Sure";
        #endregion


        #region Enums
        private enum JumpState
        {
            MANUAL = 1,
            AUTO = 2, 
            EMERGENCY = 3,
            NONE = ~(MANUAL | AUTO | EMERGENCY)
        }
        #endregion


        #region Variables
        private bool _autoFire = false;
        private bool _autoFireMissile = false;
        private JumpState _jumpState = JumpState.NONE;

        private DialogVI _dialg_Jump = new DialogVI("Initiating $[fulcrum ]jump;Jumping...");
        private DialogVI _dialg_EmergencyJump = new DialogVI("I'm getting us out of here!;Let's get out of here!");
        private DialogVI _dialg_CouldNotJump = new DialogVI("$[Sorry - but] ${we're unable|we don't have enough energy} to jump");
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
            DialogTreeBranch[] dialogOptions = new DialogTreeBranch[] {

                new DialogTreeBranch(
                    new DialogPlayer("$[Auto ]fire;${fire|shoot} when you have a lock on", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI("I'll light'em up, ${when|as soon as} I can", DialogBase.DialogImportance.NORMAL, this.Name, "init_auto_fire")
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("Stop $[Auto-|automatically ]${firing|shooting};Manual fire", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI("${Aye aye|Got it} - manual fire", DialogBase.DialogImportance.NORMAL, this.Name, "init_auto_fire_cancel")
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("${fire|launch|shoot} a missile $[as soon as possible|asap]", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI("Give me a lock-on and I'll shoot", DialogBase.DialogImportance.NORMAL, this.Name, "init_auto_fire_missile")
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("Initiate $[fulcrum ]jump", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI(VI_COMMAND_ACK, DialogBase.DialogImportance.NORMAL, this.Name, "init_jump")
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("$[Initiate ]auto-jump", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI(VI_COMMAND_ACK + " - I'll jump when possible", DialogBase.DialogImportance.NORMAL, this.Name, "init_auto_jump")
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("$[Initiate ]emergency jump;Get ${me|us} out of here", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI(VI_COMMAND_ACK + " - let's get out of here!", DialogBase.DialogImportance.NORMAL, this.Name, "init_emergency_jump")
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("Cancel $[auto-]jump", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI(VI_COMMAND_ACK + " - Cancelling jump;Jump cancelled", DialogBase.DialogImportance.NORMAL, this.Name, "init_auto_jump_cancel")
                    )
                )
            };

            DialogTreeReader.BuildDialogTree(null, dialogOptions);
        }

        public void OnDialogAction(DialogBase originNode)
        {
            switch(originNode.Data.ToString())
            {
                case "init_auto_fire": _autoFire = true; OnGameDataUpdate(); break;
                case "init_auto_fire_cancel": _autoFire = false; break;
                case "init_auto_fire_missile": _autoFireMissile = true; OnGameDataUpdate(); break;

                case "init_jump": if (_jumpState < JumpState.MANUAL) _jumpState = JumpState.MANUAL; EmergencyJump(); break;
                case "init_auto_jump": if (_jumpState < JumpState.AUTO) _jumpState = JumpState.AUTO; EmergencyJump(); break;
                case "init_emergency_jump": if (_jumpState < JumpState.EMERGENCY) _jumpState = JumpState.EMERGENCY; EmergencyJump(); break;
                case "init_auto_jump_cancel": _jumpState = JumpState.NONE; break;
            }
        }

        public void OnGameDataUpdate()
        {
            if (
                (_jumpState == JumpState.AUTO) ||
                (_jumpState == JumpState.EMERGENCY)
            )
            {
                EmergencyJump();
            }
            else
            {
                if (_autoFireMissile) { FireMissile(); }

                if (_autoFire) { Fire(); }
            }
        }

        public List<PluginParameterDefault> GetDefaultPluginParameters()
        {
            return new List<PluginParameterDefault>();
        }
        #endregion


        #region Plugin Functions
        /// <summary> Initiates an (emergency) jump.
        /// </summary>
        private void EmergencyJump()
        {
            if (PlayerShipData.EnergyLevel == 100)
            {
                if (_jumpState == JumpState.EMERGENCY) { Interactor.PressKey(VKeyCodes.VK_LMENU, Interactor.KeyPressMode.KEY_DOWN); }
                Interactor.PressKey(VKeyCodes.VK_F2, Interactor.KeyPressMode.KEY_PRESS, 100);
                if (_jumpState == JumpState.EMERGENCY) { Interactor.PressKey(VKeyCodes.VK_LMENU, Interactor.KeyPressMode.KEY_UP); }

                SpeechEngine.Say((_jumpState == JumpState.EMERGENCY) ? _dialg_EmergencyJump : _dialg_Jump);

                _jumpState = JumpState.NONE;
            }
            else if (_jumpState == JumpState.MANUAL)
            {
                SpeechEngine.Say(_dialg_CouldNotJump);
            }
        }


        /// <summary> Fires all active primary weapons for 1 second.
        /// </summary>
        private void Fire()
        {
            if (PlayerShipData.Mtds == PlayerShipData.MTDSState.LOCKED)
            {
                SpeechEngine.Say("${Pew pew pew!|Take this!}");
                //Interactor.PressKey(VKeyCodes.VK_KEY_B, Interactor.KeyPressMode.KEY_PRESS, 1000);
            }
        }


        /// <summary> Fires a missile.
        /// </summary>
        private void FireMissile()
        {
            if (
                (PlayerShipData.MissileLock == PlayerShipData.MissileState.LOCKED) &&
                (PlayerShipData.SecWeapon[0] != null)
            )
            {
                SpeechEngine.Say("Missile");
                //Interactor.PressKey(VKeyCodes.VK_RBUTTON, Interactor.KeyPressMode.KEY_PRESS, 100);
                _autoFireMissile = false;
            }
        }
        #endregion
    }
}
