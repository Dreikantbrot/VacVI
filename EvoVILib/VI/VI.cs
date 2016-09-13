using EvoVI.Classes.Dialog;
using EvoVI.PluginContracts;
using System.Reflection;
using System.Speech.Recognition;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("EvoVI")]

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

        private static string _playerName = "Pilot";
        private static string _playerPhoneticName = "Pilot";

        private static DialogBase _currentDialogNode;
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

        public static string PlayerName
        {
            get { return VI._playerName; }
            set { VI._playerName = value; }
        }

        public static string PlayerPhoneticName
        {
            get { return VI._playerPhoneticName; }
            set { VI._playerPhoneticName = value; }
        }

        public static DialogBase CurrentDialogNode
        {
            get { return VI._currentDialogNode; }
            internal set { VI._currentDialogNode = value; }
        }
        #endregion


        #region Functions
        /// <summary> Initializes the VI.
        /// </summary>
        internal static void Initialize()
        {
            _currentDialogNode = DialogTreeBuilder.DialogRoot;
        }
        #endregion
    }
}
