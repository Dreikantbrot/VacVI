using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EvoVI.Database
{
    /// <summary> Contains information about the game itself.</summary>
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
            private string _processName;
            private string _userInstallDirectory;
            #endregion


            #region Properties
            /// <summary> Returns the game.
            /// </summary>
            public SupportedGame GameType
            {
                get { return _gameType; }
            }


            /// <summary> Returns the game's name.
            /// </summary>
            public string GameName
            {
                get { return _gameName; }
            }


            /// <summary> Returns the game's default directory name.
            /// </summary>
            public string FolderName
            {
                get { return _folderName; }
            }


            /// <summary> Returns the game's default process name.
            /// </summary>

            public string ProcessName
            {
                get { return _processName; }
            }


            /// <summary> Returns the game's current installation path.
            /// </summary>
            public string UserInstallDirectory
            {
                get { return _userInstallDirectory; }
                set { _userInstallDirectory = value; }
            }
            #endregion


            #region Constructors
            /// <summary> Creates an object containing details about a game.
            /// </summary>
            /// <param name="pGameType">The (supported) game.</param>
            /// <param name="pFolderName">The games directory name.</param>
            internal GameInfo(SupportedGame pGameType, string pFolderName)
            {
                this._gameName = GameMeta.GetDescription(pGameType);
                this._gameType = pGameType;
                this._folderName = pFolderName;
                this._processName = this._folderName;
                this._userInstallDirectory = ConfigurationManager.ConfigurationFile.GetValue(ConfigurationManager.SECTION_FILEPATHS, pGameType.ToString());
            }
            #endregion
        }
        #endregion


        #region Enums
        [Flags]
        public enum SupportedGame
        {
            [Description("None")]
            NONE = 0,
            
            [Description("Evochron Mercenary")]
            EVOCHRON_MERCENARY = 1,
            
            [Description("Evochron Legacy")]
            EVOCHRON_LEGACY = 2
        };
        #endregion


        #region Events
        internal static event EventHandler OnGameProcessStarted;

        private static void OnGameProcessStartedFnc(EventArgs e)
        {
            if (OnGameProcessStarted != null)
            {
                OnGameProcessStarted(null, EventArgs.Empty);
            }
        }
        #endregion


        #region Constants
        public const string DEFAULT_GAME_PATH = @"C:\sw3dg";
        public const string DEFAULT_SAVEDATA_PATH = @"C:\sw3dg";
        public const string DEFAULT_SAVEDATA_FILENAME = "savedata.txt";
        public const string DEFAULT_GAMECONFIG_FILENAME = "sw.cfg";
        public const string SAVEDATA_SETTINGS_FILENAME = "savedatasettings.txt";
        public const string KEYMAPPING_FILENAME = "keymap8.sw";
        #endregion


        #region Variables
        private static SupportedGame _currentGame = SupportedGame.NONE;
        private static System.Diagnostics.Process _gameProcess;
        private static Thread _gameProcessCheckThread;
        private static Dictionary<SupportedGame, GameInfo> _gameDetails = new Dictionary<SupportedGame, GameInfo>();
        #endregion


        #region Properties
        /// <summary> Returns or sets the currently active game.
        /// </summary>
        public static SupportedGame CurrentGame
        {
            get { return _currentGame; }
            internal set { _currentGame = value; }
        }
        

        /// <summary> Returns the current game's process.
        /// </summary>
        public static System.Diagnostics.Process GameProcess
        {
            get { return _gameProcess; }
        }


        /// <summary> Returns the database containing detailed information about the supported games.
        /// </summary>
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


        /// <summary> Returns the current game's default keymap configuration file path.
        /// </summary>
        public static string DefaultKeymapFilePath
        {
            get { return GameMeta.DefaultGameSettingsDirectoryPath + "\\" + KEYMAPPING_FILENAME; }
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
        /// <summary> Checks for the game's process to start.
        /// </summary>
        internal static void CheckForGameProcess()
        {
            StopGameProcessSearch();

            _gameProcessCheckThread = new Thread(waitForGameProcess);
            _gameProcessCheckThread.Start();
        }


        /// <summary> Aborts the search for the game process start.
        /// </summary>
        internal static void StopGameProcessSearch()
        {
            if (
                (_gameProcessCheckThread != null) &&
                (_gameProcessCheckThread.IsAlive)
            )
            {
                _gameProcessCheckThread.Abort();
                while (_gameProcessCheckThread.IsAlive) { Thread.Sleep(100); }
            }
        }


        /// <summary> Waits for the game's process to start.
        /// </summary>
        private static void waitForGameProcess()
        {
            do
            {
                Thread.Sleep(250);

                string processName = _gameDetails.ContainsKey(_currentGame) ? _gameDetails[_currentGame].ProcessName : null;
                _gameProcess = System.Diagnostics.Process.GetProcessesByName(processName).FirstOrDefault();
            }
            while (_gameProcess == null && Thread.CurrentThread.ThreadState != ThreadState.AbortRequested);

            OnGameProcessStartedFnc(EventArgs.Empty);
        }


        /// <summary> Gets the description tag entry from the SupportedGame enum.
        /// </summary>
        /// <param name="game">The enum flag for which to get the description.</param>
        /// <returns>The description (the game's name).</returns>
        public static string GetDescription(SupportedGame game)
        {
            Type valType = typeof(SupportedGame);
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
