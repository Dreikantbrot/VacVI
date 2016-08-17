using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EvoVI.engine
{
    public static class Database
    {
        /// <summary> Contains in-game data gathered from "savedata.txt".
        /// </summary>
        public static class InGameData
        {
            #region Regexes (readonly)
            readonly static Regex SLOT_ID_REGEX = new Regex(@"<(?<DataType>.*?)>\s*(?<ParamName>.*?)(?:\r|\n|$)");
            #endregion


            #region Private Classes
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
                        switch (this._type)
                        {
                            case DataType.INTEGER: return _intVal;
                            case DataType.STRING: return _strVal;

                            default: return null;
                        }
                    }
                }
                #endregion


                /// <summary> Creates a data entry instance holding a single entry from the savedata.txt file.
                /// </summary>
                /// <param name="pName">The parameter name.</param>
                /// <param name="pType">The parameter's data type</param>
                public dataEntry(string pName, string pType)
                {
                    this._id = pName;

                    switch (pType.ToLower())
                    {
                        case "integer":
                            this._type = DataType.INTEGER;
                            break;
                        case "string":
                            this._type = DataType.STRING;
                            break;

                        default:
                            this._type = DataType.UNKNOWN;
                            break;
                    }
                }


                #region Public Functions
                public void SetValue(string val) { this._strVal = val; }
                public void SetValue(int val) { this._intVal = val; }
                #endregion
            }
            #endregion


            #region Variables
            private static List<dataEntry> _inGameData = new List<dataEntry>();
            #endregion


            #region Public Functions
            /// <summary> Initializes the VI itself.
            /// </summary>
            public static void Initialize()
            {
                _inGameData.Clear();

                /* Read savedata template and parse parameters */
                string[] savedataTemplate = EvoVI.Properties.Resources.savedata_template.Split('\n');

                // Build the database
                for (int i = 0; i < savedataTemplate.Length; i++)
                {
                    string currLine = savedataTemplate[i];
                    Match match = SLOT_ID_REGEX.Match(currLine);
                    _inGameData.Add(new dataEntry(match.Groups["ParamName"].Value, match.Groups["DataType"].Value));
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
                for (int i = 0; i < Math.Min(fileContent.Length, _inGameData.Count); i++)
                {
                    string currVal = fileContent[i];
                    currVal = fileContent[i].Trim();

                    switch (_inGameData[i].Type)
                    {
                        case dataEntry.DataType.INTEGER:
                            int intValue;
                            Int32.TryParse(currVal, out intValue);
                            _inGameData[i].SetValue(intValue);
                            break;

                        case dataEntry.DataType.STRING:
                            _inGameData[i].SetValue(currVal);
                            break;

                        default: break;
                    }
                }

                return true;
            }
            #endregion
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
