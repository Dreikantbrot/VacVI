using EvoVI.Classes.Math;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EvoVI.Database
{
    public static class PlayerShipData
    {
        #region Enums
        public enum HeatState { LOW = 0, HIGH = 1 };
        public enum MTDSState { OFF = 0, ON = 1, LOCKED = 2 };
        public enum MissileState { NO_LOCK = 0, LOCKED = 1 };
        public enum AutopilotState { OFF = 0, FORM_ON_TARGET = 1, FLY_TO_NAV_POINT = 2 };
        #endregion


        #region Variables - Converted values
        private static string[] _cargoBay = new string[5];
        private static Vector3D _position = new Vector3D();
        private static Vector3D _sectorPosition = new Vector3D();
        private static Dictionary<ShieldLevelState, int> _shieldLevel = new Dictionary<ShieldLevelState, int>();
        private static string[] _secWeapon = new string[8];
        private static string[] _equipmentSlot = new string[8];
        private static HeatState _heat = HeatState.LOW;
        private static MTDSState _mtds = MTDSState.ON;
        private static MissileState _missileLock = MissileState.NO_LOCK;
        private static int _shieldBias = 0;
        private static int _weaponBias = 0;
        private static OnOffState _ids = OnOffState.ON;
        private static OnOffState _afterburner = OnOffState.ON;
        private static AutopilotState _autopilot = AutopilotState.OFF;
        private static OnOffState _tractorBeam = OnOffState.OFF;
        #endregion


        #region Properties - Converted values
        public static string[] CargoBay { get { return PlayerShipData._cargoBay; } }
        public static Vector3D Position { get { return PlayerShipData._position; } }
        public static Vector3D SectorPosition { get { return PlayerShipData._sectorPosition; } }
        public static Dictionary<ShieldLevelState, int> ShieldLevel { get { return PlayerShipData._shieldLevel; } }
        public static string[] SecWeapon { get { return PlayerShipData._secWeapon; } }
        public static string[] EquipmentSlot { get { return PlayerShipData._equipmentSlot; } }
        public static HeatState Heat { get { return PlayerShipData._heat; } }
        public static MTDSState Mtds { get { return PlayerShipData._mtds; } }
        public static MissileState MissileLock { get { return PlayerShipData._missileLock; } }
        public static int ShieldBias { get { return PlayerShipData._shieldBias; } }
        public static int WeaponBias { get { return PlayerShipData._weaponBias; } }
        public static OnOffState Ids { get { return PlayerShipData._ids; } }
        public static OnOffState Afterburner { get { return PlayerShipData._afterburner; } }
        public static AutopilotState Autopilot { get { return PlayerShipData._autopilot; } }
        public static OnOffState TractorBeam { get { return PlayerShipData._tractorBeam; } }
        #endregion


        #region Properties - Unconverted Values
        public static int Fuel { get { return (int)SaveDataReader.SaveData[1].Value; } }
        public static int EnergyLevel { get { return (int)SaveDataReader.SaveData[15].Value; } }
        public static int EngineDamage { get { return (int)SaveDataReader.SaveData[21].Value; } }
        public static int WeaponDamage { get { return (int)SaveDataReader.SaveData[22].Value; } }
        public static int NavDamage { get { return (int)SaveDataReader.SaveData[23].Value; } }
        public static string ParticleCannon { get { return (string)SaveDataReader.SaveData[40].Value; } }
        public static string BeamCannon { get { return (string)SaveDataReader.SaveData[41].Value; } }
        public static string ShipType { get { return (string)SaveDataReader.SaveData[58].Value; } }
        public static int EngineClass { get { return (int)SaveDataReader.SaveData[59].Value; } }
        public static int ShieldClass { get { return (int)SaveDataReader.SaveData[60].Value; } }
        public static int CargoCapacity { get { return (int)SaveDataReader.SaveData[61].Value; } }
        public static int WingClass { get { return (int)SaveDataReader.SaveData[62].Value; } }
        public static int ThrusterClass { get { return WingClass; } }
        public static int CrewLimit { get { return (int)SaveDataReader.SaveData[63].Value; } }
        public static int EquipentLimit { get { return (int)SaveDataReader.SaveData[64].Value; } }
        public static int CountermeasureLimit { get { return (int)SaveDataReader.SaveData[65].Value; } }
        public static int HarpointLimit { get { return (int)SaveDataReader.SaveData[66].Value; } }
        public static int ParticleCannonRange { get { return (int)SaveDataReader.SaveData[71].Value; } }
        public static int MissileRange { get { return (int)SaveDataReader.SaveData[72].Value; } }
        public static string TargetedSubsystem { get { return (string)SaveDataReader.SaveData[73].Value; } }
        public static int CounterMeasures { get { return (int)SaveDataReader.SaveData[80].Value; } }
        public static int IDSMultiplier { get { return (int)SaveDataReader.SaveData[84].Value; } }
        public static int Velocity { get { return (int)SaveDataReader.SaveData[94].Value; } }
        public static int SetVelocity { get { return (int)SaveDataReader.SaveData[95].Value; } }
        public static int Altitude { get { return (int)SaveDataReader.SaveData[96].Value; } }
        public static int HeatSignatureLevel { get { return (int)SaveDataReader.SaveData[98].Value; } }
        #endregion


        #region Functions
        /// <summary> Converts some data into a more convenient form.
        /// </summary>
        public static void Update()
        {
            // Get cargo bays (lines 4-8)
            for (int i = 3; i < 8; i++) { _cargoBay[i - 3] = (string)SaveDataReader.SaveData[i].Value; }

            // Get position data (lines 9-11, 12-14)
            _position.X = (int)SaveDataReader.SaveData[8].Value;
            _position.Y = (int)SaveDataReader.SaveData[9].Value;
            _position.Z = (int)SaveDataReader.SaveData[10].Value;
            _sectorPosition.X = (int)SaveDataReader.SaveData[11].Value;
            _sectorPosition.Y = (int)SaveDataReader.SaveData[12].Value;
            _sectorPosition.Z = (int)SaveDataReader.SaveData[13].Value;

            // Get shield levels (lines 17-21)
            if (_shieldLevel.Count == 0)
            {
                _shieldLevel.Add(ShieldLevelState.FRONT, 0);
                _shieldLevel.Add(ShieldLevelState.RIGHT, 0);
                _shieldLevel.Add(ShieldLevelState.LEFT, 0);
                _shieldLevel.Add(ShieldLevelState.REAR, 0);
                _shieldLevel.Add(ShieldLevelState.TOTAL, 0);
            }
            _shieldLevel[ShieldLevelState.FRONT] = (int)SaveDataReader.SaveData[16].Value;
            _shieldLevel[ShieldLevelState.RIGHT] = (int)SaveDataReader.SaveData[17].Value;
            _shieldLevel[ShieldLevelState.LEFT] = (int)SaveDataReader.SaveData[18].Value;
            _shieldLevel[ShieldLevelState.REAR] = (int)SaveDataReader.SaveData[19].Value;
            _shieldLevel[ShieldLevelState.TOTAL] = (int)SaveDataReader.SaveData[20].Value;

            // Get secondary weapons (lines 43-50)
            for (int i = 42; i < 50; i++) { _secWeapon[i - 42] = (string)SaveDataReader.SaveData[i].Value; }

            // Get equipment (lines 51-58)
            for (int i = 50; i < 58; i++) { _equipmentSlot[i - 50] = (string)SaveDataReader.SaveData[i].Value; }

            // Convert heat (line 78)
            switch ((int)SaveDataReader.SaveData[77].Value)
            {
                case 0: _heat = HeatState.LOW; break;
                case 1: _heat = HeatState.HIGH; break;
            }

            // Convert MTDS state (line 79)
            switch ((int)SaveDataReader.SaveData[78].Value)
            {
                case 0: _mtds = MTDSState.OFF; break;
                case 1: _mtds = MTDSState.ON; break;
                case 2: _mtds = MTDSState.LOCKED; break;
            }

            // Convert missile lock state (line 80)
            switch ((int)SaveDataReader.SaveData[79].Value)
            {
                case 0: _missileLock = MissileState.NO_LOCK; break;
                case 1: _missileLock = MissileState.LOCKED; break;
            }

            // Set S/E bias (line 82)
            string bias;
            Regex biasRegex = new Regex(@"(?<Shields>[-+]?\d+?)S\/(?<Weapons>[-+]?\d+?)W");

            bias = biasRegex.Match((string)SaveDataReader.SaveData[81].Value).Groups["Shields"].Value;
            Int32.TryParse(bias, out _shieldBias);

            bias = biasRegex.Match((string)SaveDataReader.SaveData[81].Value).Groups["Weapons"].Value;
            Int32.TryParse(bias, out _weaponBias);

            // Convert IDS state (line 84)
            SaveDataReader.ConvertOnOffState((int)SaveDataReader.SaveData[83].Value, out _ids);

            // Convert afterburner status (line 86)
            SaveDataReader.ConvertOnOffState((int)SaveDataReader.SaveData[85].Value, out _afterburner);

            // Convert autopilot status (line 87)
            switch ((int)SaveDataReader.SaveData[86].Value)
            {
                case 0: _autopilot = AutopilotState.OFF; break;
                case 1: _autopilot = AutopilotState.FORM_ON_TARGET; break;
                case 2: _autopilot = AutopilotState.FLY_TO_NAV_POINT; break;
            }

            // Convert tractor beam status (line 91)
            SaveDataReader.ConvertOnOffState((int)SaveDataReader.SaveData[90].Value, out _tractorBeam);
        }
        #endregion
    }
}
