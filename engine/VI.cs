﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Evo_VI.engine
{
    public static class VI
    {
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
                    switch(this._type)
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
                
                switch(pType.ToLower())
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
        private static List<dataEntry> _saveDataLayout = new List<dataEntry>();
        #endregion


        #region Public Functions 
        /// <summary> Initializes the VI itself.
        /// </summary>
        public static void Initialize()
        {
            _saveDataLayout.Clear();

            /* Read savedata template and parse file order */
            string[] savedataTemplate = Evo_VI.Properties.Resources.savedata_template.Split('\n');

            Regex SlotIdRegex = new Regex(@"<(.*?)>\s*(.*?)(?:\r|\n|$)");

            for (int i = 0; i < savedataTemplate.Length; i++)
            {
                string currLine = savedataTemplate[i];
                Match match = SlotIdRegex.Match(currLine);
                _saveDataLayout.Add(new dataEntry(match.Groups[2].Value, match.Groups[1].Value));
            }
        }


        /// <summary> Reads and stores the contents of the savedata.txt file.
        /// </summary>
        /// <param name="filepath">The filepath to savedata.txt.</param>
        /// <returns>Whether information has been retrieved successfully.</returns>
        public static bool ReadGameData(string filepath = "C:\\sw3dg\\EvochronMercenary\\savedata.txt")
        {
            string[] fileContent;

            try
            {
                fileContent = System.IO.File.ReadAllLines(filepath);
            }
            catch (System.IO.FileNotFoundException)
            {
                SpeechEngine.Say(Evo_VI.Properties.StringTable.COULD_NOT_ACQUIRE_DATA);
                return false;
            }

            for (int i = 0; i < Math.Min(fileContent.Length, _saveDataLayout.Count); i++)
            {
                string currVal = fileContent[i];
                currVal = fileContent[i].Trim();

                switch(_saveDataLayout[i].Type)
                {
                    case dataEntry.DataType.INTEGER:
                        int intValue;
                        Int32.TryParse(currVal, out intValue);
                        _saveDataLayout[i].SetValue(intValue);
                        break;
                    
                    case dataEntry.DataType.STRING:
                        _saveDataLayout[i].SetValue(currVal);
                        break;

                    default: break;
                }
            }

            return true;
        }
        #endregion
    }
}
