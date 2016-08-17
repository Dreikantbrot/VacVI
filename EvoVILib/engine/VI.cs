using EvoVI.classes.dialog;
using EvoVI.PluginContracts;
using System.Reflection;

namespace EvoVI.engine
{
    public static class VI
    {
        #region Enums
        private enum VIState { READY, SLEEPING, BUSY, OFFLINE };
        #endregion


        #region Variables
        private static IPlugin _lastCommand;
        private static DialogBase _currentDialogNode;
        private static DialogBase _previousDialogNode;
        private static uint _affiliationToPlayer = 50;
        private static VIState _state = VIState.READY;
        #endregion


        #region Properties

        public static IPlugin LastCommand
        {
            get { return VI._lastCommand; }
            set { VI._lastCommand = value; }
        }

        public static DialogBase CurrentDialogNode
        {
            get { return VI._currentDialogNode; }
            set { VI._currentDialogNode = value; }
        }

        public static DialogBase PreviousDialogNode
        {
            get { return VI._previousDialogNode; }
            set { VI._previousDialogNode = value; }
        }

        public static uint AffiliationToPlayer
        {
            get { return VI._affiliationToPlayer; }
        }

        private static VIState State
        {
            get { return VI._state; }
        }
        #endregion


        #region Functions
        /// <summary> Initializes the VI.
        /// </summary>
        public static void Initialize()
        {
            _currentDialogNode = DialogTreeReader.RootDialogNode;
        }


        /// <summary> Checks the game data and triggers events.
        /// </summary>
        public static void CheckGameData()
        {
            // Call OnGameDataUpdate on all plugins
            for (int i = 0; i < PluginLoader.Plugins.Count; i++) { PluginLoader.Plugins[i].OnGameDataUpdate(); }
        }
        #endregion
    }
}
