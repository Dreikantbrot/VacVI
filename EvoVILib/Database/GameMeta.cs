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
        #region Structs
        public class GameInfo
        {
            #region Variables
            private SupportedGame _gameType;
            private string _gameName;
            //private Image _image;
            private string _folderName;
            private string _userInstallDirectory;
            #endregion


            #region Properties
            public SupportedGame GameType
            {
                get { return _gameType; }
            }

            public string GameName
            {
                get { return _gameName; }
            }

            public string FolderName
            {
                get { return _folderName; }
            }

            public string UserInstallDirectory
            {
                get { return _userInstallDirectory; }
                set { _userInstallDirectory = value; }
            }
            #endregion


            internal GameInfo(SupportedGame pGameType, string pFolderName)
            {
                this._gameName = GameMeta.GetDescription(pGameType);
                this._gameType = pGameType;
                this._folderName = pFolderName;
                this._userInstallDirectory = ConfigurationManager.ConfigurationFile.GetValue(ConfigurationManager.SECTION_FILEPATHS, pGameType.ToString());
            }
        }
        #endregion


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
        public const string SAVEDATA_SETTINGS_FILENAME = "savedatasettings.txt";
        #endregion


        #region Variables
        private static SupportedGame _currentGame = SupportedGame.NONE;
        private static Dictionary<SupportedGame, GameInfo> _gameDetails = new Dictionary<SupportedGame, GameInfo>();
        #endregion


        #region Properties
        /// <summary> Returns or sets the currently active game.
        /// </summary>
        public static SupportedGame CurrentGame
        {
            get { return _currentGame; }
            set { _currentGame = value; }
        }

        public static Dictionary<SupportedGame, GameInfo> GameDetails
        {
            get { return GameMeta._gameDetails; }
        }


        /// <summary> Returns or sets the current game's directory path.
        /// </summary>
        public static string CurrentGameDirectoryPath
        {
            get { return _gameDetails.ContainsKey(_currentGame) ? _gameDetails[_currentGame].UserInstallDirectory : String.Empty; }
        }


        /// <summary> Returns the current game's default folder name.
        /// </summary>
        public static string CurrentGameDefaultFolderName
        {
            get { return _gameDetails.ContainsKey(_currentGame) ? _gameDetails[_currentGame].FolderName : String.Empty; }
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


        /// <summary> Returns or sets the current game's default configuration file path.
        /// </summary>
        public static string CurrentSaveDataSettingsTextFilePath
        {
            get { return GameMeta.CurrentGameDirectoryPath + "\\" + SAVEDATA_SETTINGS_FILENAME; }
        }
        #endregion


        #region (Static) Constructor
        static GameMeta()
        {
            _gameDetails.Add(SupportedGame.EVOCHRON_MERCENARY, new GameInfo(SupportedGame.EVOCHRON_MERCENARY, "EvochronMercenary"));
            _gameDetails.Add(SupportedGame.EVOCHRON_LEGACY, new GameInfo(SupportedGame.EVOCHRON_LEGACY, "EvochronLegacy"));
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
