using EvoVI.Dialog;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("EvoVI")]

namespace EvoVI
{
    /// <summary> A class containing information about VI (boardcomputer) itself.</summary>
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
        /// <summary> The VI's name.
        /// </summary>
        public static string Name
        {
            get { return VI._name; }
            set { VI._name = value; }
        }


        /// <summary> The text describing the VI name's ponunciation.
        /// </summary>
        public static string PhoneticName
        {
            get { return VI._phoneticName; }
            set { VI._phoneticName = value; }
        }


        /// <summary> The VI's current state.
        /// </summary>
        public static VIState State
        {
            get { return VI._state; }
            set { VI._state = value; }
        }


        /// <summary> The VI's affiliation to the player.
        /// </summary>
        public static uint AffiliationToPlayer
        {
            get { return VI._affiliationToPlayer; }
        }


        /// <summary> The player's name.
        /// </summary>
        public static string PlayerName
        {
            get { return VI._playerName; }
            set { VI._playerName = value; }
        }


        /// <summary> The text describing the player name's ponunciation.
        /// </summary>
        public static string PlayerPhoneticName
        {
            get { return VI._playerPhoneticName; }
            set { VI._playerPhoneticName = value; }
        }


        /// <summary> The currently active dialog node.
        /// </summary>
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
