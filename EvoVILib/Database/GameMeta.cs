using System;
using System.Collections.Generic;
using System.Linq;
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
            NONE = ~(EVOCHRON_MERCENARY | EVOCHRON_LEGACY),
            EVOCHRON_MERCENARY = 1,
            EVOCHRON_LEGACY = 2
        };
        #endregion


        #region Constants
        private const string DEFAULT_GAME_PATH = @"C:\sw3dg";
        private const string DEFAULT_SAVEDATA_PATH = @"C:\sw3dg";
        private const string DEFAULT_SAVEDATA_FILENAME = "savedata.txt";
        private const string DEFAULT_GAMECONFIG_FILENAME = "sw.cfg";
        #endregion


        #region Variables
        private static string _gamePath = "";
        private static SupportedGame _currentGame = SupportedGame.EVOCHRON_MERCENARY;
        private static Dictionary<SupportedGame, string> _folderNames = new Dictionary<SupportedGame, string>();
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
        public static string GamePath
        {
            get { return _gamePath; }
            set { _gamePath = value; }
        }


        /// <summary> Returns the current game's default folder name.
        /// </summary>
        public static string CurrentGameDefaultFolderName
        {
            get { return _folderNames[_currentGame]; }
        }


        /// <summary> Returns or sets the current game's default savedata file path.
        /// </summary>
        public static string DefaultSavedataPath
        {
            get { return DEFAULT_SAVEDATA_PATH + "\\" + CurrentGameDefaultFolderName + "\\" + DEFAULT_SAVEDATA_FILENAME; }
        }


        /// <summary> Returns or sets the current game's default configuration file path.
        /// </summary>
        public static string DefaultGameSettingsPath
        {
            get { return DEFAULT_SAVEDATA_PATH + "\\" + CurrentGameDefaultFolderName + "\\" + DEFAULT_GAMECONFIG_FILENAME; }
        }
        #endregion


        #region (Static) Constructor
        static GameMeta()
        {
            _folderNames.Add(SupportedGame.EVOCHRON_MERCENARY, "EvochronMercenary");
            _folderNames.Add(SupportedGame.EVOCHRON_LEGACY, "EvochronLegacy");
        }
        #endregion
    }
}
