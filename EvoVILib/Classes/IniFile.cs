using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EvoVI
{
    public class IniFile
    {
        #region Constants
        private readonly Regex SECTION_VALIDATIOR = new Regex(@"^\s*\[\s*(?<Section>[A-Za-z0-9_ ]+)\s*\]\s*$");
        private readonly Regex KEY_VALUE_VALIDATIOR = new Regex(@"^\s*(?<Key>.*?)\s*=\s*(?<Value>.*?)\s*$");
        private readonly Regex VALUE_IS_BOOLEAN_VALIDATOR = new Regex(@"^\s*(true|false)\s*$", RegexOptions.IgnoreCase);
        #endregion


        #region Variables
        private string _filepath;
        private Dictionary<string, Dictionary<string, KeyValuePair<string, string>>> _sections;
        #endregion


        #region Properties
        /// <summary> Returns or sets the target filepath.
        /// </summary>
        public string Filepath
        {
            get { return _filepath; }
            set { _filepath = value; }
        }


        /// <summary> Returns the section dictionary, containing the key-value pairs, indexed by the key.
        /// </summary>
        public Dictionary<string, Dictionary<string, KeyValuePair<string, string>>> Sections
        {
            get { return _sections; }
        }
        #endregion


        #region Constructor
        /// <summary> Creates a new INI file instance.
        /// </summary>
        /// <param name="pFilepath">The path to the target ini file.</param>
        public IniFile(string pFilepath)
        {
            this._filepath = pFilepath;
            this._sections = new Dictionary<string, Dictionary<string, KeyValuePair<string, string>>>();
        }
        #endregion


        #region Functions
        /// <summary> Reads the file and fills the database.
        /// </summary>
        public void Read()
        {
            _sections.Clear();

            if (!File.Exists(_filepath)) { return; }

            string[] fileContent = File.ReadAllLines(this._filepath);

            string currSection = String.Empty;
            for (int i = 0; i < fileContent.Length; i++)
            {
                string currLine = fileContent[i].Contains(';') ? fileContent[i].Substring(0, fileContent[i].IndexOf(';')) : fileContent[i];

                if (SECTION_VALIDATIOR.IsMatch(currLine))
                {
                    currSection = SECTION_VALIDATIOR.Match(currLine).Groups["Section"].Value.ToLower();

                    if (_sections.ContainsKey(currSection))
                    {
                        // Ovewrite section (clear all)
                        _sections[currSection].Clear();
                    }
                    else
                    {
                        // Add new section
                        _sections.Add(currSection, new Dictionary<string, KeyValuePair<string, string>>());
                    }
                }
                else if (
                    (!String.IsNullOrEmpty(currSection)) &&
                    (_sections.ContainsKey(currSection)) &&
                    (KEY_VALUE_VALIDATIOR.IsMatch(currLine))
                )
                {
                    Match match = KEY_VALUE_VALIDATIOR.Match(currLine);
                    string key = match.Groups["Key"].Value.ToLower();
                    string value = match.Groups["Value"].Value.ToLower();

                    if (_sections[currSection].ContainsKey(key))
                    {
                        // Ovewrite keyval
                        _sections[currSection][key] = new KeyValuePair<string, string>(key, value);
                    }
                    else
                    {
                        // Add new keyval
                        _sections[currSection].Add(key, new KeyValuePair<string, string>(key, value));
                    }
                }
            }
        }


        /// <summary> Checks whether the specified parameter is defined within the given section.
        /// </summary>
        /// <param name="section">The section in which the value is located.</param>
        /// <param name="key">The key to the value.</param>
        /// <returns>The value or an empty string on failure.</returns>
        public bool HasKey(string section, string key)
        {
            section = section.ToLower();
            key = key.ToLower();

            return (_sections.ContainsKey(section) && _sections[section].ContainsKey(key));
        }


        /// <summary> Gets the specified entry from the database.
        /// </summary>
        /// <param name="section">The section in which the value is located.</param>
        /// <param name="key">The key to the value.</param>
        /// <returns>The value or an empty string on failure.</returns>
        public string GetValue(string section, string key)
        {
            section = section.ToLower();
            key = key.ToLower();

            return (
                (
                    _sections.ContainsKey(section) &&
                    _sections[section].ContainsKey(key)
                ) ?
                _sections[section][key].Value : String.Empty
            );
        }


        /// <summary> Check whether the value resembles a boolean value and if it is true.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the value is "true".</returns>
        public bool ValueIsBoolAndTrue(string value)
        {
            return (
                (VALUE_IS_BOOLEAN_VALIDATOR.IsMatch(value)) &&
                (VALUE_IS_BOOLEAN_VALIDATOR.Match(value).Groups[0].Value.ToLower() == "true")
            );
        }


        /// <summary> Check whether the value resembles a boolean value and if it is true.
        /// </summary>
        /// <param name="section">The section in which the value is located.</param>
        /// <param name="key">The key to the value.</param>
        /// <returns>Whether the value is "true".</returns>
        public bool ValueIsBoolAndTrue(string section, string key)
        {
            return ValueIsBoolAndTrue(this.GetValue(section, key));
        }
        #endregion
    }
}
