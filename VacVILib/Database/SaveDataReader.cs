using VacVI.Dialog;
using VacVI.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace VacVI.Database
{
    #region Enums (shared between game databases)
    /// <summary> Represents an on/off state.</summary>
    public enum OnOffState { OFF = 0, ON = 1 };

    /// <summary> Represents a shield section.</summary>
    public enum ShieldLevelState { FRONT, RIGHT, LEFT, REAR, TOTAL };

    /// <summary> Represents the threat level.</summary>
    public enum ThreatLevelState { LOW = 0, MED = 1, HIGH = 2 };
    #endregion


    /// <summary> Contains general ship-related values for ship databases.</summary>
    internal static class ShipData
    {
        #region Properties (shared between game databases)
        /// <summary> Returns the maximum number of cargo slots possible.</summary>
        public static int MaxCargoSlots
        {
            get
            {
                return (
                    (GameMeta.CurrentGame == (GameMeta.CurrentGame & GameMeta.SupportedGame.EVOCHRON_MERCENARY)) ? 5 :
                    (GameMeta.CurrentGame == (GameMeta.CurrentGame & GameMeta.SupportedGame.EVOCHRON_LEGACY)) ? 10 :
                    0
                );
            }
        }


        /// <summary> Returns the maximum number of equipment slots possible.</summary>
        public static int MaxEquipmentSlots
        {
            get
            {
                return (
                    (GameMeta.CurrentGame == (GameMeta.CurrentGame & GameMeta.SupportedGame.EVOCHRON_MERCENARY)) ? 8 :
                    (GameMeta.CurrentGame == (GameMeta.CurrentGame & GameMeta.SupportedGame.EVOCHRON_LEGACY)) ? 10 :
                    0
                );
            }
        }


        /// <summary> Returns the maximum number of capital ship turrets possible.</summary>
        public static int MaxCapShipTurrets
        {
            get
            {
                return (
                    (GameMeta.CurrentGame == (GameMeta.CurrentGame & GameMeta.SupportedGame.EVOCHRON_MERCENARY)) ? 0 :
                    (GameMeta.CurrentGame == (GameMeta.CurrentGame & GameMeta.SupportedGame.EVOCHRON_LEGACY)) ? 4 :
                    0
                );
            }
        }


        
        #endregion
    }


    /// <summary> Reads, parses and stores data from "savedata.txt".</summary>
    internal static class SaveDataReader
    {
        #region Classes
        [System.Diagnostics.DebuggerDisplay("<{_type}> {_name}")]
        public class DataEntry
        {
            #region Enum
            /// <summary> Known data types within savedata.txt.
            /// </summary>
            public enum DataType { INTEGER, STRING, FLOAT, UNKNOWN };
            #endregion


            #region Variables
            private string _name;
            private DataType _type;
            private int _fileIndex;
            private GameMeta.SupportedGame _gameFlags;
            private string _category;
            private string _paramRemarks;

            private string _strVal;
            private int _intVal;
            private float _floatVal;
            #endregion


            #region Properties
            /// <summary> Returns the entrie's data type (see savedata.txt).
            /// </summary>
            public DataType Type
            {
                get { return _type; }
            }


            /// <summary> Returns the index of the parameter within savedata_template.
            /// </summary>
            public int FileIndex
            {
                get { return _fileIndex; }
            }


            /// <summary> Returns the parameter's game flags, indicating game compatbility.
            /// </summary>
            public GameMeta.SupportedGame GameFlags
            {
                get { return _gameFlags; }
            }


            /// <summary> Returns the parameter category.
            /// </summary>
            public string Category
            {
                get { return _category; }
            }


            /// <summary> Returns or sets parameter remarks.
            /// </summary>
            public string ParamRemarks
            {
                get { return _paramRemarks; }
                set { _paramRemarks = value; }
            }


            /// <summary> Returns the entry's value in it's appropriate format (see savedata.txt).
            /// </summary>
            public object Value
            {
                get
                {
                    switch (_type)
                    {
                        case DataType.INTEGER: return _intVal;
                        case DataType.FLOAT: return _floatVal;
                        case DataType.STRING: return _strVal;

                        default: return null;
                    }
                }
            }
            #endregion


            #region Constructor
            /// <summary> Creates a data entry instance holding a single entry from the savedata.txt file.
            /// <para>See: <see cref="VacVI.Properties.Resources.Savedata_Layout"/></para>
            /// </summary>
            /// <param name="pName">The parameter name.</param>
            /// <param name="pType">The parameter's data type.</param>
            /// <param name="pFileIndex">The parameter's index wthin the template file.</param>
            /// <param name="pGameFlags">The flags indicating which games support this parameter.</param>
            /// <param name="pCategory">The parameter category.</param>
            public DataEntry(string pName, string pType, int pFileIndex, GameMeta.SupportedGame pGameFlags, string pCategory)
            {
                _name = pName;
                _fileIndex = pFileIndex;
                _gameFlags = pGameFlags;
                _category = pCategory;

                switch (pType.ToLower())
                {
                    case "integer":
                        _type = DataType.INTEGER;
                        break;
                    case "float":
                        _type = DataType.FLOAT;
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
            public void SetValue(float val) { _floatVal = val; }
            #endregion
        }
        #endregion


        #region Regexes (readonly)
        private readonly static Regex SAVEDATA_TEMPLATE_REGEX = new Regex(
            @"\[\s*(?<Availability>.*?)\s*\]\s*" + 
            @"\[\s*(?<Category>.*?)\s*\]\s*" + 
            @"<(?<DataType>.*?)>\s*" + 
            @"(?<ParamName>.*?)\s*" + 
            @"(?<Remarks>\(.*?\))?" + 
            @"(?:\r|\n|$)"
        );
        #endregion


        #region Variables
        private static Dictionary<string, DataEntry> _saveData = new Dictionary<string, DataEntry>();
        private static Dictionary<int, DataEntry> _indexTable = new Dictionary<int, DataEntry>();
        private static DateTime _lastUpdateTime = DateTime.Now;
        private static int _updateInterval = -1;
        #endregion


        #region Properties
        /// <summary> Returns the id lookup table.
        /// </summary>
        internal static Dictionary<string, DataEntry> SaveData
        {
            get { return SaveDataReader._saveData; }
        }


        /// <summary> Returns the update interval, as it is set within the "savedatasettings.txt".
        /// </summary>
        public static int UpdateInterval
        {
            get { return SaveDataReader._updateInterval; }
        }


        /// <summary> Returns the last/previous time the gamedata has been updated.
        /// </summary>
        public static DateTime LastUpdateTime
        {
            get { return SaveDataReader._lastUpdateTime; }
        }
        #endregion


        #region Functions
        /// <summary> Initializes the savedata database.
        /// </summary>
        internal static void BuildDatabase()
        {
            _saveData.Clear();
            _indexTable.Clear();

            /* Read savedata template and parse parameters;
             * Template is based upon Evochron Legacy as it's just an expansion on all the others
             */
            string[] savedataTemplate = VacVI.Properties.Resources.Savedata_Layout.Split('\n');

            // Build the database
            int currDbIndex = 0;
            for (int i = 0; i < savedataTemplate.Length; i++)
            {
                string currLine = savedataTemplate[i];
                Match match = SAVEDATA_TEMPLATE_REGEX.Match(currLine);

                if (!match.Success) { continue; }
                
                GameMeta.SupportedGame availabilityFlags = GameMeta.SupportedGame.NONE;
                if (match.Groups["Availability"].Value.Contains("EMERC")) { availabilityFlags |= GameMeta.SupportedGame.EVOCHRON_MERCENARY; }
                if (match.Groups["Availability"].Value.Contains("ELGCY")) { availabilityFlags |= GameMeta.SupportedGame.EVOCHRON_LEGACY; }

                // Is the parameter supported by the currently set game?
                if ((availabilityFlags & GameMeta.CurrentGame) != GameMeta.CurrentGame) { continue; }

                DataEntry newEntry = new DataEntry(
                    match.Groups["ParamName"].Value,
                    match.Groups["DataType"].Value.ToLower(), 
                    i,
                    availabilityFlags,
                    match.Groups["Category"].Value
                );
                newEntry.ParamRemarks = match.Groups["Remarks"].Value;

                _saveData.Add(match.Groups["ParamName"].Value.ToUpper(), newEntry);
                _indexTable.Add(currDbIndex, newEntry);

                currDbIndex++;
            }
        }


        /// <summary> Converts the specified integer value into the respective OnOffState type (0 = off, 1 = on).
        /// </summary>
        /// <param name="val">The variables current integer state.</param>
        /// <param name="var">The variable which to assing the on/off state to.</param>
        internal static void ConvertOnOffState(int val, out OnOffState var)
        {
            var = (val == 1) ? OnOffState.ON : OnOffState.OFF;
        }


        /// <summary> Reads and stores the contents of the savedata.txt file.
        /// </summary>
        /// <returns>Whether information has been retrieved successfully.</returns>
        internal static bool ReadGameData()
        {
            // Determine the update interval, if not already done
            if (
                (_updateInterval < 0) &&
                (File.Exists(GameMeta.CurrentGameDirectoryPath + "\\" + GameMeta.SAVEDATA_SETTINGS_FILENAME))
            )
            {
                Int32.TryParse(
                    File.ReadAllText(GameMeta.CurrentGameDirectoryPath + "\\" + GameMeta.SAVEDATA_SETTINGS_FILENAME),
                    out _updateInterval
                );
            }

            string[] fileContent;
            string filepath = GameMeta.DefaultSavedataPath;

            // Try to read the file
            if (!File.Exists(filepath))
            {
                SpeechEngine.Say(VacVI.Properties.StringTable.COULD_NOT_ACQUIRE_DATA);
                return false;
            }

            try { fileContent = File.ReadAllLines(filepath); }
            catch (System.IO.IOException) { return false; }

            // File exists, but is empty
            if (
                (fileContent.Length == 1) &&
                (Regex.IsMatch(fileContent[0], @"\0"))
            )
            { return false; }

            // Get all in-game values and store them inside the database
            for (int i = 0; i < Math.Min(fileContent.Length, _saveData.Count); i++)
            {
                string currVal = fileContent[i];
                currVal = fileContent[i].Trim();

                // Convert the data into an appropriate type
                switch (_indexTable[i].Type)
                {
                    case DataEntry.DataType.INTEGER:
                        int intValue;
                        int.TryParse(currVal, out intValue);
                        _indexTable[i].SetValue(intValue);
                        break;

                    case DataEntry.DataType.FLOAT:
                        float floatValue;
                        float.TryParse(currVal, out floatValue);
                        _indexTable[i].SetValue(floatValue);
                        break;

                    case DataEntry.DataType.STRING:
                        _indexTable[i].SetValue(currVal);
                        break;

                    default: break;
                }
            }

            PlayerData.Update();
            PlayerShipData.Update();
            HudData.Update();
            TargetShipData.Update();
            EnvironmentalData.Update();

            // Call IPlugin.OnGameDataUpdate on all plugins
            PluginManager.CallGameDataUpdateOnPlugins();

            // Update the last update time
            _lastUpdateTime = DateTime.Now;
            return true;
        }


        /// <summary> Gets an entry by it's parameter name.
        /// </summary>
        /// <param name="paramName">The name of the parameter, of which to retrieve the entry (must be upper case!).</param>
        /// <returns>The entry or null on error.</returns>
        internal static DataEntry GetEntry(string paramName)
        {
            return _saveData.ContainsKey(paramName) ? _saveData[paramName] : null;
        }
        #endregion
    }
}
