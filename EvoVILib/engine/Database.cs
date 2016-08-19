using EvoVI.classes.math;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace EvoVI.engine
{
    public static class Database
    {
        #region Enums 
        public enum OnOffState { OFF = 0, ON = 1 };
        public enum ShieldLevelState { FRONT, RIGHT, LEFT, REAR, TOTAL };
        #endregion


        #region Classes
        [System.Diagnostics.DebuggerDisplay("<{_type}> {_id}")]
        private class dataEntry
        {
            #region Enum
            /// <summary> Known data types within savedata.txt.
            /// </summary>
            public enum DataType { INTEGER, STRING, UNKNOWN };
            #endregion


            #region Variables
            string _id;
            DataType _type;
            string _strVal;
            int _intVal;
            #endregion


            #region Properties
            public DataType Type
            {
                get { return _type; }
            }

            public object Value
            {
                get
                {
                    switch (_type)
                    {
                        case DataType.INTEGER: return _intVal;
                        case DataType.STRING: return _strVal;

                        default: return null;
                    }
                }
            }
            #endregion


            #region Constructor
            /// <summary> Creates a data entry instance holding a single entry from the savedata.txt file.
            /// </summary>
            /// <param name="pName">The parameter name.</param>
            /// <param name="pType">The parameter's data type</param>
            public dataEntry(string pName, string pType)
            {
                _id = pName;

                switch (pType.ToLower())
                {
                    case "integer":
                        _type = DataType.INTEGER;
                        break;
                    case "string":
                        _type = DataType.STRING;
                        break;

                    default:
                        _type = DataType.UNKNOWN;
                        break;
                }
            }
            #endregion


            #region Functions
            public void SetValue(string val) { _strVal = val; }
            public void SetValue(int val) { _intVal = val; }
            #endregion
        }
        #endregion


        #region Variables
        private static List<dataEntry> _saveData = new List<dataEntry>();
        #endregion


        #region Functions
        private static void convertOnOffState(int val, out OnOffState var)
        {
            var = (val == 1) ? OnOffState.ON : OnOffState.OFF;
        }
        #endregion


        /// <summary> Contains, parses and stores data extracted from savedata.txt.
        /// </summary>
        public static class IngameData
        {
            /// <summary> Reads, parses and stores data from savedata.txt.
            /// </summary>
            public static class SaveDataReader
            {
                #region Regexes (readonly)
                readonly static Regex SLOT_ID_REGEX = new Regex(@"<(?<DataType>.*?)>\s*(?<ParamName>.*?)(?:\r|\n|$)");
                #endregion


                #region Functions
                /// <summary> Initializes the savedata database.
                /// </summary>
                public static void Initialize()
                {
                    _saveData.Clear();

                    /* Read savedata template and parse parameters */
                    string[] savedataTemplate = EvoVI.Properties.Resources.savedata_template.Split('\n');

                    // Build the database
                    for (int i = 0; i < savedataTemplate.Length; i++)
                    {
                        string currLine = savedataTemplate[i];
                        Match match = SLOT_ID_REGEX.Match(currLine);
                        _saveData.Add(new dataEntry(match.Groups["ParamName"].Value, match.Groups["DataType"].Value));
                    }
                }


                /// <summary> Reads and stores the contents of the savedata.txt file.
                /// </summary>
                /// <param name="filepath">The filepath to savedata.txt.</param>
                /// <returns>Whether information has been retrieved successfully.</returns>
                public static bool ReadGameData(string filepath = "C:\\sw3dg\\EvochronMercenary\\savedata.txt")
                {
                    string[] fileContent;

                    // Try to read the file
                    try
                    {
                        fileContent = System.IO.File.ReadAllLines(filepath);
                    }
                    catch (Exception)
                    {
                        SpeechEngine.Say(EvoVI.Properties.StringTable.COULD_NOT_ACQUIRE_DATA);
                        return false;
                    }

                    // Get all in-game values and store them inside the database
                    for (int i = 0; i < Math.Min(fileContent.Length, _saveData.Count); i++)
                    {
                        string currVal = fileContent[i];
                        currVal = fileContent[i].Trim();

                        switch (_saveData[i].Type)
                        {
                            case dataEntry.DataType.INTEGER:
                                int intValue;
                                Int32.TryParse(currVal, out intValue);
                                _saveData[i].SetValue(intValue);
                                break;

                            case dataEntry.DataType.STRING:
                                _saveData[i].SetValue(currVal);
                                break;

                            default: break;
                        }
                    }

                    PlayerData.Update();
                    PlayerShipData.Update();
                    HudData.Update();
                    TargetShipData.Update();
                    EnvironmentData.Update();

                    // Call OnGameDataUpdate on all plugins
                    for (int i = 0; i < PluginLoader.Plugins.Count; i++) { PluginLoader.Plugins[i].OnGameDataUpdate(); }
                    return true;
                }
                #endregion
            }


            /// <summary> Contains information about the player ship.
            /// </summary>
            public static class PlayerData
            {
                #region Variables - Converted Values
                private static int _cash = 0;
                #endregion


                #region Properties (Converted Values)
                public static int Cash { get { return _cash; } }
                #endregion


                #region Properties (Unconverted Values)
                public static string Name { get { return (string)_saveData[0].Value; } }
                public static int TotalKills { get { return (int)_saveData[67].Value; } }
                public static int TotalContracts { get { return (int)_saveData[68].Value; } }
                public static int SkillAndProficiencyRating { get { return (int)_saveData[69].Value; } }
                public static int MilitaryRating { get { return (int)_saveData[70].Value; } }
                #endregion


                #region Functions
                /// <summary> Converts some data into a more convenient form.
                /// </summary>
                public static void Update()
                {
                    // Convert cash (line 3)
                    Int32.TryParse(((string)_saveData[2].Value).Replace(",", ""), out _cash);
                }
                #endregion
            }


            /// <summary> Contains information about the player ship.
            /// </summary>
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
                public static int Fuel { get { return (int)_saveData[1].Value; } }
                public static int EnergyLevel { get { return (int)_saveData[15].Value; } }
                public static int EngineDamage { get { return (int)_saveData[21].Value; } }
                public static int WeaponDamage { get { return (int)_saveData[22].Value; } }
                public static int NavDamage { get { return (int)_saveData[23].Value; } }
                public static string ParticleCannon { get { return (string)_saveData[40].Value; } }
                public static string BeamCannon { get { return (string)_saveData[41].Value; } }
                public static string ShipType { get { return (string)_saveData[58].Value; } }
                public static int EngineClass { get { return (int)_saveData[59].Value; } }
                public static int ShieldClass { get { return (int)_saveData[60].Value; } }
                public static int CargoCapacity { get { return (int)_saveData[61].Value; } }
                public static int WingClass { get { return (int)_saveData[62].Value; } }
                public static int ThrusterClass { get { return WingClass; } }
                public static int CrewLimit { get { return (int)_saveData[63].Value; } }
                public static int EquipentLimit { get { return (int)_saveData[64].Value; } }
                public static int CountermeasureLimit { get { return (int)_saveData[65].Value; } }
                public static int HarpointLimit { get { return (int)_saveData[66].Value; } }
                public static int ParticleCannonRange { get { return (int)_saveData[71].Value; } }
                public static int MissileRange { get { return (int)_saveData[72].Value; } }
                public static string TargetedSubsystem { get { return (string)_saveData[73].Value; } }
                public static int CounterMeasures { get { return (int)_saveData[80].Value; } }
                public static int IDSMultiplier { get { return (int)_saveData[84].Value; } }
                public static int Velocity { get { return (int)_saveData[94].Value; } }
                public static int SetVelocity { get { return (int)_saveData[95].Value; } }
                public static int Altitude { get { return (int)_saveData[96].Value; } }
                public static int HeatSignatureLevel { get { return (int)_saveData[98].Value; } }
                #endregion


                #region Functions
                /// <summary> Converts some data into a more convenient form.
                /// </summary>
                public static void Update()
                {
                    // Get cargo bays (lines 4-8)
                    for (int i = 3; i < 8; i++) { _cargoBay[i - 3] = (string)_saveData[i].Value; }

                    // Get position data (lines 9-11, 12-14)
                    _position.X = (int)_saveData[8].Value;
                    _position.Y = (int)_saveData[9].Value;
                    _position.Z = (int)_saveData[10].Value;
                    _sectorPosition.X = (int)_saveData[11].Value;
                    _sectorPosition.Y = (int)_saveData[12].Value;
                    _sectorPosition.Z = (int)_saveData[13].Value;

                    // Get shield levels (lines 17-21)
                    if (_shieldLevel.Count == 0)
                    {
                        _shieldLevel.Add(ShieldLevelState.FRONT, 0);
                        _shieldLevel.Add(ShieldLevelState.RIGHT, 0);
                        _shieldLevel.Add(ShieldLevelState.LEFT, 0);
                        _shieldLevel.Add(ShieldLevelState.REAR, 0);
                        _shieldLevel.Add(ShieldLevelState.TOTAL, 0);
                    }
                    _shieldLevel[ShieldLevelState.FRONT] = (int)_saveData[16].Value;
                    _shieldLevel[ShieldLevelState.RIGHT] = (int)_saveData[17].Value;
                    _shieldLevel[ShieldLevelState.LEFT] = (int)_saveData[18].Value;
                    _shieldLevel[ShieldLevelState.REAR] = (int)_saveData[19].Value;
                    _shieldLevel[ShieldLevelState.TOTAL] = (int)_saveData[20].Value;

                    // Get secondary weapons (lines 43-50)
                    for (int i = 42; i < 50; i++) { _secWeapon[i - 42] = (string)_saveData[i].Value; }

                    // Get equipment (lines 51-58)
                    for (int i = 50; i < 58; i++) { _equipmentSlot[i - 50] = (string)_saveData[i].Value; }

                    // Convert heat (line 78)
                    switch ((int)_saveData[77].Value)
                    {
                        case 0: _heat = HeatState.LOW; break;
                        case 1: _heat = HeatState.HIGH; break;
                    }

                    // Convert MTDS state (line 79)
                    switch ((int)_saveData[78].Value)
                    {
                        case 0: _mtds = MTDSState.OFF; break;
                        case 1: _mtds = MTDSState.ON; break;
                        case 2: _mtds = MTDSState.LOCKED; break;
                    }

                    // Convert missile lock state (line 80)
                    switch ((int)_saveData[79].Value)
                    {
                        case 0: _missileLock = MissileState.NO_LOCK; break;
                        case 1: _missileLock = MissileState.LOCKED; break;
                    }

                    // Set S/E bias (line 82)
                    string bias;
                    Regex biasRegex = new Regex(@"(?<Shields>[-+]?\d+?)S\/(?<Weapons>[-+]?\d+?)W");

                    bias = biasRegex.Match((string)_saveData[81].Value).Groups["Shields"].Value;
                    Int32.TryParse(bias, out _shieldBias);

                    bias = biasRegex.Match((string)_saveData[81].Value).Groups["Weapons"].Value;
                    Int32.TryParse(bias, out _weaponBias);

                    // Convert IDS state (line 84)
                    convertOnOffState((int)_saveData[83].Value, out _ids);

                    // Convert afterburner status (line 86)
                    convertOnOffState((int)_saveData[85].Value, out _afterburner);

                    // Convert autopilot status (line 87)
                    switch ((int)_saveData[86].Value)
                    {
                        case 0: _autopilot = AutopilotState.OFF; break;
                        case 1: _autopilot = AutopilotState.FORM_ON_TARGET; break;
                        case 2: _autopilot = AutopilotState.FLY_TO_NAV_POINT; break;
                    }

                    // Convert tractor beam status (line 91)
                    convertOnOffState((int)_saveData[90].Value, out _tractorBeam);
                }
                #endregion
            }


            /// <summary> Contains information about the player ship.
            /// </summary>
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
                public static string Description { get { return (string)_saveData[24].Value; } }
                public static string ThreatLevel { get { return (string)_saveData[25].Value; } }
                public static int Range { get { return (int)_saveData[26].Value; } }
                public static int EngineDamage { get { return (int)_saveData[31].Value; } }
                public static int WeaponDamage { get { return (int)_saveData[32].Value; } }
                public static int NavDamage { get { return (int)_saveData[33].Value; } }
                public static string Faction { get { return (string)_saveData[74].Value; } }
                public static int DamageLevel { get { return (int)_saveData[75].Value; } }
                public static int Velocity { get { return (int)_saveData[76].Value; } }
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
                    _shieldLevel[ShieldLevelState.FRONT] = (int)_saveData[27].Value;
                    _shieldLevel[ShieldLevelState.RIGHT] = (int)_saveData[28].Value;
                    _shieldLevel[ShieldLevelState.LEFT] = (int)_saveData[29].Value;
                    _shieldLevel[ShieldLevelState.REAR] = (int)_saveData[30].Value;

                    // Get cargo bays (lines 35-39)
                    for (int i = 34; i < 39; i++) { _cargoBay[i - 34] = (string)_saveData[i].Value; }
                }
                #endregion
            }


            /// <summary> Contains information about the player ship.
            /// </summary>
            public static class HudData
            {
                #region Enums
                public enum HudStatus { OFF = 0, PARTIAL = 1, FULL = 2 };
                public enum TargetDisplayStatus { DETAIL = 0, LIST = 1 };
                #endregion


                #region Variables - Converted values
                private static OnOffState _navigationConsole = OnOffState.OFF;
                private static OnOffState _inventoryConsole = OnOffState.OFF;
                private static OnOffState _tradeConsole = OnOffState.OFF;
                private static HudStatus _hud = HudStatus.OFF;
                private static TargetDisplayStatus _targetDisplay = TargetDisplayStatus.DETAIL;
                #endregion


                #region Properties - Converted values
                public static OnOffState NavigationConsole { get { return HudData._navigationConsole; } }
                public static OnOffState InventoryConsole { get { return HudData._inventoryConsole; } }
                public static OnOffState TradeConsole { get { return HudData._tradeConsole; } }
                public static HudStatus Hud { get { return HudData._hud; } }
                public static TargetDisplayStatus TargetDisplay { get { return HudData._targetDisplay; } }
                #endregion


                #region Properties - Unconverted Values
                public static int TotalHostilesOnRadar { get { return (int)_saveData[93].Value; } }
                #endregion


                #region Functions
                /// <summary> Converts some data into a more convenient form.
                /// </summary>
                public static void Update()
                {
                    // Convert navigation console status (line 88)
                    convertOnOffState((int)_saveData[87].Value, out _navigationConsole);

                    // Convert inventory console status (line 89)
                    convertOnOffState((int)_saveData[88].Value, out _inventoryConsole);

                    // Convert trade console status (line 90)
                    convertOnOffState((int)_saveData[89].Value, out _tradeConsole);

                    // Convert HUD status (line 92)
                    switch ((int)_saveData[91].Value)
                    {
                        case 0: _hud = HudStatus.OFF; break;
                        case 1: _hud = HudStatus.PARTIAL; break;
                        case 2: _hud = HudStatus.FULL; break;
                    }

                    // Convert target display status (line 93)
                    switch ((int)_saveData[92].Value)
                    {
                        case 0: _targetDisplay = TargetDisplayStatus.DETAIL; break;
                        case 1: _targetDisplay = TargetDisplayStatus.LIST; break;
                    }
                }
                #endregion
            }


            /// <summary> Contains information about the player ship.
            /// </summary>
            public static class EnvironmentData
            {
                #region Variables - Converted values
                private static OnOffState _inboundMissileAlert = OnOffState.OFF;
                private static int _navPointDistance = 0;
                #endregion


                #region Properties - Converted values
                public static OnOffState InboundMissileAlert { get { return EnvironmentData._inboundMissileAlert; } }
                public static int NavPointDistance { get { return EnvironmentData._navPointDistance; } }
                #endregion


                #region Properties - Unconverted values
                public static string LocalSystemName { get { return (string)_saveData[14].Value; } }
                public static int GravityLevel { get { return (int)_saveData[97].Value; } }
                #endregion


                #region Functions
                /// <summary> Converts some data into a more convenient form.
                /// </summary>
                public static void Update()
                {
                    // Convert inbound missile alert (line 40)
                    convertOnOffState((int)_saveData[39].Value, out _inboundMissileAlert);

                    // TODO: Actually convert it, chech how this value is being represented
                    // Convert waypoint distance (line 83)
                    Int32.TryParse((string)_saveData[82].Value, out _navPointDistance);
                }
                #endregion
            }
        }


        /// <summary> Contains information concerning Evochron Lore.
        /// </summary>
        public static class Lore
        {
            /// <summary> Contains information about in-game ships.
            /// </summary>
            public static class ShipData
            {

            }


            /// <summary> Contains information about in-game tech.
            /// </summary>
            public static class TechData
            {

            }


            /// <summary> Contains information about in-game items.
            /// </summary>
            public static class ItemData
            {

            }


            /// <summary> Contains information about (known) in-game systems.
            /// </summary>
            public static class SystemData
            {

            }


            /// <summary> Contains all registered commands.
            /// </summary>
            public static class Commands
            {

            }
        }
    }
}
