using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EvoVI.Database
{
    /// <summary> Contains information about the game itself.
    /// </summary>
    public static class GameMeta
    {
        #region Enums
        [Flags]
        public enum SupportedGame {
            [Description("None")]
            NONE = 0,
            [Description("Evochron Mercenary")]
            EVOCHRON_MERCENARY = 1,
            [Description("Evochron Legacy")]
            EVOCHRON_LEGACY = 2
        };
        #endregion


        #region Constants
        public const string DEFAULT_GAME_PATH = @"C:\sw3dg";
        public const string DEFAULT_SAVEDATA_PATH = @"C:\sw3dg";
        public const string DEFAULT_SAVEDATA_FILENAME = "savedata.txt";
        public const string DEFAULT_GAMECONFIG_FILENAME = "sw.cfg";
        #endregion


        #region Variables
        private static SupportedGame _currentGame = SupportedGame.EVOCHRON_MERCENARY;
        private static Dictionary<SupportedGame, string> _folderNames = new Dictionary<SupportedGame, string>();
        private static Dictionary<SupportedGame, string> _installDirectories = new Dictionary<SupportedGame, string>();
        #endregion


        #region Properties
        /// <summary> Returns or sets the currently active game.
        /// </summary>
        public static SupportedGame CurrentGame
        {
            get { return _currentGame; }
            set { _currentGame = value; }
        }


        /// <summary> Returns or sets the current game's path.
        /// </summary>
        public static string CurrentGamePath
        {
            get { return _installDirectories[_currentGame]; }
            set { _installDirectories[_currentGame] = value; }
        }


        /// <summary> Returns or sets the game install directory table.
        /// </summary>
        public static Dictionary<SupportedGame, string> InstallDirectories
        {
            get { return GameMeta._installDirectories; }
            set { GameMeta._installDirectories = value; }
        }


        /// <summary> Returns the current game's default folder name.
        /// </summary>
        public static string CurrentGameDefaultFolderName
        {
            get { return _folderNames[_currentGame]; }
        }


        /// <summary> Returns or sets the current game's default savedata directory path.
        /// </summary>
        public static string DefaultSavedataDirectoryPath
        {
            get { return DEFAULT_SAVEDATA_PATH + "\\" + CurrentGameDefaultFolderName; }
        }


        /// <summary> Returns or sets the current game's default configuration directory path.
        /// </summary>
        public static string DefaultGameSettingsDirectoryPath
        {
            get { return DEFAULT_SAVEDATA_PATH + "\\" + CurrentGameDefaultFolderName; }
        }


        /// <summary> Returns or sets the current game's default savedata file path.
        /// </summary>
        public static string DefaultSavedataPath
        {
            get { return GameMeta.DefaultSavedataDirectoryPath + "\\" + DEFAULT_SAVEDATA_FILENAME; }
        }


        /// <summary> Returns or sets the current game's default configuration file path.
        /// </summary>
        public static string DefaultGameSettingsPath
        {
            get { return GameMeta.DefaultGameSettingsDirectoryPath + "\\" + DEFAULT_GAMECONFIG_FILENAME; }
        }
        #endregion


        #region (Static) Constructor
        static GameMeta()
        {
            _folderNames.Add(SupportedGame.EVOCHRON_MERCENARY, "EvochronMercenary");
            _folderNames.Add(SupportedGame.EVOCHRON_LEGACY, "EvochronLegacy");

            _installDirectories.Add(SupportedGame.NONE, String.Empty);
            _installDirectories.Add(SupportedGame.EVOCHRON_MERCENARY, String.Empty);
            _installDirectories.Add(SupportedGame.EVOCHRON_LEGACY, String.Empty);
        }
        #endregion


        #region Functions
        /// <summary> Gets the description tag entry from the SupportedGame enum.
        /// </summary>
        /// <param name="game">The enum flagfor which to get the description for.</param>
        /// <returns>The description (the game's name).</returns>
        public static string GetDescription(SupportedGame game)
        {
            Type valType = game.GetType();
            string name = Enum.GetName(valType, game);

            if (name != null)
            {
                FieldInfo field = valType.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field,typeof(DescriptionAttribute));
                    
                    if (attribute != null) { return attribute.Description; }
                }
            }
            return null;
        }
        #endregion
    }
}
