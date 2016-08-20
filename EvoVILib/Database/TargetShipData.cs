using System.Collections.Generic;

namespace EvoVI.Database
{
    public static class TargetShipData
    {
        #region Variables - Converted values
        private static string[] _cargoBay = new string[5];
        private static Dictionary<ShieldLevelState, int> _shieldLevel = new Dictionary<ShieldLevelState, int>();
        #endregion


        #region Properties - Converted values
        public static string[] CargoBay { get { return TargetShipData._cargoBay; } }
        public static Dictionary<ShieldLevelState, int> ShieldLevel { get { return TargetShipData._shieldLevel; } }
        #endregion


        #region Properties - Unconverted Values
        public static string Description { get { return (string)SaveDataReader.SaveData[24].Value; } }
        public static string ThreatLevel { get { return (string)SaveDataReader.SaveData[25].Value; } }
        public static int Range { get { return (int)SaveDataReader.SaveData[26].Value; } }
        public static int EngineDamage { get { return (int)SaveDataReader.SaveData[31].Value; } }
        public static int WeaponDamage { get { return (int)SaveDataReader.SaveData[32].Value; } }
        public static int NavDamage { get { return (int)SaveDataReader.SaveData[33].Value; } }
        public static string Faction { get { return (string)SaveDataReader.SaveData[74].Value; } }
        public static int DamageLevel { get { return (int)SaveDataReader.SaveData[75].Value; } }
        public static int Velocity { get { return (int)SaveDataReader.SaveData[76].Value; } }
        #endregion


        #region Functions
        /// <summary> Converts some data into a more convenient form.
        /// </summary>
        public static void Update()
        {
            // Get shield levels (lines 28-31)
            if (_shieldLevel.Count == 0)
            {
                _shieldLevel.Add(ShieldLevelState.FRONT, 0);
                _shieldLevel.Add(ShieldLevelState.RIGHT, 0);
                _shieldLevel.Add(ShieldLevelState.LEFT, 0);
                _shieldLevel.Add(ShieldLevelState.REAR, 0);
                _shieldLevel.Add(ShieldLevelState.TOTAL, -1);
            }
            _shieldLevel[ShieldLevelState.FRONT] = (int)SaveDataReader.SaveData[27].Value;
            _shieldLevel[ShieldLevelState.RIGHT] = (int)SaveDataReader.SaveData[28].Value;
            _shieldLevel[ShieldLevelState.LEFT] = (int)SaveDataReader.SaveData[29].Value;
            _shieldLevel[ShieldLevelState.REAR] = (int)SaveDataReader.SaveData[30].Value;

            // Get cargo bays (lines 35-39)
            for (int i = 34; i < 39; i++) { _cargoBay[i - 34] = (string)SaveDataReader.SaveData[i].Value; }
        }
        #endregion
    }
}
