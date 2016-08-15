using Evo_VI.classes.dialog;
using System.Reflection;

namespace Evo_VI.engine
{
    public static class VI
    {
        #region Enums
        private enum VIState { READY, SLEEPING, BUSY, OFFLINE };
        #endregion


        #region Variables
        private static ICommand _lastCommand;
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
