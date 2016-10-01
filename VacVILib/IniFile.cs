using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VacVI
{
    /// <summary> A class used for reading and writing INI files.</summary>
    public class IniFile
    {
        #region Constants
        /// <summary> Regex that identifies a section in an INI file.</summary>
        private readonly Regex SECTION_VALIDATOR = new Regex(@"^\s*\[\s*(?<Section>[A-Za-z0-9_\- ]+)\s*\]\s*$");

        /// <summary> Regex that identifies a key-value pair in an INI file.</summary>
        private readonly Regex KEY_VALUE_VALIDATIOR = new Regex(@"^\s*(?<Key>.*?)\s*=\s*(?<Value>.*?)\s*$");

        /// <summary> Regex that identifies a stringified boolean value.</summary>
        private readonly Regex VALUE_IS_BOOLEAN_VALIDATOR = new Regex(@"^\s*(true|false)\s*$", RegexOptions.IgnoreCase);
        #endregion


        #region Variables
        private string _filepath;
        private Dictionary<string, Dictionary<string, string>> _sections;
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
        public Dictionary<string, Dictionary<string, string>> Sections
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
            this._sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);
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

                if (SECTION_VALIDATOR.IsMatch(currLine))
                {
                    currSection = SECTION_VALIDATOR.Match(currLine).Groups["Section"].Value;

                    if (_sections.ContainsKey(currSection))
                    {
                        // Ovewrite section (clear all)
                        _sections[currSection].Clear();
                    }
                    else
                    {
                        // Add new section
                        _sections.Add(currSection, new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase));
                    }
                }
                else if (
                    (!String.IsNullOrEmpty(currSection)) &&
                    (_sections.ContainsKey(currSection)) &&
                    (KEY_VALUE_VALIDATIOR.IsMatch(currLine))
                )
                {
                    Match match = KEY_VALUE_VALIDATIOR.Match(currLine);
                    string key = match.Groups["Key"].Value;
                    string value = match.Groups["Value"].Value;

                    if (_sections[currSection].ContainsKey(key))
                    {
                        // Ovewrite keyval
                        _sections[currSection][key] = value;
                    }
                    else
                    {
                        // Add new keyval
                        _sections[currSection].Add(key, value);
                    }
                }
            }
        }


        /// <summary> Writes the file to the specified loaction.
        /// </summary>
        /// <param name="filepath">The destination filepath.</param>
        public void Write(string filepath)
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, Dictionary<string, string>> section in _sections)
            {
                builder.Append("[");
                builder.Append(section.Key);
                builder.Append("]\n");

                foreach (KeyValuePair<string, string> keyVals in section.Value)
                {
                    builder.Append(keyVals.Key);
                    builder.Append("=");
                    builder.Append(keyVals.Value);
                    builder.Append("\n");
                }

                builder.Append("\n");
            }

            File.WriteAllText(filepath, builder.ToString());
        }


        /// <summary> Updates the currently saved configuration file only changing the given value, without changing the current configuration.
        /// </summary>
        /// <param name="section">The section in which the value is located.</param>
        /// <param name="key">The key to the value.</param>
        /// <param name="value">The new value.</param>
        /// <param name="filepath">The destination filepath.</param>
        public void SaveValue(string section, string key, string value, string filepath = null)
        {
            IniFile currFile = new IniFile(_filepath);
            currFile.Read();
            currFile.SetValue(section, key, value);
            currFile.Write((filepath == null) ? _filepath : filepath);
        }


        /// <summary> Checks whether the specified parameter is defined within the given section.
        /// </summary>
        /// <param name="section">The section in which the value is located.</param>
        /// <param name="key">The key to the value.</param>
        /// <returns>Whether the attribute is available.</returns>
        public bool HasKey(string section, string key)
        {
            return (_sections.ContainsKey(section) && _sections[section].ContainsKey(key));
        }


        /// <summary> Gets the specified entry from the database.
        /// </summary>
        /// <param name="section">The section in which the value is located.</param>
        /// <param name="key">The key to the value.</param>
        /// <returns>The value or an empty string on failure.</returns>
        public string GetValue(string section, string key)
        {
            return (
                (
                    _sections.ContainsKey(section) &&
                    _sections[section].ContainsKey(key)
                ) ?
                _sections[section][key] : String.Empty
            );
        }


        /// <summary> Checks whether the specified section is defined within the file.
        /// </summary>
        /// <param name="section">The section to check for..</param>
        /// <returns>Whether the section is available.</returns>
        public bool HasSection(string section)
        {
            return (_sections.ContainsKey(section));
        }


        /// <summary> Gets the specified section entries from the database.
        /// </summary>
        /// <param name="section">The section in which the value is located.</param>
        /// <returns>The attribute dictionary or null on failure.</returns>
        public Dictionary<string, string> GetSectionAttributes(string section)
        {
            return (_sections.ContainsKey(section) ? _sections[section] : null);
        }


        /// <summary> Gets the specified entry from the database.
        /// </summary>
        /// <param name="section">The section in which the value is located.</param>
        /// <param name="key">The key to the value.</param>
        /// <param name="value">The new value.</param>
        public void SetValue(string section, string key, string value)
        {
            if (!_sections.ContainsKey(section)) { _sections.Add(section, new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)); }
            if (!_sections[section].ContainsKey(key)) { _sections[section].Add(key, value); } else { _sections[section][key] = value; }
        }


        /// <summary> Check whether the value resembles a boolean value and if it is true.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the value is "true".</returns>
        public bool ValueIsBoolAndTrue(string value)
        {
            return (
                (VALUE_IS_BOOLEAN_VALIDATOR.IsMatch(value)) &&
                (String.Equals(VALUE_IS_BOOLEAN_VALIDATOR.Match(value).Groups[0].Value, "true", StringComparison.InvariantCultureIgnoreCase))
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
