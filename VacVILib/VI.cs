using VacVI.Dialog;
using System;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VacVI")]

namespace VacVI
{
    /// <summary> A class containing information about VI (boardcomputer) itself.</summary>
    public static class VI
    {
        #region Custom Actions
        public static event Action<OnVIStateChangedEventArgs> OnVIStateChanged = null;
        public class OnVIStateChangedEventArgs
        {
            public VIState PreviousState { get; private set; }
            public VIState CurrentState { get; private set; }

            internal OnVIStateChangedEventArgs(VIState pPreviousState, VIState pCurrentState)
            {
                this.PreviousState = pPreviousState;
                this.CurrentState = pCurrentState;
            }
        }
        #endregion


        #region Enums
        public enum VIState
        {
            OFFLINE = 1,
            SLEEPING = 2,
            BUSY = 3,
            TALKING = 4,
            READY = 5
        };
        #endregion


        #region Variables
        private static string _name = "Vāc";
        private static string _phoneticName = "Vahq";
        private static VIState _state = VIState.OFFLINE;
        private static VIState _previousState = VIState.OFFLINE;
        private static uint _affiliationToPlayer = 50;

        private static string _playerName = "Pilot";
        private static string _playerPhoneticName = "Pilot";

        private static DialogBase _currentDialogNode;
        private static bool _disabled;
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
            get { return _disabled ? VIState.OFFLINE : VI._state; }
            set
            {
                _previousState = VI.State;
                VI._state = value;

                // This check with State prevents the event from triggering, when the VI
                // is disabled, but it's state is being changed, because to an outsider
                // VI.State will always return OFFLINE. That would make it appear as if
                // the state has changed from OFFLINE to OFFLINE, which is no change at all.
                if (
                    (_previousState != State) &&
                    (OnVIStateChanged != null)
                )
                { OnVIStateChanged(new OnVIStateChangedEventArgs(State, value)); }
            }
        }


        /// <summary> Returns the VI's previous state.
        /// </summary>
        public static VIState PreviousState
        {
            get { return VI._previousState; }
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


        /// <summary> Return whether the VI is disabled.
        /// </summary>
        public static bool Disabled
        {
            get { return VI._disabled; }
            internal set
            {
                _previousState = VI.State;
                VI._disabled = value;

                // Disabling/Enabling the VI essentially makes it apppear to be OFFLINE (see "State"-getter),
                // without changing the value itself, however.
                // So if the "disabled" state changes, we need to fire the OnVIStateChanged event manually.
                if (
                    (
                        ((!VI._disabled) && (_previousState == VIState.OFFLINE)) ||
                        ((VI._disabled) && (_previousState != VIState.OFFLINE))
                    ) &&
                    (OnVIStateChanged != null)
                )
                { 
                    OnVIStateChanged(
                        new OnVIStateChangedEventArgs(
                            VI._disabled ? _previousState : VIState.OFFLINE,
                            VI._disabled ? VIState.OFFLINE : State
                        )
                    );
                }
            }
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
