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
        private static DialogNode _currentDialogNode;
        private static DialogNode _previousDialogNode;
        private static uint _affiliationToPlayer = 50;
        private static VIState _state = VIState.READY;
        #endregion


        #region Public Function
        /// <summary> Initializes the VI.
        /// </summary>
        public static void Initialize()
        {

        }


        /// <summary> Checks the game data and triggers events.
        /// </summary>
        public static void CheckGameData()
        {

        }
        #endregion
    }
}
