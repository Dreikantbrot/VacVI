using System.Collections.Generic;

namespace EvoVI.Database
{
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
        private static string[] _cargoBay = new string[5];
        private static string[] _capitalShipTurret = new string[5];
        private static Dictionary<ShieldLevelState, int> _shieldLevel = new Dictionary<ShieldLevelState, int>();
        #endregion


        #region Properties - Converted values
        public static string[] CargoBay { get { return TargetShipData._cargoBay; } }
        public static string[] CapitalShipTurret { get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? TargetShipData._capitalShipTurret : null; } }
        public static Dictionary<ShieldLevelState, int> ShieldLevel { get { return TargetShipData._shieldLevel; } }
        #endregion


        #region Properties - Unconverted Values
        public static string Description { get { return (string)SaveDataReader.GetEntry(PARAM_TARGET_DESCRIPTION).Value; } }
        public static string ThreatLevel { get { return (string)SaveDataReader.GetEntry(PARAM_TARGET_THREAT_LEVEL).Value; } }
        public static int Range { get { return (int)SaveDataReader.GetEntry(PARAM_TARGET_RANGE).Value; } }
        public static int EngineDamage { get { return (int)SaveDataReader.GetEntry(PARAM_TARGET_ENGINE_DAMAGE).Value; } }
        public static int WeaponDamage { get { return (int)SaveDataReader.GetEntry(PARAM_TARGET_WEAPON_DAMAGE).Value; } }
        public static int NavDamage { get { return (int)SaveDataReader.GetEntry(PARAM_TARGET_NAV_DAMAGE).Value; } }
        public static string Faction { get { return (string)SaveDataReader.GetEntry(PARAM_TARGET_FACTION).Value; } }
        public static int DamageLevel { get { return (int)SaveDataReader.GetEntry(PARAM_TARGET_DAMAGE_LEVEL).Value; } }
        public static int Velocity { get { return (int)SaveDataReader.GetEntry(PARAM_TARGET_VELOCITY).Value; } }
        public static int EngineClass { get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? (int)SaveDataReader.GetEntry(PARAM_TARGET_ENGINE_CLASS).Value : -1; } }
        public static int ResistorPacks { get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? (int)SaveDataReader.GetEntry(PARAM_TARGET_RESISTOR_PACKS).Value : -1; } }
        public static int HullPlating { get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? (int)SaveDataReader.GetEntry(PARAM_TARGET_HULL_PLATING).Value : -1; } }
        public static int ModuleType { get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? (int)SaveDataReader.GetEntry(PARAM_TARGET_MODULE_TYPE).Value : -1; } }
        public static int WingClass { get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? (int)SaveDataReader.GetEntry(PARAM_TARGET_WING_CLASS).Value : -1; } }
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
        public static void Update()
        {
            int maxCount;

            // Get shield levels
            _shieldLevel[ShieldLevelState.FRONT] = (int)SaveDataReader.GetEntry(PARAM_TARGET_FRONT_SHIELD_LEVEL).Value;
            _shieldLevel[ShieldLevelState.RIGHT] = (int)SaveDataReader.GetEntry(PARAM_TARGET_RIGHT_SHIELD_LEVEL).Value;
            _shieldLevel[ShieldLevelState.LEFT] = (int)SaveDataReader.GetEntry(PARAM_TARGET_LEFT_SHIELD_LEVEL).Value;
            _shieldLevel[ShieldLevelState.REAR] = (int)SaveDataReader.GetEntry(PARAM_TARGET_REAR_SHIELD_LEVEL).Value;
            _shieldLevel[ShieldLevelState.TOTAL] = (        // Total shield strength is not being output by the game, so DIY
                _shieldLevel[ShieldLevelState.FRONT] + 
                _shieldLevel[ShieldLevelState.RIGHT] +
                _shieldLevel[ShieldLevelState.LEFT] +
                _shieldLevel[ShieldLevelState.REAR]
            ) / 4;

            if (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY)
            {
                // Get capital ship turrets
                maxCount = (
                    (GameMeta.CurrentGame == (GameMeta.CurrentGame & GameMeta.SupportedGame.EVOCHRON_MERCENARY)) ? 0 :
                    (GameMeta.CurrentGame == (GameMeta.CurrentGame & GameMeta.SupportedGame.EVOCHRON_LEGACY)) ? 5 :
                    0
                );
                for (int i = 0; i < maxCount; i++) { _capitalShipTurret[i] = (string)SaveDataReader.GetEntry(PARAM_CAPITAL_SHIP_WEAPON_TURRET[i]).Value; }
            }

            // Get cargo bays
            maxCount = (
                (GameMeta.CurrentGame == (GameMeta.CurrentGame & GameMeta.SupportedGame.EVOCHRON_MERCENARY)) ? 5 :
                (GameMeta.CurrentGame == (GameMeta.CurrentGame & GameMeta.SupportedGame.EVOCHRON_LEGACY)) ? 10 :
                0
            );
            for (int i = 0; i < maxCount; i++) { _cargoBay[i] = (string)SaveDataReader.GetEntry(PARAM_TARGET_CARGO_BAY[i]).Value; }
        }
        #endregion
    }
}
