using EvoVI.Database;
using EvoVI.Dialog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace EvoVI
{
    /// <summary> Contains and manages the program's main configuration (confguration.ini).</summary>
    public static class ConfigurationManager
    {
        #region Constants
        /// <summary> The configuration file's name.</summary>
        private const string CONFIGURATION_FILENAME = "configuration.ini";

        /// <summary> The name for the section "Game" within the config file.</summary>
        public const string SECTION_GAME = "Game";

        /// <summary> The name for the section "Filepaths" within the config file.</summary>
        public const string SECTION_FILEPATHS = "Filepaths";

        /// <summary> The name for the section "Overlay" within the config file.</summary>
        public const string SECTION_OVERLAY = "Overlay";

        /// <summary> The name for the section "VI" within the config file.</summary>
        public const string SECTION_VI = "VI";

        /// <summary> The name for the section "Player" within the config file.</summary>
        public const string SECTION_PLAYER = "Player";
        #endregion


        #region Variables
        private static IniFile _configurationFile = new IniFile(ConfigurationFilepath);
        #endregion


        #region Properties
        /// <summary> Returns the configuration file's filepath.
        /// </summary>
        public static string ConfigurationFilepath
        {
            get { return GetConfigurationPath() + "\\" + CONFIGURATION_FILENAME; }
        }


        /// <summary> Returns or sets the configuration file.
        /// </summary>
        public static IniFile ConfigurationFile
        {
            get { return _configurationFile; }
            internal set { _configurationFile = value; }
        }
        #endregion


        #region Functions
        /// <summary> Returns the directory path to the configuration file.
        /// </summary>
        /// <returns>The directory path.</returns>
        internal static string GetConfigurationPath()
        {
            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");
            return appPath;
        }


        /// <summary> Reads the configuration file and assigns all values defined in it.
        /// </summary>
        internal static void LoadConfiguration()
        {
            string section = "";
            _configurationFile.Read();

            /* "Game" section */
            section = SECTION_GAME;
            if (_configurationFile.HasSection(section))
            {
                if (_configurationFile.HasKey(section, "Current_Game"))
                {
                    GameMeta.SupportedGame currentGame;
                    Enum.TryParse(_configurationFile.GetValue(section, "Current_Game"), true, out currentGame);

                    GameMeta.CurrentGame = currentGame;
                }
            }


            /* "Filepaths" section */
            section = SECTION_FILEPATHS;
            if (_configurationFile.HasSection(section))
            {
                Dictionary<string, string> filepaths = _configurationFile.GetSectionAttributes(section);
                foreach (KeyValuePair<string, string> GamePathPair in filepaths)
                {
                    GameMeta.SupportedGame currentGame;
                    Enum.TryParse(GamePathPair.Key, true, out currentGame);

                    if (GameMeta.GameDetails.ContainsKey(currentGame)) { GameMeta.GameDetails[currentGame].UserInstallDirectory = GamePathPair.Value; }
                }
            }


            /* "Overlay" section */
            section = SECTION_OVERLAY;
            if (_configurationFile.HasSection(section))
            {
                if (_configurationFile.HasKey(section, "Play_Intro")) { /* Handled externally by the overlay itself */ }
            }


            /* "VI" section */
            section = SECTION_VI;
            if (_configurationFile.HasSection(section))
            {
                // VI Name
                if (
                    (_configurationFile.HasKey(section, "Name")) &&
                    (!String.IsNullOrWhiteSpace(_configurationFile.GetValue(section, "Name")))
                )
                { VI.Name = _configurationFile.GetValue(section, "Name"); }

                // VI Phonetic Name
                if (
                    (_configurationFile.HasKey(section, "Phonetic_Name")) &&
                    (!String.IsNullOrWhiteSpace(_configurationFile.GetValue(section, "Phonetic_Name")))
                )
                { VI.PhoneticName = _configurationFile.GetValue(section, "Phonetic_Name"); }

                // VI Voice
                if (_configurationFile.HasKey(section, "Voice"))
                {
                    SpeechEngine.VoiceModulationModes chosenVoice;
                    Enum.TryParse(_configurationFile.GetValue(section, "Voice"), true, out chosenVoice);

                    SpeechEngine.VoiceModulation = chosenVoice;
                }

                // Speech recognition language
                if (
                    (_configurationFile.HasKey(section, "Speech_Recognition_Lang")) &&
                    (!String.IsNullOrWhiteSpace(_configurationFile.GetValue(section, "Speech_Recognition_Lang")))
                )
                { SpeechEngine.Language = _configurationFile.GetValue(section, "Speech_Recognition_Lang"); }
            }


            /* "Player" section */
            section = SECTION_PLAYER;
            if (_configurationFile.HasSection(section))
            {
                if (_configurationFile.HasKey(section, "Name")) { VI.PlayerName = _configurationFile.GetValue(section, "Name"); }
                if (_configurationFile.HasKey(section, "Phonetic_Name")) { VI.PlayerPhoneticName = _configurationFile.GetValue(section, "Phonetic_Name"); }
            }
        }
        #endregion
    }
}
