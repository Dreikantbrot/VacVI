using EvoVI.Engine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EvoVI.Database
{
    #region Enums
    public enum OnOffState { OFF = 0, ON = 1 };
    public enum ShieldLevelState { FRONT, RIGHT, LEFT, REAR, TOTAL };
    #endregion


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


    /// <summary> Reads, parses and stores data from savedata.txt.
    /// </summary>
    public static class SaveDataReader
    {
        #region Regexes (readonly)
        readonly static Regex SLOT_ID_REGEX = new Regex(@"<(?<DataType>.*?)>\s*(?<ParamName>.*?)(?:\r|\n|$)");
        #endregion


        #region Variables
        private static List<dataEntry> _saveData = new List<dataEntry>();
        #endregion


        #region Properties
        /// <summary> Returns the entire current database.
        /// </summary>
        internal static List<dataEntry> SaveData
        {
            get { return SaveDataReader._saveData; }
        }
        #endregion


        #region Functions
        public static void ConvertOnOffState(int val, out OnOffState var)
        {
            var = (val == 1) ? OnOffState.ON : OnOffState.OFF;
        }


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
            EnvironmentalData.Update();

            // Call OnGameDataUpdate on all plugins
            for (int i = 0; i < PluginLoader.Plugins.Count; i++) { PluginLoader.Plugins[i].OnGameDataUpdate(); }
            return true;
        }
        #endregion
    }
}
