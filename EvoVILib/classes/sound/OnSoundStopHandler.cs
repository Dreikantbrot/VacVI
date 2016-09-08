using IrrKlang;
using System.Collections.Generic;

namespace EvoVI.Classes.Sound
{
    public class OnSoundStopHandler : ISoundStopEventReceiver
    {
        #region Enums
        /// <summary> The reason why a soundtrack was / had to be stopped
        /// </summary>
        public enum StopStates { TRACK_FINISHED, STOPPED_BY_USER, TRACK_SWITCH, PAUSED, INGORE };

        /// <summary> The kind of sound that has been stopped
        /// </summary>
        public enum SoundType { MUSIC, SPEECH, RECORDING };
        #endregion


        #region Functions
        /// <summary> Plays a new soundtrack if the previous reached the end of it's playtime.
        /// </summary>
        /// <param name="sound">The sound object, causing the stop event handler.</param>
        /// <param name="reason">The sound object's reason (from IrrKlang), why the track stopped.</param>
        /// <param name="stopCause">The event handler's (custom) reason, why the track stopped.</param>
        public void OnSoundStopped(ISound sound, StopEventCause reason, object stopCause)
        {
            KeyValuePair<SoundType, StopStates> stopCauseVals = (KeyValuePair<SoundType, StopStates>)stopCause;
            switch (stopCauseVals.Key)
            {
                case SoundType.MUSIC:
                    switch (stopCauseVals.Value)
                    {
                        case StopStates.TRACK_FINISHED:
                            // TODO: Play next soundtrack
                            break;
                        case StopStates.STOPPED_BY_USER: break;
                        case StopStates.TRACK_SWITCH: break;
                        case StopStates.PAUSED: break;
                    }
                    break;

                case SoundType.SPEECH:
                    break;
            }
        }
        #endregion
    }
}
