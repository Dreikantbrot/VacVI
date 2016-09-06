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
        const int MIN_JUMP_ALTITUDE = 360000;
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
        private int _targetIdsMultiplier = -1;
        private int _targetHudMode = -1;
        private int _targetAutopilotState = -1;

        private DialogVI _dialg_Jump = new DialogVI("Initiating $[fulcrum ]jump;Jumping...");
        private DialogVI _dialg_EmergencyJump = new DialogVI("I'm getting us out of here!;Let's get out of here!");
        #endregion


        #region Properties
        private bool playerIsInAtmosphere
        {
            get { return (PlayerShipData.Altitude > 0) && (PlayerShipData.Altitude <= MIN_JUMP_ALTITUDE); }
        }
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

                #region Auto-Fire
                new DialogTreeBranch(
                    new DialogPlayer(
                        "$[Auto ]fire;$(fire|shoot) when you have a lock on", 
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "I'll light'em up, $(when|as soon as) I can", 
                            DialogBase.DialogImportance.NORMAL, 
                            () => { return (TargetShipData.ThreatLevel > ThreadLevelState.LOW); }, 
                            this.Name, 
                            "auto_fire"
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[I'm sorry but|Sorry but] I won't shoot at a friendly target.", 
                            DialogBase.DialogImportance.HIGH, 
                            () => { return (TargetShipData.ThreatLevel <= ThreadLevelState.LOW); }, 
                            this.Name, 
                            "auto_fire_cancel"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "Stop $[Auto-|automatically ]$(firing|shooting);Manual fire", 
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$(Aye aye|Got it) - manual fire", 
                            DialogBase.DialogImportance.NORMAL, 
                            null, 
                            this.Name, 
                            "auto_fire_cancel"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "$(fire|launch|shoot) a missile $[as soon as possible|asap]", 
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[I'm sorry but|Sorry but] I won't shoot at a friendly target.", 
                            DialogBase.DialogImportance.HIGH, 
                            () => { return (TargetShipData.ThreatLevel <= ThreadLevelState.LOW); }, 
                            this.Name, 
                            "auto_fire_missile_cancel"
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[I'm|I am] already on it!", 
                            DialogBase.DialogImportance.HIGH, 
                            () => { return (_autoFireMissile); }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Give me a lock-on and I'll shoot", 
                            DialogBase.DialogImportance.NORMAL, 
                            null, 
                            this.Name, 
                            "auto_fire_missile"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "Cancel missile launch", 
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$(Aye aye|Got it) - cancelling missile launch", 
                            DialogBase.DialogImportance.NORMAL, 
                            null, 
                            this.Name, 
                            "auto_fire_missile_cancel"
                        )
                    )
                ),
                #endregion

                #region Autopilot
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Form on target", 
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[Sorry - but] form on what target exactly?", 
                            DialogBase.DialogImportance.HIGH, 
                            () => { return String.IsNullOrWhiteSpace(TargetShipData.Description); }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            VI_COMMAND_ACK, 
                            DialogBase.DialogImportance.NORMAL, 
                            () => { return (PlayerShipData.Autopilot != PlayerShipData.AutopilotState.FORM_ON_TARGET); },
                            this.Name, 
                            "form_on_target"
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "I'm already on it", 
                            DialogBase.DialogImportance.NORMAL, 
                            () => { return (PlayerShipData.Autopilot == PlayerShipData.AutopilotState.FORM_ON_TARGET); }
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "Break the formation", 
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Breaking formation", 
                            DialogBase.DialogImportance.NORMAL, 
                            () => { return (PlayerShipData.Autopilot == PlayerShipData.AutopilotState.FORM_ON_TARGET); },
                            this.Name, 
                            "form_on_target"
                        )
                    )
                ),
                
                // TODO: NAV_AUTOPILOT doesn't work for some reason...
                //new DialogTreeBranch(
                //    new DialogPlayer(
                //        "Enable autopilot;Take the wheel", 
                //        DialogBase.DialogImportance.CRITICAL
                //    ),
                //    new DialogTreeBranch(
                //        new DialogVI(
                //            VI_COMMAND_ACK, 
                //            DialogBase.DialogImportance.NORMAL, 
                //            () => { return (PlayerShipData.Autopilot == PlayerShipData.AutopilotState.OFF); },
                //            this.Name, 
                //            "fly_to_nav"
                //        )
                //    ),
                //    new DialogTreeBranch(
                //        new DialogVI(
                //            "I'm already on it", 
                //            DialogBase.DialogImportance.NORMAL, 
                //            () => { return (PlayerShipData.Autopilot != PlayerShipData.AutopilotState.OFF); }
                //        )
                //    )
                //),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Manual control", 
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            String.Format("You already are in control $[, {0}]", VI.PlayerPhoneticName), 
                            DialogBase.DialogImportance.NORMAL, 
                            () => { return (PlayerShipData.Autopilot == PlayerShipData.AutopilotState.OFF); }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            String.Format("You have the controls;Passing controls back to you $[, {0}]", VI.PlayerPhoneticName),
                            DialogBase.DialogImportance.NORMAL, 
                            () => { return (PlayerShipData.Autopilot != PlayerShipData.AutopilotState.OFF); },
                            this.Name, 
                            "autopilot_off"
                        )
                    )
                ),
                #endregion

                #region Jumping
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Initiate $[fulcrum ]jump", 
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "I wouldn't jump from within a planet's atmosphere", 
                            DialogBase.DialogImportance.VERY_HIGH, 
                            () => { return playerIsInAtmosphere; }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[Sorry - but] $(we're unable|we don't have enough energy) to jump", 
                            DialogBase.DialogImportance.NORMAL, 
                            () => { return (PlayerShipData.EnergyLevel < 100); }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            VI_COMMAND_ACK, 
                            DialogBase.DialogImportance.NORMAL, 
                            () => { return (PlayerShipData.EnergyLevel >= 100); }, 
                            this.Name, 
                            "jump"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "$[Initiate ]auto-jump;Jump $(as soon as possible|asap)", 
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            VI_COMMAND_ACK + " - I'll jump when possible", 
                            DialogBase.DialogImportance.NORMAL, 
                            null, 
                            this.Name, 
                            "auto_jump"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "$[Initiate ]emergency jump;Get $(me|us) out of here", 
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "We need to get out of the atmosphere first!", 
                            DialogBase.DialogImportance.VERY_HIGH, 
                            () => { return playerIsInAtmosphere; }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            VI_COMMAND_ACK + " - let's get out of here!", 
                            DialogBase.DialogImportance.HIGH, 
                            () => { return (PlayerShipData.EnergyLevel <= 60); }, 
                            this.Name, 
                            "emergency_jump"
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            VI_COMMAND_ACK, 
                            DialogBase.DialogImportance.NORMAL, 
                            null, 
                            this.Name, 
                            "emergency_jump"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("Cancel $[auto-]jump", DialogBase.DialogImportance.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI(
                            VI_COMMAND_ACK + " - Cancelling jump;Jump cancelled", 
                            DialogBase.DialogImportance.NORMAL, 
                            null, 
                            this.Name, 
                            "auto_jump_cancel"
                        )
                    )
                ),
                #endregion

                #region Thruster
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Full Stop", 
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[Aye aye -|Got it -] Full stop", 
                            DialogBase.DialogImportance.NORMAL, 
                            null, 
                            this.Name, 
                            "full_stop"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "$[Set ]IDS $[Multiplier|Factor ]to $(1|2|3|4|5)", 
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI("$(Aye aye|Got it)", 
                            DialogBase.DialogImportance.NORMAL, 
                            null, 
                            this.Name, 
                            "set_ids_multiplier"
                        )
                    )
                ),
                #endregion
                
                #region Consoles
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Open navigation console",
                        DialogBase.DialogImportance.CRITICAL,
                        () => { return (HudData.NavigationConsole == OnOffState.OFF); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye",
                            DialogBase.DialogImportance.NORMAL,
                            null, 
                            this.Name,
                            "toggle_nav_console"
                        )
                    )
                ),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Close navigation console", 
                        DialogBase.DialogImportance.CRITICAL,
                        () => { return (HudData.NavigationConsole == OnOffState.ON); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye",
                            DialogBase.DialogImportance.NORMAL,
                            null,
                            this.Name, 
                            "toggle_nav_console"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "Open build console",
                        DialogBase.DialogImportance.CRITICAL,
                        () => { return ((GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) && HudData.BuildConsole == OnOffState.OFF); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye",
                            DialogBase.DialogImportance.NORMAL,
                            null,
                            this.Name,
                            "toggle_build_console"
                        )
                    )
                ),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Close build console",
                        DialogBase.DialogImportance.CRITICAL, 
                        () => { return ((GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) && HudData.BuildConsole == OnOffState.ON); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye",
                            DialogBase.DialogImportance.NORMAL, 
                            null,
                            this.Name,
                            "toggle_build_console"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "Open inventory console",
                        DialogBase.DialogImportance.CRITICAL, 
                        () => { return (HudData.InventoryConsole == OnOffState.OFF); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye",
                            DialogBase.DialogImportance.NORMAL,
                            null,
                            this.Name, 
                            "toggle_inventory_console"
                        )
                    )
                ),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Close inventory console",
                        DialogBase.DialogImportance.CRITICAL, 
                        () => { return (HudData.InventoryConsole == OnOffState.ON); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye",
                            DialogBase.DialogImportance.NORMAL, 
                            null, 
                            this.Name, 
                            "toggle_inventory_console"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "Open trade console", 
                        DialogBase.DialogImportance.CRITICAL, 
                        () => { return (HudData.TradeConsole == OnOffState.OFF); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye", DialogBase.DialogImportance.NORMAL, 
                            null, 
                            this.Name, 
                            "toggle_trade_console"
                        )
                    )
                ),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Close trade console", 
                        DialogBase.DialogImportance.CRITICAL, 
                        () => { return (HudData.TradeConsole == OnOffState.ON); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye", DialogBase.DialogImportance.NORMAL, 
                            null, 
                            this.Name, 
                            "toggle_trade_console"
                        )
                    )
                ),
                #endregion

                #region HUD
                new DialogTreeBranch(
                    new DialogPlayer(
                        "$(Switch to|Display) $(partial|full) HUD;Set Hud to $(partial|full) $[mode]",
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye",
                            DialogBase.DialogImportance.NORMAL,
                            null, 
                            this.Name,
                            "toggle_HUD"
                        )
                    )
                ),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Disable HUD;Turn off the HUD",
                        DialogBase.DialogImportance.CRITICAL,
                        () => { return (HudData.Hud != HudData.HudStatus.OFF); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye",
                            DialogBase.DialogImportance.NORMAL,
                            null, 
                            this.Name,
                            "disable_HUD"
                        )
                    )
                ),
                #endregion

                #region Target and Ship Information
                new DialogTreeBranch(
                    new DialogPlayer(
                        "$(Damage|Status|Ship Status) Report;Ship Status",
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye", 
                            DialogBase.DialogImportance.NORMAL, 
                            null, 
                            null,
                            null,
                            true
                        ),
                        new DialogTreeBranch(
                            new DialogCommand(
                                "Status Report",
                                DialogBase.DialogImportance.NORMAL,
                                null,
                                this.Name,
                                "status_report"
                            )
                        )
                    )
                ),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Target Status", 
                        DialogBase.DialogImportance.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye", 
                            DialogBase.DialogImportance.NORMAL, 
                            () => { return !String.IsNullOrWhiteSpace(TargetShipData.Description); }, this.Name, "target_status"
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "No target selected",
                            DialogBase.DialogImportance.NORMAL,
                            () => { return String.IsNullOrWhiteSpace(TargetShipData.Description); }
                        )
                    )
                ),
                #endregion

            };

            DialogTreeBuilder.BuildDialogTree(null, dialogOptions);
        }

        public void OnDialogAction(DialogBase originNode)
        {
            switch(originNode.Data.ToString())
            {
                #region Auto-Fire
                case "auto_fire": 
                    _autoFire = true; 
                    OnGameDataUpdate(); 
                    break;
                case "auto_fire_cancel": 
                    _autoFire = false; 
                    break;
                case "auto_fire_missile": 
                    _autoFireMissile = true; 
                    OnGameDataUpdate(); 
                    break;
                case "auto_fire_missile_cancel": 
                    _autoFireMissile = false; 
                    break;
                #endregion

                #region Auto-Pilot
                case "form_on_target":
                    _targetAutopilotState = (int)PlayerShipData.AutopilotState.FORM_ON_TARGET;
                    break;
                case "fly_to_nav":
                    _targetAutopilotState = (int)PlayerShipData.AutopilotState.FLY_TO_NAV_POINT;
                    break;
                case "autopilot_off":
                    _targetAutopilotState = (int)PlayerShipData.AutopilotState.OFF;
                    break;
                #endregion

                #region Jumping
                case "jump":
                    if (_jumpState < JumpState.MANUAL) { _jumpState = JumpState.MANUAL; }
                    emergencyJump(); 
                    break;
                case "auto_jump":
                    if (_jumpState < JumpState.AUTO) { _jumpState = JumpState.AUTO; }
                    emergencyJump(); 
                    break;
                case "emergency_jump":
                    if (_jumpState < JumpState.EMERGENCY) { _jumpState = JumpState.EMERGENCY; }
                    emergencyJump(); 
                    break;
                case "auto_jump_cancel": 
                    _jumpState = JumpState.NONE;
                    break;
                #endregion

                #region Thruster
                case "full_stop": 
                    Interactor.ExecuteAction(GameAction.ZERO_THROTTLE); 
                    break;
                case "set_ids_multiplier":
                    Int32.TryParse(VI.CurrRecognizedPhrase.Substring(VI.CurrRecognizedPhrase.Length - 1), out _targetIdsMultiplier);
                    updateIDSMultiplier();
                    break;
                #endregion

                #region Consoles
                case "toggle_nav_console":
                    Interactor.ExecuteAction(GameAction.NAVIGATION_CONSOLE);
                    break;
                case "toggle_build_console":
                    Interactor.ExecuteAction(GameAction.BUILD_AND_DEPLOY_CONSOLE);
                    break;
                case "toggle_inventory_console":
                    Interactor.ExecuteAction(GameAction.INVENTORY_CONSOLE);
                    break;
                case "toggle_trade_console":
                    Interactor.ExecuteAction(GameAction.TRADE_CONSOLE);
                    break;
                #endregion

                #region HUD
                case "toggle_HUD":
                    _targetHudMode = (
                        VI.CurrRecognizedPhrase.Contains("full") ? (int)HudData.HudStatus.FULL : 
                        VI.CurrRecognizedPhrase.Contains("partial") ? (int)HudData.HudStatus.PARTIAL :
                        -1
                    );
                    updateHUDMode();
                    break;
                case "disable_HUD":
                    _targetHudMode = (int)HudData.HudStatus.OFF;
                    updateHUDMode();
                    break;
                #endregion

                #region Target and Ship Information
                case "status_report":
                    SpeechEngine.Say(getShipStatus(), false);
                    break;
                case "target_status":
                    SpeechEngine.Say(getTargetInformation(), false);
                    break;
                #endregion
            }
        }

        public void OnGameDataUpdate()
        {
            if (
                (_jumpState == JumpState.AUTO) ||
                (_jumpState == JumpState.EMERGENCY)
            )
            {
                emergencyJump();
            }

            if (_autoFireMissile) { fireMissile(); }

            if (_autoFire) { fire(); }

            updateAutpilot();
            updateIDSMultiplier();
            updateHUDMode();
        }

        public List<PluginParameterDefault> GetDefaultPluginParameters()
        {
            return new List<PluginParameterDefault>();
        }
        #endregion


        #region Plugin Functions
        /// <summary> Initiates an (emergency) jump.
        /// </summary>
        private void emergencyJump()
        {
            if (playerIsInAtmosphere) { return; }

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
        }


        /// <summary> Fires all active primary weapons for 1 second.
        /// </summary>
        private void fire()
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
        private void fireMissile()
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


        /// <summary> Sets the autopilot to match the target mode.
        /// </summary>
        private void updateAutpilot()
        {
            if (_targetAutopilotState == (int)PlayerShipData.Autopilot) { _targetAutopilotState = -1; }
            if (_targetAutopilotState < 0) { return; }

            switch(_targetAutopilotState)
            {
                case (int)PlayerShipData.AutopilotState.FORM_ON_TARGET:
                    Interactor.ExecuteAction(GameAction.FORM_ON_TARGET);
                    break;

                case (int)PlayerShipData.AutopilotState.FLY_TO_NAV_POINT:
                    Interactor.ExecuteAction(GameAction.NAV_AUTOPILOT);
                    break;

                case (int)PlayerShipData.AutopilotState.OFF:
                    Interactor.ExecuteAction(GameAction.NAV_AUTOPILOT);
                    break;
            }
        }


        /// <summary> Sets the IDS multiplier to the given value.
        /// </summary>
        private void updateIDSMultiplier()
        {
            if (_targetIdsMultiplier <= 0) { return; }

            // Is the IDS multiplier already set to the desired mode?
            if ((_targetIdsMultiplier - PlayerShipData.IDSMultiplier) != 0)
            {
                Interactor.ExecuteAction(
                    ((_targetIdsMultiplier - PlayerShipData.IDSMultiplier) < 0) ?
                    GameAction.DECREASE_IDS_THOTTLE_SCALE :
                    GameAction.INCREASE_IDS_THOTTLE_SCALE
                );
            }
            else
            {
                _targetIdsMultiplier = -1;
            }
        }


        /// <summary> Sets the IDS multiplier to the given value.
        /// </summary>
        private void updateHUDMode()
        {
            if (_targetHudMode < 0) { return; }

            // Is the IDS multiplier already set to the desired mode?
            if ((HudData.Hud - _targetHudMode) != 0)
            {
                Interactor.ExecuteAction(GameAction.HUD_MODE);
            }
            else
            {
                _targetHudMode = -1;
            }
        }


        /// <summary> Creates a string with information about the player ship.
        /// </summary>
        /// <returns>A dialog string for the VI to speak.</returns>
        private string getShipStatus()
        {
            return (
                "Engines @" + PlayerShipData.EngineDamage + "%. " +
                "Navigations @" + PlayerShipData.NavDamage + "%. " +
                "Weapons @" + PlayerShipData.WeaponDamage + "%. " +
                "Shields @" + PlayerShipData.ShieldLevel[ShieldLevelState.TOTAL] + "%. " + 
                Math.Round((double)PlayerShipData.FuelPercentage * 100) + "% fuel remaining."
            );
        }


        /// <summary> Creates a string with target information.
        /// </summary>
        /// <returns>A dialog string for the VI to speak.</returns>
        private string getTargetInformation()
        {
            return (
                "Target: " + TargetShipData.Description + ", " +
                "Target Shields @" + TargetShipData.ShieldLevel[ShieldLevelState.TOTAL] + "%, " +
                "Faction: " + TargetShipData.Faction + ", " +
                "Hull Integrity " + TargetShipData.DamageLevel + "%, " +
                "Threat Level: " + (
                    (TargetShipData.ThreatLevel == ThreadLevelState.LOW) ? "Low" : 
                    (TargetShipData.ThreatLevel == ThreadLevelState.MED) ? "Medium" : 
                    "High"
                )
            );
        }
        #endregion
    }
}
