using System.Collections.Generic;

namespace VacVI.Database
{
    /// <summary> Contains information about the player's currently targeted ship.</summary>
    public static class TargetShipData
    {
        #region Constants (Parameter Names)
        const string PARAM_TARGET_DESCRIPTION = "TARGET DESCRIPTION";
        const string PARAM_TARGET_THREAT_LEVEL = "TARGET THREAT LEVEL";
        const string PARAM_TARGET_RANGE = "TARGET RANGE";
        const string PARAM_TARGET_ENGINE_DAMAGE = "TARGET ENGINE DAMAGE";
        const string PARAM_TARGET_WEAPON_DAMAGE = "TARGET WEAPON DAMAGE";
        const string PARAM_TARGET_NAV_DAMAGE = "TARGET NAV DAMAGE";
        const string PARAM_TARGET_FACTION = "TARGET FACTION";
        const string PARAM_TARGET_DAMAGE_LEVEL = "TARGET DAMAGE LEVEL";
        const string PARAM_TARGET_VELOCITY = "TARGET VELOCITY";
        const string PARAM_TARGET_FRONT_SHIELD_LEVEL = "TARGET FRONT SHIELD LEVEL";
        const string PARAM_TARGET_RIGHT_SHIELD_LEVEL = "TARGET RIGHT SHIELD LEVEL";
        const string PARAM_TARGET_LEFT_SHIELD_LEVEL = "TARGET LEFT SHIELD LEVEL";
        const string PARAM_TARGET_REAR_SHIELD_LEVEL = "TARGET REAR SHIELD LEVEL";

        const string PARAM_TARGET_ENGINE_CLASS = "TARGET ENGINE CLASS";
        const string PARAM_TARGET_RESISTOR_PACKS = "TARGET RESISTOR PACKS";
        const string PARAM_TARGET_HULL_PLATING = "TARGET HULL PLATING";
        const string PARAM_TARGET_MODULE_TYPE = "TARGET MODULE TYPE";
        const string PARAM_TARGET_WING_CLASS = "TARGET WING CLASS";

        static readonly string[] PARAM_TARGET_CARGO_BAY = new string[] { 
            "TARGET CARGO BAY 1", "TARGET CARGO BAY 2", "TARGET CARGO BAY 3", "TARGET CARGO BAY 4", "TARGET CARGO BAY 5", 
            "TARGET CARGO BAY 6", "TARGET CARGO BAY 7", "TARGET CARGO BAY 8", "TARGET CARGO BAY 9", "TARGET CARGO BAY 10"
        };

        static readonly string[] PARAM_CAPITAL_SHIP_WEAPON_TURRET = new string[] {
            "CAPITAL SHIP WEAPON TURRET 1", "CAPITAL SHIP WEAPON TURRET 2", "CAPITAL SHIP WEAPON TURRET 3", "CAPITAL SHIP WEAPON TURRET 4"
        };
        #endregion


        #region Variables - Converted values
        private static string[] _cargoBay = new string[10];
        private static string[] _capitalShipTurret = new string[5];
        private static Dictionary<ShieldLevelState, int> _shieldLevel = new Dictionary<ShieldLevelState, int>();
        private static ThreatLevelState _threatLevel = ThreatLevelState.LOW;
        #endregion


