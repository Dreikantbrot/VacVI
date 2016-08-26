using EvoVI.Classes.Dialog;
using EvoVI.PluginContracts;
using System.Reflection;
using System.Speech.Recognition;

namespace EvoVI.Engine
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
        private static string _lastRecognizedPhrase;
        private static Grammar _lastRecognizedGrammar;
        private static DialogPlayer _lastMisunderstoodDialogNode;
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

        public static string LastRecognizedPhrase
        {
            get { return VI._lastRecognizedPhrase; }
            set { VI._lastRecognizedPhrase = value; }
        }

        public static Grammar LastRecognizedGrammar
        {
            get { return VI._lastRecognizedGrammar; }
            set { VI._lastRecognizedGrammar = value; }
        }

        public static DialogPlayer LastMisunderstoodDialogNode
        {
            get { return VI._lastMisunderstoodDialogNode; }
            set { VI._lastMisunderstoodDialogNode = value; }
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
        #endregion
    }
}
