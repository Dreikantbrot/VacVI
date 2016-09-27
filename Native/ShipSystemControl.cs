using VacVI;
using VacVI.Dialog;
using VacVI.Database;
using VacVI.Input;
using VacVI.Plugins;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Native
{
    public class ShipSystemControl : IPlugin
    {
        #region Constants
        const string VI_COMMAND_ACK = "$(Aye aye|Acknowledged|Sure)";
        const int MIN_JUMP_ALTITUDE = 360000;

        const string PARAM_AUTO_EJ_ENABLED = "Enable Auto Emergency Jump (Auto EJ)";
        const string PARAM_AUTO_EJ_HULL_INTEGRITY = "Auto EJ - Min. Hull Integrity";
        const string PARAM_AUTO_EJ_MIN_FUEL = "Auto EJ - Min. Fuel";
        const string PARAM_AUTO_EJ_MIN_HOSTILES = "Auto EJ - Min. Hostile Count";

        const string DIALOG_SYSTEM_LOCATION = "We are currently in \"{0}\" < --> - > located at sector coordinates {1}, {2}, {3}.";
        #endregion


        #region Enums
        private enum JumpState
        {
            NONE = 0,
            MANUAL = 1,
            AUTO = 2,
            EMERGENCY = 3,
            AUTO_EMERGENCY = 4
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
        private string _currRecognizedPhrase;

        private bool _autoEJ_enabled = false;
        private int _autoEJ_hullThreshold = -1;
        private int _autoEJ_minFuelThreshold = -1;
        private int _autoEJ_minHostileCountThreshold = -1;

        private DialogVI _dialg_Jump = new DialogVI("Initiating $[fulcrum ]jump;Jumping...");
        private DialogVI _dialg_EmergencyJump = new DialogVI("I'm getting us out of here!;Let's get out of here!");
        private DialogVI _dialg_EmergencyJumpAuto = new DialogVI("This is $[getting ]too dangerous. $[I'm ]$(initiating|engaging) emergency jump.");

        private DialogVI _dialg_SystemLocation = new DialogVI(DIALOG_SYSTEM_LOCATION);
        #endregion


        #region Variables (Parameters)
        string[] _autoEJ_minHullThreshold_params = new string[98];
        string[] _autoEJ_minFuelThreshold_params = new string[201];
        string[] _autoEJ_minHostileCountThreshold_params = new string[23];
        #endregion


        #region Properties
        private bool playerIsInAtmosphere
        {
            get { return (PlayerShipData.Altitude > 0) && (PlayerShipData.Altitude <= MIN_JUMP_ALTITUDE); }
        }
        #endregion


        #region Constructor
        public ShipSystemControl()
        {
            for (int i = 0; i < _autoEJ_minHullThreshold_params.Length; i++)
            {
                _autoEJ_minHullThreshold_params[i] = (i + 1).ToString();
            }

            for (int i = 0; i < _autoEJ_minFuelThreshold_params.Length; i++)
            {
                _autoEJ_minFuelThreshold_params[i] = ((i == 0) ? 1 : (i * 5)).ToString();
            }

            for (int i = 0; i < _autoEJ_minHostileCountThreshold_params.Length; i++)
            {
                _autoEJ_minHostileCountThreshold_params[i] = (i + 1).ToString();
            }
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
            get { return Guid.Parse("20366fc6-4358-49a8-8a52-eed36ac2cb03"); }
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

        public System.Drawing.Bitmap LogoImage
        {
            get { return Properties.Resources.ShipSystemControl; }
        }
        #endregion


        #region Interface Functions
        public List<PluginParameterDefault> GetDefaultPluginParameters()
        {
            List<PluginParameterDefault> parameters = new List<PluginParameterDefault>();
            parameters.Add(new PluginParameterDefault(
                PARAM_AUTO_EJ_ENABLED,

                "When certain conditions are fulfilled (see \"Auto EJ\" options), the VI will attempt an automatic " +
                "emergency jump to save the ship. All \"Auto EJ\" conditions must be fulfilled for the jump to be triggered.\n" +
                "Set to \"True\" to enable or to \"False\" to disable this feature.",

                "False",
                new string[] { "True", "False" }
            ));

            parameters.Add(new PluginParameterDefault(
                PARAM_AUTO_EJ_HULL_INTEGRITY,
                "Determines the value the hull integrity must reach or fall below to trigger an auto emergency jump.",
                "25",
                _autoEJ_minHullThreshold_params
            ));

            parameters.Add(new PluginParameterDefault(
                PARAM_AUTO_EJ_MIN_FUEL,
                "Determines the minimum amount of fuel that must be available to trigger an auto emergency jump.",
                "100",
                _autoEJ_minFuelThreshold_params
            ));

            parameters.Add(new PluginParameterDefault(
                PARAM_AUTO_EJ_MIN_HOSTILES,
                "Determines how many hostiles (at least) must be visible on radar to trigger an auto emergency jump.",
                "8",
                _autoEJ_minHostileCountThreshold_params
            ));

            return parameters;
        }

        public void Initialize()
        {
            /* Set auto-emergency jump values */
            _autoEJ_enabled = PluginManager.PluginFile.ValueIsBoolAndTrue(this.Id.ToString(), PARAM_AUTO_EJ_ENABLED);

            if (
                (_autoEJ_minHullThreshold_params.Contains(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_AUTO_EJ_HULL_INTEGRITY))) &&
                (_autoEJ_minFuelThreshold_params.Contains(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_AUTO_EJ_MIN_FUEL))) &&
                (_autoEJ_minHostileCountThreshold_params.Contains(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_AUTO_EJ_MIN_HOSTILES)))
            )
            {
                Int32.TryParse(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_AUTO_EJ_HULL_INTEGRITY), out _autoEJ_hullThreshold);
                Int32.TryParse(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_AUTO_EJ_MIN_FUEL), out _autoEJ_minFuelThreshold);
                Int32.TryParse(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_AUTO_EJ_MIN_HOSTILES), out _autoEJ_minHostileCountThreshold);
            }
            else
            {
                // Invalid value (plugins.ini file manipulated by hand) - Deactivate feature
                _autoEJ_enabled = false;
            }


            /* Subscribe to events */
            SpeechEngine.OnVISpeechRecognized += SpeechEngine_OnVISpeechRecognized;
        }

        public void BuildDialogTree()
        {
            DialogTreeBranch[] dialog = new DialogTreeBranch[] {

                #region Auto-Fire
                new DialogTreeBranch(
                    new DialogPlayer(
                        "$[Auto ]fire;$(Fire|Shoot) $(as soon as|when) you have a lock on!", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "I'll light'em up, $(when|as soon as) I can!", 
                            DialogBase.DialogPriority.NORMAL, 
                            () => { return (TargetShipData.ThreatLevel > ThreatLevelState.LOW); }, 
                            this.Id.ToString(), 
                            "auto_fire"
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[I'm sorry but |Sorry but ]I won't shoot at a friendly target.", 
                            DialogBase.DialogPriority.HIGH, 
                            () => { return (TargetShipData.ThreatLevel <= ThreatLevelState.LOW); }, 
                            this.Id.ToString(), 
                            "auto_fire_cancel"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "Stop $[Auto-|automatically ]$(firing|shooting)!;Manual fire!", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$(Aye aye|Got it) - manual fire.", 
                            DialogBase.DialogPriority.NORMAL, 
                            null, 
                            this.Id.ToString(), 
                            "auto_fire_cancel"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "$(Fire|Launch|Shoot) a missile$[ as soon as possible| asap]!", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[I'm sorry but |Sorry but ]I won't shoot at a friendly target.", 
                            DialogBase.DialogPriority.HIGH, 
                            () => { return (TargetShipData.ThreatLevel <= ThreatLevelState.LOW); }, 
                            this.Id.ToString(), 
                            "auto_fire_missile_cancel"
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[I'm |I am ]already on it!", 
                            DialogBase.DialogPriority.HIGH, 
                            () => { return (_autoFireMissile); }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Give me a lock-on and I'll shoot.", 
                            DialogBase.DialogPriority.NORMAL, 
                            null, 
                            this.Id.ToString(), 
                            "auto_fire_missile"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "Cancel missile launch!", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$(Aye aye|Got it) - cancelling missile launch...", 
                            DialogBase.DialogPriority.NORMAL, 
                            null, 
                            this.Id.ToString(), 
                            "auto_fire_missile_cancel"
                        )
                    )
                ),
                #endregion

                #region Autopilot
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Form on target", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[Sorry - but ]form on what target exactly?", 
                            DialogBase.DialogPriority.HIGH, 
                            () => { return String.IsNullOrWhiteSpace(TargetShipData.Description); }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            VI_COMMAND_ACK, 
                            DialogBase.DialogPriority.NORMAL, 
                            () => { return (PlayerShipData.Autopilot != PlayerShipData.AutopilotState.FORM_ON_TARGET); },
                            this.Id.ToString(), 
                            "form_on_target"
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[I'm |I am ]already on it!", 
                            DialogBase.DialogPriority.NORMAL, 
                            () => { return (PlayerShipData.Autopilot == PlayerShipData.AutopilotState.FORM_ON_TARGET); }
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "Break the formation!", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Breaking formation...", 
                            DialogBase.DialogPriority.NORMAL, 
                            () => { return (PlayerShipData.Autopilot == PlayerShipData.AutopilotState.FORM_ON_TARGET); },
                            this.Id.ToString(), 
                            "form_on_target"
                        )
                    )
                ),
                
                // TODO: NAV_AUTOPILOT doesn't work for some reason...
                //new DialogTreeBranch(
                //    new DialogPlayer(
                //        "Enable autopilot1;Take the wheel!", 
                //        DialogBase.DialogImportance.CRITICAL
                //    ),
                //    new DialogTreeBranch(
                //        new DialogVI(
                //            VI_COMMAND_ACK, 
                //            DialogBase.DialogImportance.NORMAL, 
                //            () => { return (PlayerShipData.Autopilot == PlayerShipData.AutopilotState.OFF); },
                //            this.Id.ToString(), 
                //            "fly_to_nav"
                //        )
                //    ),
                //    new DialogTreeBranch(
                //        new DialogVI(
                //            "$[I'm |I am ]already on it!", 
                //            DialogBase.DialogImportance.NORMAL, 
                //            () => { return (PlayerShipData.Autopilot != PlayerShipData.AutopilotState.OFF); }
                //        )
                //    )
                //),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Manual control", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            String.Format("You already are in control $[, {0}].", VI.PlayerPhoneticName), 
                            DialogBase.DialogPriority.NORMAL, 
                            () => { return (PlayerShipData.Autopilot == PlayerShipData.AutopilotState.OFF); }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            String.Format("You have the controls;Passing controls back to you $[, {0}].", VI.PlayerPhoneticName),
                            DialogBase.DialogPriority.NORMAL, 
                            () => { return (PlayerShipData.Autopilot != PlayerShipData.AutopilotState.OFF); },
                            this.Id.ToString(), 
                            "autopilot_off"
                        )
                    )
                ),
                #endregion

                #region Jumping
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Initiate $[fulcrum ]jump!", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "I wouldn't jump from within a planet's atmosphere...", 
                            DialogBase.DialogPriority.VERY_HIGH, 
                            () => { return playerIsInAtmosphere; }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[Sorry - but] $(we're unable|we don't have enough energy) to jump." + ";" + 
                            "$[Sorry - but] Energy levels are insufficient for a jump.", 
                            DialogBase.DialogPriority.NORMAL, 
                            () => { return (PlayerShipData.EnergyLevel < 100); }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            VI_COMMAND_ACK, 
                            DialogBase.DialogPriority.NORMAL, 
                            () => { return (PlayerShipData.EnergyLevel >= 100); }, 
                            this.Id.ToString(), 
                            "jump"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "$[Initiate ]auto-jump" + ";" + 
                        "Jump $(as soon as possible|asap|when you can|when possible)!", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            VI_COMMAND_ACK + " - $(I'll jump|Auto-jumping|Jumping) to the nav point $(as soon as|when) possible!", 
                            DialogBase.DialogPriority.NORMAL, 
                            null, 
                            this.Id.ToString(), 
                            "auto_jump"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "$[Initiate ]emergency jump!" + ";" + 
                        "Get $(me|us) out of here!", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            String.Format(
                                "We need to get out of the atmosphere first!" + ";" +
                                "Not while we are within a planet's atmosphere!" + ";" + 
                                "Jumping from within the atmosphere is too dangerous! You need to get us out of there first" + 
                                "$[, <{0}-->{1}>]!", 
                                VI.PlayerName,
                                VI.PlayerPhoneticName
                            ),
                            DialogBase.DialogPriority.VERY_HIGH, 
                            () => { return playerIsInAtmosphere; }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            VI_COMMAND_ACK + " - let's get out of here!", 
                            DialogBase.DialogPriority.HIGH, 
                            () => { return (PlayerShipData.EnergyLevel <= 60); }, 
                            this.Id.ToString(), 
                            "emergency_jump"
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            VI_COMMAND_ACK, 
                            DialogBase.DialogPriority.NORMAL, 
                            null, 
                            this.Id.ToString(), 
                            "emergency_jump"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("Cancel $[auto-]jump", DialogBase.DialogPriority.CRITICAL),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[" + VI_COMMAND_ACK + " - ]$(Cancelling jump...|Jump cancelled.)",
                            DialogBase.DialogPriority.NORMAL, 
                            null, 
                            this.Id.ToString(), 
                            "auto_jump_cancel"
                        )
                    )
                ),
                #endregion

                #region Thruster
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Full Stop", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$[Aye aye -|Got it -] Full stop!", 
                            DialogBase.DialogPriority.NORMAL, 
                            null, 
                            this.Id.ToString(), 
                            "full_stop"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "$(Enable|Activate) $(inertial dampening system|IDS).", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI("$(Aye aye|Got it).", 
                            DialogBase.DialogPriority.NORMAL, 
                            null, 
                            this.Id.ToString(), 
                            "toggle_ids"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "$[Set ]IDS $[Multiplier |Factor ]to " + 
                        "$(1|2|3|4|5" + ((GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? "|6|7|8|9" : "") + ")!", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI("$(Aye aye|Got it).", 
                            DialogBase.DialogPriority.NORMAL, 
                            null, 
                            this.Id.ToString(), 
                            "set_ids_multiplier"
                        )
                    )
                ),
                #endregion
                
                #region Consoles
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Open navigation console!",
                        DialogBase.DialogPriority.CRITICAL,
                        () => { return (HudData.NavigationConsole == OnOffState.OFF); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye.",
                            DialogBase.DialogPriority.NORMAL,
                            null, 
                            this.Id.ToString(),
                            "toggle_nav_console"
                        )
                    )
                ),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Close navigation console!", 
                        DialogBase.DialogPriority.CRITICAL,
                        () => { return (HudData.NavigationConsole == OnOffState.ON); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye.",
                            DialogBase.DialogPriority.NORMAL,
                            null,
                            this.Id.ToString(), 
                            "toggle_nav_console"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "Open build console!",
                        DialogBase.DialogPriority.CRITICAL,
                        () => { return ((GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) && HudData.BuildConsole == OnOffState.OFF); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye.",
                            DialogBase.DialogPriority.NORMAL,
                            null,
                            this.Id.ToString(),
                            "toggle_build_console"
                        )
                    )
                ),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Close build console!",
                        DialogBase.DialogPriority.CRITICAL, 
                        () => { return ((GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) && HudData.BuildConsole == OnOffState.ON); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye.",
                            DialogBase.DialogPriority.NORMAL, 
                            null,
                            this.Id.ToString(),
                            "toggle_build_console"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "Open inventory console!",
                        DialogBase.DialogPriority.CRITICAL, 
                        () => { return (HudData.InventoryConsole == OnOffState.OFF); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye.",
                            DialogBase.DialogPriority.NORMAL,
                            null,
                            this.Id.ToString(), 
                            "toggle_inventory_console"
                        )
                    )
                ),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Close inventory console!",
                        DialogBase.DialogPriority.CRITICAL, 
                        () => { return (HudData.InventoryConsole == OnOffState.ON); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye.",
                            DialogBase.DialogPriority.NORMAL, 
                            null, 
                            this.Id.ToString(), 
                            "toggle_inventory_console"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "Open trade console!", 
                        DialogBase.DialogPriority.CRITICAL, 
                        () => { return (HudData.TradeConsole == OnOffState.OFF); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye.", 
                            DialogBase.DialogPriority.NORMAL, 
                            null, 
                            this.Id.ToString(), 
                            "toggle_trade_console"
                        )
                    )
                ),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Close trade console!", 
                        DialogBase.DialogPriority.CRITICAL, 
                        () => { return (HudData.TradeConsole == OnOffState.ON); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye.",
                            DialogBase.DialogPriority.NORMAL, 
                            null, 
                            this.Id.ToString(), 
                            "toggle_trade_console"
                        )
                    )
                ),
                #endregion

                #region HUD
                new DialogTreeBranch(
                    new DialogPlayer(
                        "$(Switch to|Display) $(partial|full) HUD!;Set Hud to $(partial|full)$[ mode]!",
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye.",
                            DialogBase.DialogPriority.NORMAL,
                            null, 
                            this.Id.ToString(),
                            "toggle_HUD"
                        )
                    )
                ),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Enable HUD!;Turn on the HUD!;Turn the HUD $[back ]on!",
                        DialogBase.DialogPriority.CRITICAL,
                        () => { return (HudData.Hud != HudData.HudStatus.OFF); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye.",
                            DialogBase.DialogPriority.NORMAL,
                            null, 
                            this.Id.ToString(),
                            "toggle_HUD_full"
                        )
                    )
                ),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Disable HUD!;Turn off the HUD!",
                        DialogBase.DialogPriority.CRITICAL,
                        () => { return (HudData.Hud != HudData.HudStatus.OFF); }
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye.",
                            DialogBase.DialogPriority.NORMAL,
                            null, 
                            this.Id.ToString(),
                            "disable_HUD"
                        )
                    )
                ),
                #endregion

                #region Target and Ship Information
                new DialogTreeBranch(
                    new DialogPlayer(
                        "$[Give me a ]$(Damage|Status|Ship Status) report!;Ship status!",
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye!", 
                            DialogBase.DialogPriority.NORMAL, 
                            null, 
                            null,
                            null,
                            DialogBase.DialogFlags.NONE,
                            true
                        ),
                        new DialogTreeBranch(
                            new DialogCommand(
                                "Status Report",
                                DialogBase.DialogPriority.NORMAL,
                                null,
                                this.Id.ToString(),
                                "status_report"
                            )
                        )
                    )
                ),
                
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Target Status!", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "Aye aye.", 
                            DialogBase.DialogPriority.NORMAL, 
                            () => { return !String.IsNullOrWhiteSpace(TargetShipData.Description); }, 
                            this.Id.ToString(), 
                            "target_status",
                            DialogBase.DialogFlags.NONE,
                            true
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "No target selected!",
                            DialogBase.DialogPriority.NORMAL,
                            () => { return String.IsNullOrWhiteSpace(TargetShipData.Description); }
                        )
                    )
                ),
                #endregion
            
                #region Enviornment Information
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Where are we?" + ";" + 
                        "$(What's|What is) our current location?", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(
                        new DialogCommand(
                            "Give info about the current location",
                            DialogBase.DialogPriority.NORMAL,
                            null,
                            this.Id.ToString(),
                            "local_system_info"
                        )
                    )
                ),
                #endregion
            };

            DialogTreeBuilder.BuildDialogTree(null, dialog);
        }

        public void OnDialogAction(DialogBase originNode)
        {
            switch (originNode.Data.ToString())
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
                    _jumpState = JumpState.MANUAL;
                    emergencyJump();
                    break;
                case "auto_jump":
                    _jumpState = JumpState.AUTO;
                    emergencyJump();
                    break;
                case "emergency_jump":
                    _jumpState = JumpState.EMERGENCY;
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
                case "toggle_ids":
                    Interactor.ExecuteAction(GameAction.INERTIAL_SYSTEM_ON_OFF);
                    break;
                case "set_ids_multiplier":
                    Int32.TryParse(_currRecognizedPhrase.Substring(_currRecognizedPhrase.Length - 1), out _targetIdsMultiplier);
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
                        _currRecognizedPhrase.Contains("full") ? (int)HudData.HudStatus.FULL :
                        _currRecognizedPhrase.Contains("partial") ? (int)HudData.HudStatus.PARTIAL :
                        -1
                    );
                    updateHUDMode();
                    break;
                case "toggle_HUD_full":
                    _targetHudMode = (int)HudData.HudStatus.FULL;
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

                #region Enviornment Information
                case "local_system_info":
                    _dialg_SystemLocation.RawText = String.Format(
                        DIALOG_SYSTEM_LOCATION,
                        EnvironmentalData.LocalSystemName,
                        PlayerShipData.SectorPosition.X,
                        PlayerShipData.SectorPosition.Y,
                        PlayerShipData.SectorPosition.Z
                    );
                    SpeechEngine.Say(_dialg_SystemLocation);
                    break;
                #endregion
            }
        }

        public void OnGameDataUpdate()
        {
            if (
                (_autoEJ_enabled) &&
                (_jumpState < JumpState.AUTO) &&
                (PlayerShipData.FuelRemaining >= _autoEJ_minFuelThreshold) &&
                (PlayerShipData.HullIntegrity <= _autoEJ_hullThreshold) &&
                (HudData.TotalHostilesOnRadar >= _autoEJ_minHostileCountThreshold)
            )
            { _jumpState = JumpState.AUTO_EMERGENCY; }

            if (
                (_jumpState == JumpState.AUTO) ||
                (_jumpState == JumpState.EMERGENCY) ||
                (_jumpState == JumpState.AUTO_EMERGENCY)
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

        public void OnProgramShutdown()
        {

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
                switch(_jumpState)
                {
                    case JumpState.AUTO_EMERGENCY:
                        if (Interactor.ExecuteAction(GameAction.ENGAGE_JUMP_DRIVE_MAXIMUM_RANGE)) { SpeechEngine.Say(_dialg_EmergencyJumpAuto); }
                        break;
                    
                    case JumpState.EMERGENCY:
                        if (Interactor.ExecuteAction(GameAction.ENGAGE_JUMP_DRIVE_MAXIMUM_RANGE)) { SpeechEngine.Say(_dialg_EmergencyJump); }
                        break;
                    
                    default:
                        if (Interactor.ExecuteAction(GameAction.ENGAGE_JUMP_DRIVE)) { SpeechEngine.Say(_dialg_Jump); }
                        break;
                }

                _jumpState = JumpState.NONE;
            }
        }


        /// <summary> Fires all active primary weapons for 1 second.
        /// </summary>
        private void fire()
        {
            if (
                (TargetShipData.ThreatLevel > ThreatLevelState.LOW) &&
                (
                    (GameMeta.CurrentGame == GameMeta.SupportedGame.EVOCHRON_LEGACY) || // <-- DIRTY FIX! - In ELGCY, the MDTS state does not seem to get updated. Guess we'll have to wait for Vice to fix that.
                    (PlayerShipData.Mtds == PlayerShipData.MTDSState.LOCKED)
                ) &&
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
                (TargetShipData.ThreatLevel > ThreatLevelState.LOW) &&
                (PlayerShipData.MissileLock == PlayerShipData.MissileState.LOCKED) &&
                (PlayerShipData.SecWeapon[0] != null)
            )
            {
                if (Interactor.ExecuteAction(GameAction.FIRE_SECONDARY_WEAPON)) { _autoFireMissile = false; }
            }
        }


        /// <summary> Sets the autopilot to match the target mode.
        /// </summary>
        private void updateAutpilot()
        {
            if (_targetAutopilotState == (int)PlayerShipData.Autopilot) { _targetAutopilotState = -1; }
            if (_targetAutopilotState < 0) { return; }

            switch (_targetAutopilotState)
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
                "Engines at " + PlayerShipData.EngineDamage + "%.\n" +
                "Navigations at " + PlayerShipData.NavDamage + "%.\n" +
                "Weapons at " + PlayerShipData.WeaponDamage + "%.\n" +
                "Shields at " + PlayerShipData.ShieldStatus[ShieldLevelState.TOTAL] + "%.\n" +
                "Hull Integrity at " + PlayerShipData.HullIntegrity + "%.\n" +
                Math.Round((double)PlayerShipData.FuelPercentage * 100) + "% fuel remaining."
            );
        }


        /// <summary> Creates a string with target information.
        /// </summary>
        /// <returns>A dialog string for the VI to speak.</returns>
        private string getTargetInformation()
        {
            return (
                "Target: " + TargetShipData.Description + ".\n" +
                "Faction: " + TargetShipData.Faction + ".\n" +
                "Target Shields at " + TargetShipData.ShieldStatus[ShieldLevelState.TOTAL] + "%.\n" +
                "Hull Integrity at " + TargetShipData.DamageLevel + "%.\n" +
                "Threat Level: " + (
                    (TargetShipData.ThreatLevel == ThreatLevelState.LOW) ? "Low" :
                    (TargetShipData.ThreatLevel == ThreatLevelState.MED) ? "Medium" :
                    "High"
                ) + "."
            );
        }
        #endregion


        #region Events
        void SpeechEngine_OnVISpeechRecognized(SpeechEngine.VISpeechRecognizedEventArgs obj)
        {
            _currRecognizedPhrase = obj.RecognizedPhrase;
        }
        #endregion
    }
}