        #region Properties - Converted values
        /// <summary> [EMERC+] Returns the content of the target ship's cargo bay.
        /// <para>The amount of maximum cargo slots differs per game - see <see cref="VacVI.Database.ShipData.MaxCargoSlots"/>.
        /// </para>
        /// </summary>
        public static string[] CargoBay 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? TargetShipData._cargoBay : null; }
		}


        /// <summary> [ELGCY+] Return the status of the capital ship turret, if a capital ship has been targeted.</summary>
        public static string[] CapitalShipTurret 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? TargetShipData._capitalShipTurret : null; }
		}


        /// <summary> [EMERC+] Returns the ship's shield states (0 to 100).</summary>
        public static Dictionary<ShieldLevelState, int> ShieldLevel 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? TargetShipData._shieldLevel : null; }
		}


        /// <summary> [EMERC+] Returns the target ship's threat level.</summary>
        public static ThreatLevelState? ThreatLevel
        {
            get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (ThreatLevelState?)TargetShipData._threatLevel : null; }
        }
        #endregion


        #region Properties - Unconverted Values
        /// <summary> [EMERC+] Returns the target ship description (the ship's specific name).</summary>
        public static string Description 
		{
            get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (string)SaveDataReader.GetEntry(PARAM_TARGET_DESCRIPTION).Value : string.Empty; }
		}


        /// <summary> [EMERC+] Returns the target ship's distance from the player.</summary>
        public static int? Range 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)SaveDataReader.GetEntry(PARAM_TARGET_RANGE).Value : null; }
		}


        /// <summary> [EMERC+] Returns the damage on the target ship's engines (0-100).</summary>
        public static int? EngineDamage 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)SaveDataReader.GetEntry(PARAM_TARGET_ENGINE_DAMAGE).Value : null; }
		}


        /// <summary> [EMERC+] Returns the damage on the target ship's weapon systems (0-100).</summary>
        public static int? WeaponDamage 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)SaveDataReader.GetEntry(PARAM_TARGET_WEAPON_DAMAGE).Value : null; }
		}


        /// <summary> [EMERC+] Returns the damage on the target ship's nav-systems (0-100).</summary>
        public static int? NavDamage 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)SaveDataReader.GetEntry(PARAM_TARGET_NAV_DAMAGE).Value : null; }
		}


        /// <summary> [EMERC+] Returns the target ship's affiliated faction.</summary>
        public static string Faction 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (string)SaveDataReader.GetEntry(PARAM_TARGET_FACTION).Value : string.Empty; }
		}


        /// <summary> [EMERC+] Returns the damage on the target ship's hull (0-100).</summary>
        public static int? DamageLevel 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)SaveDataReader.GetEntry(PARAM_TARGET_DAMAGE_LEVEL).Value : null; }
		}


        /// <summary> [EMERC+] Returns the ship's velocity.</summary>
        public static int? Velocity 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)SaveDataReader.GetEntry(PARAM_TARGET_VELOCITY).Value : null; }
		}


        /// <summary> [ELGCY+] Returns the target ship's engine class.</summary>
        public static int? EngineClass 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? (int?)SaveDataReader.GetEntry(PARAM_TARGET_ENGINE_CLASS).Value : null; }
		}


        /// <summary> [ELGCY+] Returns the target ship's amount of resistor packs.</summary>
        public static int? ResistorPacks 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? (int?)SaveDataReader.GetEntry(PARAM_TARGET_RESISTOR_PACKS).Value : null; }
		}


        /// <summary> [ELGCY+] Returns the target ship's hull plating.</summary>
        public static int? HullPlating 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? (int?)SaveDataReader.GetEntry(PARAM_TARGET_HULL_PLATING).Value : null; }
		}


        /// <summary> [ELGCY+] Returns the target module's type.</summary>
        public static int? ModuleType 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? (int?)SaveDataReader.GetEntry(PARAM_TARGET_MODULE_TYPE).Value : null; }
		}


        /// <summary> [ELGCY+] Returns the target ship's wing class.</summary>
        public static int? WingClass 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? (int?)SaveDataReader.GetEntry(PARAM_TARGET_WING_CLASS).Value : null; }
		}
        #endregion


        #region (Static) Constructor
        static TargetShipData()
        {
            _shieldLevel.Add(ShieldLevelState.FRONT, 0);
            _shieldLevel.Add(ShieldLevelState.RIGHT, 0);
            _shieldLevel.Add(ShieldLevelState.LEFT, 0);
            _shieldLevel.Add(ShieldLevelState.REAR, 0);
            _shieldLevel.Add(ShieldLevelState.TOTAL, -1);
        }
        #endregion


        #region Functions
        /// <summary> Converts some data into a more convenient form.
        /// </summary>
        internal static void Update()
        {
            int maxCount;

            // Get shield levels
            _shieldLevel[ShieldLevelState.FRONT] = (int)SaveDataReader.GetEntry(PARAM_TARGET_FRONT_SHIELD_LEVEL).Value;
            _shieldLevel[ShieldLevelState.RIGHT] = (int)SaveDataReader.GetEntry(PARAM_TARGET_RIGHT_SHIELD_LEVEL).Value;
            _shieldLevel[ShieldLevelState.LEFT] = (int)SaveDataReader.GetEntry(PARAM_TARGET_LEFT_SHIELD_LEVEL).Value;
            _shieldLevel[ShieldLevelState.REAR] = (int)SaveDataReader.GetEntry(PARAM_TARGET_REAR_SHIELD_LEVEL).Value;
            _shieldLevel[ShieldLevelState.TOTAL] = (        // <-- Total shield strength is not being output by the game, so DIY
                _shieldLevel[ShieldLevelState.FRONT] + 
                _shieldLevel[ShieldLevelState.RIGHT] +
                _shieldLevel[ShieldLevelState.LEFT] +
                _shieldLevel[ShieldLevelState.REAR]
            ) / 4;

            if (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY)
            {
                // Get capital ship turrets
                maxCount = ShipData.MaxCapShipTurrets;
                for (int i = 0; i < maxCount; i++) { _capitalShipTurret[i] = (string)SaveDataReader.GetEntry(PARAM_CAPITAL_SHIP_WEAPON_TURRET[i]).Value; }
            }

            // Get cargo bays
            maxCount = ShipData.MaxCargoSlots;
            for (int i = 0; i < maxCount; i++) { _cargoBay[i] = (string)SaveDataReader.GetEntry(PARAM_TARGET_CARGO_BAY[i]).Value; }

            // Get threat level
            switch (((string)SaveDataReader.GetEntry(PARAM_TARGET_THREAT_LEVEL).Value).ToLowerInvariant())
            {
                case "low": _threatLevel = ThreatLevelState.LOW; break;
                case "med": _threatLevel = ThreatLevelState.MED; break;
                case "high": _threatLevel = ThreatLevelState.HIGH; break;
            }
        }
        #endregion
    }
}
