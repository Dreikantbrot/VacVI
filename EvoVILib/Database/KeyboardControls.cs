using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EvoVI.Database
{
    #region Public Enums
    public enum GameAction
    {
        TURN_RIGHT,
        TURN_LEFT,
        PITCH_DOWN,
        PITCH_UP,
        ROLL_RIGHT,
        ROLL_LEFT,
        STRAFE_RIGHT,
        STRAFE_LEFT,
        AFTERBURNER,
        INERTIAL_SYSTEM_ON_OFF,
        MATCH_TARGET_SPEED,
        FIRE_PRIMARY_WEAPON,
        FIRE_SECONDARY_WEAPON,
        SELECT_NEXT_SECONDARY_WEAPON,
        SELECT_PREVIOUS_SECONDARY_WEAPON,
        COUNTERMEASURES,
        TARGET_NEAREST_SHIP,
        TARGET_NEAREST_HOSTILE,
        TARGET_NEXT_SHIP,
        TARGET_NEXT_HOSTILE,
        BOOST_ENERGY_TO_SHIELDS,
        BOOST_ENERGY_TO_WEAPONS,
        HUD_MODE,
        MDTS_ON_OFF,
        TOGGLE_3RD_PERSON_VIEWS,
        PADLOCK_ON_OFF,
        FLY_BY_VIEW,
        CINEMATIC_MODE,
        VIEW_RIGHT,
        PAN_RIGHT,
        VIEW_LEFT,
        PAN_LEFT,
        VIEW_UP,
        PAN_UP,
        VIEW_BEHIND,
        PAN_DOWN,
        NAVIGATION_CONSOLE,
        ENGAGE_JUMP_DRIVE,
        ENGAGE_JUMP_DRIVE_MAXIMUM_RANGE,
        BUILD_AND_DEPLOY_CONSOLE,
        INVENTORY_CONSOLE,
        DOCKING_CONTROL_ON_OFF,
        TRADE_CONSOLE,
        STRAFE_UP,
        STRAFE_DOWN,
        TRACTOR_MINING_BEAM_ON_OFF,
        TRACTOR_MINING_BEAM_TOGGLE,
        TRACTOR_MINING_REPAIR_BEAM_ON_OFF,
        TRACTOR_MINING_REPAIR_BEAM_TOGGLE,
        REVERSE_ENGINE_THRUST,
        FORM_ON_TARGET,
        NAV_AUTOPILOT,
        TOGGLE_TARGET_DETAIL_AND_RANGED_LIST,
        TARGET_DISPLAY_MODE,
        QUIT,
        QUICK_SAVE,
        DELETE_MESSAGE,
        RECALL_MESSAGE,
        DELETE_ALL_MESSAGE_LINES,
        JETTISON_CARGO,
        INERTIAL_FORWARD,
        INERTIAL_REVERSE,
        TARGET_SHIP_IN_GUNSIGHT,
        THROTTLE_UP_SECONDARY_CONTROL,
        THROTTLE_UP,
        THROTTLE_DOWN_SECONDARY_CONTROL,
        THROTTLE_DOWN,
        AUGMENT_FRONT_SHIELD,
        AUGMENT_REAR_SHIELD,
        AUGMENT_RIGHT_SHIELD,
        AUGMENT_LEFT_SHIELD,
        EQUALIZE_SHIELDS,
        EJECT_AND_SELF_DESTRUCT_SHIP,
        NEAREST_HOSTILE_SECONDARY_CONTROL,
        NEXT_HOSTILE_SECONDARY_CONTROL,
        CYCLE_PRIMARY_WEAPON,
        FIRE_PARTICLE_WEAPON_ONLY,
        FIRE_BEAM_WEAPON_ONLY,
        CYCLE_TARGET_SUBSYSTEM,
        MULTIPLAYER_CLAN_LINK,
        MULIPLAYER_CLEAR_LINK,
        HOSTILE_CONTACT_RADAR_MODE,
        STEALTH_GENERATOR,
        MOUSE_LOOK_ON_OFF,
        MULTIPLAYER_VOICE_CHAT,
        SEND_ORDER_FORM_UP,
        SEND_ORDER_DEFEND_ME,
        SEND_ORDER_ATTACK_TARGET,
        SEND_ORDER_ATTACK_HOSTILES,
        SEND_ORDER_MINE_ASTEROIDS,
        SEND_ORDER_RELOAD_REFUEL,
        SEND_ORDER_DISMISS_ALL,
        SEND_ORDER_DISMISS,
        DEPLOY_ITEM_ENERGY_STATION,
        DEPLOY_ITEM_REPAIR_STATION,
        DEPLOY_ITEM_SENSOR_STATION,
        DEPLOY_ITEM_FUEL_PROCESSOR,
        DEPLOY_ITEM_SHIELD_ARRAY,
        DEPLOY_ITEM_MINING_PROBE,
        BUILD_TRADE_STATION,
        BUILD_CONSTRUCTOR,
        BUILD_RESEARCH,
        BUILD_ENERGY_STATION,
        BUILD_ORE_PROCESSOR,
        LOW_LIGHT_VISION_MODE,
        TARGET_NEAREST_OBJECT,
        TARGET_NEXT_OBJECT,
        TARGET_OBJECT_IN_GUNSIGHT,
        RADAR_MODE,
        RADAR_RANGE,
        FLEET_STATUS,
        TOGGLE_FORWARD_FULL_RANGE_THROTTLE_CONTROL,
        AUTO_LEVEL,
        PING_LOCATION,
        TRANSMIT_LOCATION,
        QUICK_SAVE_SECONDARY_CONTROL,
        AUTOPILOT_SECONDARY_CONTROL,
        LOCK_MINING_TRACTOR_BEAM_ON_SECONDARY_CONTROL,
        LOCK_MINING_TRACTOR_REPAIR_BEAM_ON_SECONDARY_CONTROL,
        PADLOCK_VIEW_SECONDARY_CONTROL,
        CINEMATIC_VIEW_SECONDARY_CONTROL,
        INCREASE_IDS_THOTTLE_SCALE,
        DECREASE_IDS_THOTTLE_SCALE,
        TERRAIN_WALKER_MOVE_FORWARD,
        TERRAIN_WALKER_MOVE_BACKWARD,
        TERRAIN_WALKER_MOVE_RIGHT,
        TERRAIN_WALKER_MOVE_LEFT,
        TERRAIN_WALKER_JUMP_JETS,
        TERRAIN_WALKER_FIRE_CANNONS,
        TERRAIN_WALKER_ACTIVATE_DEACTIVATE_TERRAIN_WALKER,
        THROTTLE_10_PERCENT,
        THROTTLE_20_PERCENT,
        THROTTLE_30_PERCENT,
        THROTTLE_40_PERCENT,
        THROTTLE_50_PERCENT,
        THROTTLE_60_PERCENT,
        THROTTLE_70_PERCENT,
        THROTTLE_80_PERCENT,
        THROTTLE_90_PERCENT,
        THROTTLE_100_PERCENT,
        ZERO_THROTTLE
    }
    #endregion


    [System.Diagnostics.DebuggerDisplay("[{_inputType}, {_scancode}] {_action}")]
    public class ActionDetail
    {
        #region Variables
        private string _category;
        private string _inputType;
        private GameAction _action;
        private string _description;
        private bool _isAltAction;
        private GameMeta.SupportedGame _availabilityFlags;
        private int _scancode;
        private int _configFileLineNr;
        #endregion


        #region Properties
        /// <summary> Returns the action category.
        /// </summary>
        public string Category
        {
            get { return _category; }
        }


        /// <summary> Returns the input type (keyboard, mouse, joystick).
        /// </summary>
        public string InputType
        {
            get { return _inputType; }
        }

        /// <summary> Returns the game action to execute.
        /// </summary>
        public GameAction Action
        {
            get { return _action; }
        }


        /// <summary> Returns the action description.
        /// </summary>
        public string Description
        {
            get { return _description; }
        }


        /// <summary> Returns whether this action is an alternative version of another action.
        /// <para>Those actions are triggered by pressing the alt key together with the standard binding key.</para>
        /// </summary>
        public bool IsAltAction
        {
            get { return _isAltAction; }
        }


        /// <summary> Returns the game flags for which this action is available.
        /// </summary>
        public GameMeta.SupportedGame AvailabilityFlags
        {
            get { return _availabilityFlags; }
        }


        /// <summary> Returns the keyboard scancode.
        /// </summary>
        public int Scancode
        {
            get { return _scancode; }
            set { _scancode = value; }
        }


        /// <summary> Returns the line number within the keymap8.txt file which contains the scancode for this action.
        /// </summary>
        public int ConfigFileLineNr
        {
            get { return _configFileLineNr; }
        }
        #endregion


        #region Constructor
        /// <summary> Creates a new action entry.
        /// </summary>
        /// <param name="pAction">The name of the action.</param>
        /// <param name="pInputType">The action's input type (mouse, keyboard or joystick).</param>
        /// <param name="pCategory">The category of the action.</param>
        /// <param name="pDescription">The action descrption.</param>
        /// <param name="pConfigFileLineNr">The corresponding line number within the keymap8.txt configuration file.</param>
        /// <param name="pAvailabilityFlags">The games in which the action is available.</param>
        /// <param name="pIsAltAction">Whether is action is an alteration of another one, triggered by pressing ALT as well.</param>
        public ActionDetail(GameAction pAction, string pInputType, string pCategory, string pDescription, int pConfigFileLineNr, GameMeta.SupportedGame pAvailabilityFlags, bool pIsAltAction)
        {
            this._category = pCategory;
            this._inputType = pInputType;
            this._configFileLineNr = pConfigFileLineNr;
            this._description = pDescription;
            this._action = pAction;
            this._availabilityFlags = pAvailabilityFlags;
            this._isAltAction = pIsAltAction;
            this._scancode = -1;
        }
        #endregion
    }


    public static class KeyboardControls
    {
        #region Regexes (readonly)
        private static readonly Regex KEYMAP_LAYOUT_REGEX = new Regex(
            @"\[\s*(?<Availability>.*?)\s*\]\s*" + 
            @"\[\s*(?<Category>.*?)\s*\]\s*" + 
            @"\[\s*(?<InputType>.*?)\s*\]\s*" + 
            @"(?<Action>\S+)\s*" +
            @"(?:\|\s*(?<AltAction>\S+)\s*)?\s*" +
            @"(?:'(?<Description>.*?)')?"
        );
        #endregion


        #region Variables
        private static Dictionary<GameAction, ActionDetail> _gameActions = new Dictionary<GameAction, ActionDetail>();
        #endregion


        #region Properties
        /// <summary> Returns the current action database.
        /// </summary>
        public static Dictionary<GameAction, ActionDetail> GameActions
        {
            get { return KeyboardControls._gameActions; }
        }
        #endregion


        #region Functions
        /// <summary> Creates a database with in-game actions for the currently set game.
        /// </summary>
        public static void BuildDatabase()
        {
            _gameActions.Clear();

            // Parse every line of the keymap8_Layout.txt resource template and generate the DB
            int currConfigLineNr = 0;
            string[] keymapTemplate = Properties.Resources.Keymap8_Layout.Split('\n');

            for (int i = 0; i < keymapTemplate.Length; i++)
            {
                string currLine = keymapTemplate[i];
                Match match = KEYMAP_LAYOUT_REGEX.Match(currLine);

                GameMeta.SupportedGame availabilityFlags = GameMeta.SupportedGame.NONE;
                if (match.Groups["Availability"].Value.Contains("EM")) { availabilityFlags |= GameMeta.SupportedGame.EVOCHRON_MERCENARY; }
                if (match.Groups["Availability"].Value.Contains("EL")) { availabilityFlags |= GameMeta.SupportedGame.EVOCHRON_LEGACY; }

                // Is the action supported by the currently set game?
                if ((availabilityFlags & GameMeta.CurrentGame) != GameMeta.CurrentGame) { continue; }
                currConfigLineNr++;

                // TODO: Also catch mouse and jostick input
                // Accept only keyboard inputs for now
                if (!String.Equals(match.Groups["InputType"].Value, "KEYBOARD", StringComparison.InvariantCultureIgnoreCase)) { continue; }

                GameAction actionType;
                Enum.TryParse(match.Groups["Action"].Value, out actionType);

                ActionDetail newKeyMapEntry = new ActionDetail(
                    actionType,
                    match.Groups["InputType"].Value,
                    match.Groups["Category"].Value,
                    match.Groups["Description"].Value,
                    currConfigLineNr,
                    availabilityFlags,
                    false
                );
                if (!_gameActions.ContainsKey(actionType)) { _gameActions.Add(actionType, newKeyMapEntry); }

                if (!String.IsNullOrWhiteSpace(match.Groups["AltAction"].Value))
                {
                    // Action has a second, alternative action (via pressing ALT)
                    Enum.TryParse(match.Groups["AltAction"].Value, out actionType);
                    newKeyMapEntry = new ActionDetail(
                        actionType,
                        match.Groups["InputType"].Value,
                        match.Groups["Category"].Value,
                        match.Groups["Description"].Value,
                        currConfigLineNr,
                        availabilityFlags,
                        true
                    );
                    if (!_gameActions.ContainsKey(actionType)) { _gameActions.Add(actionType, newKeyMapEntry); }
                }
            }
        }

            
        /// <summary> Loads the key mapping file for the current game and assigns the scancode to all actions wthin the database.
        /// </summary>
        public static void LoadKeymap()
        {
            /* Assign the scancodes to each keymap */
            string[] configFile = File.ReadAllLines(GameMeta.DefaultKeymapFilePath);
            int currScancodeIndex = 0;
            
            foreach(KeyValuePair<GameAction, ActionDetail> typeAction in _gameActions)
            {
                // Alternate actions share the same scancode as the "normal" one; jump back to the last scancode
                if (typeAction.Value.IsAltAction) { currScancodeIndex--; }

                int scancode;
                Int32.TryParse(configFile[typeAction.Value.ConfigFileLineNr - 1], out scancode);
                typeAction.Value.Scancode = scancode;

                currScancodeIndex++;
            }
        }
        #endregion
    }
}
