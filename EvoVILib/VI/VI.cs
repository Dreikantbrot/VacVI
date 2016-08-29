using EvoVI.Classes.Dialog;
using EvoVI.PluginContracts;
using System.Reflection;
using System.Speech.Recognition;

namespace EvoVI.Engine
{
    public static class VI
    {
        #region Enums
        public enum VIState
        {
            OFFLINE = 1,
            SLEEPING = 2,
            READY = 3,
            BUSY = 4
        };
        #endregion


        #region Variables
        private static string _name = "Vāk";
        private static string _phoneticName = "Vahk";
        private static VIState _state = VIState.READY;
        private static uint _affiliationToPlayer = 50;

        private static DialogBase _currentDialogNode;
        private static DialogBase _previousDialogNode;

        private static IPlugin _lastCommand;
        private static string _lastRecognizedPhrase;
        private static Grammar _lastRecognizedGrammar;
        private static DialogPlayer _lastMisunderstoodDialogNode;
        #endregion


        #region Properties

        public static string Name
        {
            get { return VI._name; }
            set { VI._name = value; }
        }

        public static string PhoneticName
        {
            get { return VI._phoneticName; }
            set { VI._phoneticName = value; }
        }

        public static VIState State
        {
            get { return VI._state; }
            set { VI._state = value; }
        }

        public static uint AffiliationToPlayer
        {
            get { return VI._affiliationToPlayer; }
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

        public static IPlugin LastCommand
        {
            get { return VI._lastCommand; }
            set { VI._lastCommand = value; }
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
        #endregion


        #region Functions
        /// <summary> Initializes the VI.
        /// </summary>
        public static void Initialize()
        {
            _currentDialogNode = DialogTreeBuilder.RootDialogNode;
        }
        #endregion
    }
}
