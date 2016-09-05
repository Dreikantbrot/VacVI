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
        const string VI_COMMAND_ACK = "$(Aye aye|Acknowledged|Sure)";
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
        private DateTime _lastTimeFired = DateTime.Now;

        private DialogVI _dialg_Jump = new DialogVI("Initiating $[fulcrum ]jump;Jumping...");
        private DialogVI _dialg_EmergencyJump = new DialogVI("I'm getting us out of here!;Let's get out of here!");
        private DialogVI _dialg_CouldNotJump = new DialogVI("$[Sorry - but] $(we're unable|we don't have enough energy) to jump");
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
                    new DialogPlayer("$[Auto ]fire;$(fire|shoot) when you have a lock on", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI("I'll light'em up, $(when|as soon as) I can", DialogBase.DialogImportance.NORMAL, null, this.Name, "auto_fire")
                    ),
                    new DialogTreeBranch(
                        new DialogVI("$[I'm sorry but|Sorry but] I won't shoot at a friendly target.", DialogBase.DialogImportance.HIGH, () => { return (TargetShipData.ThreatLevel <= ThreadLevelState.LOW); }, this.Name, "auto_fire_cancel")
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("Stop $[Auto-|automatically ]$(firing|shooting);Manual fire", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI("$(Aye aye|Got it) - manual fire", DialogBase.DialogImportance.NORMAL, null, this.Name, "auto_fire_cancel")
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("$(fire|launch|shoot) a missile $[as soon as possible|asap]", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI("$[I'm sorry but|Sorry but] I won't shoot at a friendly target.", DialogBase.DialogImportance.HIGH, () => { return (TargetShipData.ThreatLevel <= ThreadLevelState.LOW); }, this.Name, "auto_fire_missile_cancel")
                    ),
                    new DialogTreeBranch(
                        new DialogVI("$[I'm|I am] already on it!", DialogBase.DialogImportance.NORMAL, () => { return (_autoFireMissile); })
                    ),
                    new DialogTreeBranch(
                        new DialogVI("Give me a lock-on and I'll shoot", DialogBase.DialogImportance.LOW, null, this.Name, "auto_fire_missile")
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("Cancel missile launch", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI("$(Aye aye|Got it) - cancelling missile launch", DialogBase.DialogImportance.NORMAL, null, this.Name, "auto_fire_missile_cancel")
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("Initiate $[fulcrum ]jump", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI(VI_COMMAND_ACK, DialogBase.DialogImportance.NORMAL, null, this.Name, "jump")
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("$[Initiate ]auto-jump;Jump $(as soon as possible|asap)", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI(VI_COMMAND_ACK + " - I'll jump when possible", DialogBase.DialogImportance.NORMAL, null, this.Name, "auto_jump")
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("$[Initiate ]emergency jump;Get $(me|us) out of here", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI(VI_COMMAND_ACK + " - let's get out of here!", DialogBase.DialogImportance.HIGH, () => { return (PlayerShipData.EnergyLevel <= 60); }, this.Name, "emergency_jump")
                    ),
                    new DialogTreeBranch(
                        new DialogVI(VI_COMMAND_ACK, DialogBase.DialogImportance.NORMAL, null, this.Name, "emergency_jump")
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("Cancel $[auto-]jump", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI(VI_COMMAND_ACK + " - Cancelling jump;Jump cancelled", DialogBase.DialogImportance.NORMAL, null, this.Name, "auto_jump_cancel")
                    )
                )
            };

            DialogTreeBuilder.BuildDialogTree(null, dialogOptions);
        }

        public void OnDialogAction(DialogBase originNode)
        {
            switch(originNode.Data.ToString())
            {
                case "auto_fire": _autoFire = true; OnGameDataUpdate(); break;
                case "auto_fire_cancel": _autoFire = false; break;
                case "auto_fire_missile": _autoFireMissile = true; OnGameDataUpdate(); break;
                case "auto_fire_missile_cancel": _autoFireMissile = false; break;

                case "jump": if (_jumpState < JumpState.MANUAL) _jumpState = JumpState.MANUAL; EmergencyJump(); break;
                case "auto_jump": if (_jumpState < JumpState.AUTO) _jumpState = JumpState.AUTO; EmergencyJump(); break;
                case "emergency_jump": if (_jumpState < JumpState.EMERGENCY) _jumpState = JumpState.EMERGENCY; EmergencyJump(); break;
                case "auto_jump_cancel": _jumpState = JumpState.NONE; break;
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
                if (_jumpState == JumpState.EMERGENCY)
                {
                    Interactor.ExecuteAction(GameAction.ENGAGE_JUMP_DRIVE_MAXIMUM_RANGE);
                    SpeechEngine.Say(_dialg_EmergencyJump);
                }
                else
                {
                    Interactor.ExecuteAction(GameAction.ENGAGE_JUMP_DRIVE);
                    SpeechEngine.Say(_dialg_Jump);
                }

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
            if (
                (TargetShipData.ThreatLevel > ThreadLevelState.LOW) &&
                (PlayerShipData.Mtds == PlayerShipData.MTDSState.LOCKED) &&
                ((DateTime.Now - _lastTimeFired).TotalMilliseconds >= 1000)
            )
            {
                Interactor.ExecuteAction(GameAction.FIRE_PRIMARY_WEAPON, 500);
                _lastTimeFired = DateTime.Now;
            }
        }


        /// <summary> Fires a missile.
        /// </summary>
        private void FireMissile()
        {
            if (
                (TargetShipData.ThreatLevel > ThreadLevelState.LOW) &&
                (PlayerShipData.MissileLock == PlayerShipData.MissileState.LOCKED) &&
                (PlayerShipData.SecWeapon[0] != null)
            )
            {
                Interactor.ExecuteAction(GameAction.FIRE_SECONDARY_WEAPON);
                _autoFireMissile = false;
            }
        }
        #endregion
    }
}
