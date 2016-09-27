using VacVI.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace VacVI.Input
{
    #region Public Enums
    /// <summary> Contains in-game controls/actions, that can be passed as simulated key-presses.</summary>
    public enum GameAction
    {
        /// <summary>[EMERC+]</summary>
        TURN_RIGHT,

		/// <summary>[EMERC+]</summary>
        TURN_LEFT,

		/// <summary>[EMERC+]</summary>
        PITCH_DOWN,

		/// <summary>[EMERC+]</summary>
        PITCH_UP,

		/// <summary>[EMERC+]</summary>
        ROLL_RIGHT,

		/// <summary>[EMERC+]</summary>
        ROLL_LEFT,

		/// <summary>[EMERC+]</summary>
        STRAFE_RIGHT,

		/// <summary>[EMERC+]</summary>
        STRAFE_LEFT,

		/// <summary>[EMERC+]</summary>
        AFTERBURNER,

		/// <summary>[EMERC+]</summary>
        INERTIAL_SYSTEM_ON_OFF,

		/// <summary>[EMERC+]</summary>
        MATCH_TARGET_SPEED,

		/// <summary>[EMERC+]</summary>
        FIRE_PRIMARY_WEAPON,

		/// <summary>[EMERC+]</summary>
        FIRE_SECONDARY_WEAPON,

		/// <summary>[EMERC+]</summary>
        SELECT_NEXT_SECONDARY_WEAPON,

		/// <summary>[EMERC+]</summary>
        SELECT_PREVIOUS_SECONDARY_WEAPON,

		/// <summary>[EMERC+]</summary>
        COUNTERMEASURES,

		/// <summary>[EMERC+]</summary>
        TARGET_NEAREST_SHIP,

		/// <summary>[EMERC+]</summary>
        TARGET_NEAREST_HOSTILE,

		/// <summary>[EMERC+]</summary>
        TARGET_NEXT_SHIP,

		/// <summary>[EMERC+]</summary>
        TARGET_NEXT_HOSTILE,

		/// <summary>[EMERC+]</summary>
        BOOST_ENERGY_TO_SHIELDS,

		/// <summary>[EMERC+]</summary>
        BOOST_ENERGY_TO_WEAPONS,

		/// <summary>[EMERC+]</summary>
        HUD_MODE,

		/// <summary>[EMERC+]</summary>
        MDTS_ON_OFF,

		/// <summary>[EMERC+]</summary>
        TOGGLE_3RD_PERSON_VIEWS,

		/// <summary>[EMERC+]</summary>
        PADLOCK_ON_OFF,

		/// <summary>[EMERC+]</summary>
        FLY_BY_VIEW,

		/// <summary>[EMERC+]</summary>
        CINEMATIC_MODE,

		/// <summary>[EMERC+]</summary>
        VIEW_RIGHT,

		/// <summary>[EMERC+]</summary>
        PAN_RIGHT,

		/// <summary>[EMERC+]</summary>
        VIEW_LEFT,

		/// <summary>[EMERC+]</summary>
        PAN_LEFT,

		/// <summary>[EMERC+]</summary>
        VIEW_UP,

		/// <summary>[EMERC+]</summary>
        PAN_UP,

		/// <summary>[EMERC+]</summary>
        VIEW_BEHIND,

		/// <summary>[EMERC+]</summary>
        PAN_DOWN,

		/// <summary>[EMERC+]</summary>
        NAVIGATION_CONSOLE,

		/// <summary>[EMERC+]</summary>
        ENGAGE_JUMP_DRIVE,

		/// <summary>[EMERC+]</summary>
        ENGAGE_JUMP_DRIVE_MAXIMUM_RANGE,

		/// <summary>[EMERC+]</summary>
        BUILD_AND_DEPLOY_CONSOLE,

		/// <summary>[EMERC+]</summary>
        INVENTORY_CONSOLE,

		/// <summary>[EMERC+]</summary>
        DOCKING_CONTROL_ON_OFF,

		/// <summary>[EMERC+]</summary>
        TRADE_CONSOLE,

		/// <summary>[EMERC+]</summary>
        STRAFE_UP,

		/// <summary>[EMERC+]</summary>
        STRAFE_DOWN,

		/// <summary>[EMERC]</summary>
        TRACTOR_MINING_BEAM_ON_OFF,

        /// <summary>[EMERC]</summary>
        TRACTOR_MINING_BEAM_TOGGLE,

        /// <summary>[ELGCY]</summary>
        TRACTOR_MINING_REPAIR_BEAM_ON_OFF,

        /// <summary>[ELGCY]</summary>
        TRACTOR_MINING_REPAIR_BEAM_TOGGLE,

		/// <summary>[EMERC+]</summary>
        REVERSE_ENGINE_THRUST,

		/// <summary>[EMERC+]</summary>
        FORM_ON_TARGET,

		/// <summary>[EMERC+]</summary>
        NAV_AUTOPILOT,

		/// <summary>[EMERC]</summary>
        TOGGLE_TARGET_DETAIL_AND_RANGED_LIST,

        /// <summary>[ELGCY]</summary>
        TARGET_DISPLAY_MODE,

		/// <summary>[EMERC+]</summary>
        QUIT,

		/// <summary>[EMERC+]</summary>
        QUICK_SAVE,

		/// <summary>[EMERC+]</summary>
        DELETE_MESSAGE,

		/// <summary>[EMERC+]</summary>
        RECALL_MESSAGE,

		/// <summary>[EMERC+]</summary>
        DELETE_ALL_MESSAGE_LINES,

		/// <summary>[EMERC+]</summary>
        JETTISON_CARGO,

		/// <summary>[EMERC+]</summary>
        INERTIAL_FORWARD,

		/// <summary>[EMERC+]</summary>
        INERTIAL_REVERSE,

		/// <summary>[EMERC+]</summary>
        TARGET_SHIP_IN_GUNSIGHT,

		/// <summary>[EMERC+]</summary>
        THROTTLE_UP_SECONDARY_CONTROL,

        /// <summary>[ELGCY]</summary>
        THROTTLE_UP,

		/// <summary>[EMERC+]</summary>
        THROTTLE_DOWN_SECONDARY_CONTROL,

        /// <summary>[ELGCY]</summary>
        THROTTLE_DOWN,

		/// <summary>[EMERC+]</summary>
        AUGMENT_FRONT_SHIELD,

		/// <summary>[EMERC+]</summary>
        AUGMENT_REAR_SHIELD,

		/// <summary>[EMERC+]</summary>
        AUGMENT_RIGHT_SHIELD,

		/// <summary>[EMERC+]</summary>
        AUGMENT_LEFT_SHIELD,

		/// <summary>[EMERC+]</summary>
        EQUALIZE_SHIELDS,

		/// <summary>[EMERC+]</summary>
        EJECT_AND_SELF_DESTRUCT_SHIP,

		/// <summary>[EMERC+]</summary>
        NEAREST_HOSTILE_SECONDARY_CONTROL,

		/// <summary>[EMERC+]</summary>
        NEXT_HOSTILE_SECONDARY_CONTROL,

		/// <summary>[EMERC+]</summary>
        CYCLE_PRIMARY_WEAPON,

		/// <summary>[EMERC+]</summary>
        FIRE_PARTICLE_WEAPON_ONLY,

		/// <summary>[EMERC+]</summary>
        FIRE_BEAM_WEAPON_ONLY,

		/// <summary>[EMERC+]</summary>
        CYCLE_TARGET_SUBSYSTEM,

		/// <summary>[EMERC]</summary>
        MULTIPLAYER_CLAN_LINK,

		/// <summary>[EMERC]</summary>
        MULIPLAYER_CLEAR_LINK,

        /// <summary>[ELGCY]</summary>
        HOSTILE_CONTACT_RADAR_MODE,

		/// <summary>[EMERC+]</summary>
        STEALTH_GENERATOR,

		/// <summary>[EMERC+]</summary>
        MOUSE_LOOK_ON_OFF,

		/// <summary>[EMERC+]</summary>
        MULTIPLAYER_VOICE_CHAT,

		/// <summary>[EMERC+]</summary>
        SEND_ORDER_FORM_UP,

		/// <summary>[EMERC]</summary>
        SEND_ORDER_DEFEND_ME,

        /// <summary>[ELGCY]</summary>
        SEND_ORDER_ATTACK_TARGET,

		/// <summary>[EMERC+]</summary>
        SEND_ORDER_ATTACK_HOSTILES,

		/// <summary>[EMERC+]</summary>
        SEND_ORDER_MINE_ASTEROIDS,

		/// <summary>[EMERC+]</summary>
        SEND_ORDER_RELOAD_REFUEL,

		/// <summary>[EMERC+]</summary>
        SEND_ORDER_DISMISS_ALL,

		/// <summary>[EMERC+]</summary>
        SEND_ORDER_DISMISS,

		/// <summary>[EMERC]</summary>
        DEPLOY_ITEM_ENERGY_STATION,

		/// <summary>[EMERC]</summary>
        DEPLOY_ITEM_REPAIR_STATION,

		/// <summary>[EMERC]</summary>
        DEPLOY_ITEM_SENSOR_STATION,

		/// <summary>[EMERC]</summary>
        DEPLOY_ITEM_FUEL_PROCESSOR,

		/// <summary>[EMERC]</summary>
        DEPLOY_ITEM_SHIELD_ARRAY,

		/// <summary>[EMERC]</summary>
        DEPLOY_ITEM_MINING_PROBE,

		/// <summary>[EMERC]</summary>
        BUILD_TRADE_STATION,

		/// <summary>[EMERC]</summary>
        BUILD_CONSTRUCTOR,

		/// <summary>[EMERC]</summary>
        BUILD_RESEARCH,

		/// <summary>[EMERC]</summary>
        BUILD_ENERGY_STATION,

		/// <summary>[EMERC]</summary>
        BUILD_ORE_PROCESSOR,

		/// <summary>[ELGCY]</summary>
        LOW_LIGHT_VISION_MODE,

		/// <summary>[ELGCY]</summary>
        TARGET_NEAREST_OBJECT,

		/// <summary>[ELGCY]</summary>
        TARGET_NEXT_OBJECT,

		/// <summary>[ELGCY]</summary>
        TARGET_OBJECT_IN_GUNSIGHT,

		/// <summary>[ELGCY]</summary>
        RADAR_MODE,

		/// <summary>[ELGCY]</summary>
        RADAR_RANGE,

		/// <summary>[ELGCY]</summary>
        FLEET_STATUS,

		/// <summary>[ELGCY]</summary>
        TOGGLE_FORWARD_FULL_RANGE_THROTTLE_CONTROL,

		/// <summary>[ELGCY]</summary>
        AUTO_LEVEL,

		/// <summary>[ELGCY]</summary>
        PING_LOCATION,

		/// <summary>[ELGCY]</summary>
        TRANSMIT_LOCATION,

		/// <summary>[EMERC+]</summary>
        QUICK_SAVE_SECONDARY_CONTROL,

		/// <summary>[EMERC+]</summary>
        AUTOPILOT_SECONDARY_CONTROL,

		/// <summary>[EMERC]</summary>
        LOCK_MINING_TRACTOR_BEAM_ON_SECONDARY_CONTROL,

        /// <summary>[ELGCY]</summary>
        LOCK_MINING_TRACTOR_REPAIR_BEAM_ON_SECONDARY_CONTROL,

		/// <summary>[EMERC+]</summary>
        PADLOCK_VIEW_SECONDARY_CONTROL,

		/// <summary>[EMERC+]</summary>
        CINEMATIC_VIEW_SECONDARY_CONTROL,

		/// <summary>[EMERC+]</summary>
        INCREASE_IDS_THOTTLE_SCALE,

		/// <summary>[EMERC+]</summary>
        DECREASE_IDS_THOTTLE_SCALE,

		/// <summary>[EMERC+]</summary>
        TERRAIN_WALKER_MOVE_FORWARD,

		/// <summary>[EMERC+]</summary>
        TERRAIN_WALKER_MOVE_BACKWARD,

		/// <summary>[EMERC+]</summary>
        TERRAIN_WALKER_MOVE_RIGHT,

		/// <summary>[EMERC+]</summary>
        TERRAIN_WALKER_MOVE_LEFT,

		/// <summary>[EMERC+]</summary>
        TERRAIN_WALKER_JUMP_JETS,

		/// <summary>[EMERC+]</summary>
        TERRAIN_WALKER_FIRE_CANNONS,

		/// <summary>[EMERC+]</summary>
        TERRAIN_WALKER_ACTIVATE_DEACTIVATE_TERRAIN_WALKER,

		/// <summary>[ELGCY]</summary>
        THROTTLE_10_PERCENT,

		/// <summary>[ELGCY]</summary>
        THROTTLE_20_PERCENT,

		/// <summary>[ELGCY]</summary>
        THROTTLE_30_PERCENT,

		/// <summary>[ELGCY]</summary>
        THROTTLE_40_PERCENT,

		/// <summary>[ELGCY]</summary>
        THROTTLE_50_PERCENT,

		/// <summary>[ELGCY]</summary>
        THROTTLE_60_PERCENT,

		/// <summary>[ELGCY]</summary>
        THROTTLE_70_PERCENT,

		/// <summary>[ELGCY]</summary>
        THROTTLE_80_PERCENT,

		/// <summary>[ELGCY]</summary>
        THROTTLE_90_PERCENT,

		/// <summary>[ELGCY]</summary>
        THROTTLE_100_PERCENT,

		/// <summary>[EMERC+]</summary>
        ZERO_THROTTLE
    }
    #endregion


    [System.Diagnostics.DebuggerDisplay("[{_inputType}, {_scancode}] {_action}")]
    /// <summary> A class containing detailed informations about an in-game action.</summary>
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


    /// <summary> Class containing a database of in-game controls for the currently set game.</summary>
    public static class KeyboardControls
    {
        #region Regexes (readonly)
        /// <summary> Regex that extracts necessary data out of the keymap8-template resource.</summary>
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
        internal static void BuildDatabase()
        {
            _gameActions.Clear();

            // Parse every line of the keymap8_Layout.txt resource template and generate the DB
            int currConfigLineNr = 0;
            string[] keymapTemplate = VacVI.Properties.Resources.Keymap8_Layout.Split('\n');

            for (int i = 0; i < keymapTemplate.Length; i++)
            {
                string currLine = keymapTemplate[i];
                Match match = KEYMAP_LAYOUT_REGEX.Match(currLine);

                if (!match.Success) { continue; }

                GameMeta.SupportedGame availabilityFlags = GameMeta.SupportedGame.NONE;
                if (match.Groups["Availability"].Value.Contains("EMERC")) { availabilityFlags |= GameMeta.SupportedGame.EVOCHRON_MERCENARY; }
                if (match.Groups["Availability"].Value.Contains("ELGCY")) { availabilityFlags |= GameMeta.SupportedGame.EVOCHRON_LEGACY; }

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
        internal static void LoadKeymap()
        {
            /* Assign the scancodes to each keymap */
            string[] configFile;
            int currScancodeIndex = 0;

            try { configFile = File.ReadAllLines(GameMeta.DefaultKeymapFilePath); }
            catch (IOException) { return; }
            
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
