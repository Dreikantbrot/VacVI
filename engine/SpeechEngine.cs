using System.Collections.Generic;
using System.Threading;
using IrrKlang;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.IO;
using System;

// TODO: Clean up!

namespace Evo_VI.engine
{
    public class OnSoundStopHandler : ISoundStopEventReceiver
    {
        #region Enums
        /// <summary> The reason why a soundtrack was / had to be stopped
        /// </summary>
        public enum StopStates { TRACK_FINISHED, STOPPED_BY_USER, TRACK_SWITCH, PAUSED, INGORE };

        /// <summary> The kind of sound that has been stopped
        /// </summary>
        public enum SoundType { MUSIC, SPEECH };
        #endregion


        #region Public Functions
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


    public static class SpeechEngine
    {
        private class OnSpeechStopHandler : ISoundStopEventReceiver
        {
            #region Public Functions
            /// <summary> Plays a new soundtrack if the previous reached the end of it's playtime.
            /// </summary>
            /// <param name="sound">The sound object, causing the stop event handler.</param>
            /// <param name="reason">The sound object's reason (from IrrKlang), why the track stopped.</param>
            /// <param name="stopCause">The event handler's (custom) reason, why the track stopped.</param>
            public void OnSoundStopped(ISound sound, StopEventCause reason, object stopCause)
            {
                KeyValuePair<OnSoundStopHandler.SoundType, OnSoundStopHandler.StopStates> stopCauseVals = (KeyValuePair<OnSoundStopHandler.SoundType, OnSoundStopHandler.StopStates>)stopCause;
                switch (stopCauseVals.Key)
                {
                    case OnSoundStopHandler.SoundType.SPEECH:
                        _queue.RemoveAt(0);
                        if (_queue.Count > 0)
                        {
                            Thread starter = new Thread(n => { Say(_queue[0]); });
                            starter.Start();
                        }
                        break;
                }
            }
            #endregion
        }


        public class DialogLine
        {
            enum DialogImportance { LOW, NORMAL, HIGH, CRITICAL };

            string _text;
            DialogImportance _importance;

            private DialogImportance Importance
            {
                get { return _importance; }
                set { _importance = value; }
            }


            public string Text
            {
                get { return _text; }
                set { _text = value; }
            }

            public DialogLine(string pText, DialogImportance pImportance = DialogImportance.NORMAL)
            {
                this._text = pText;
                this._importance = pImportance;
            }
        }


        #region Enums
        public enum VoiceModulationModes { NORMAL, ROBOTIC };
        #endregion


        #region Variables
        private static ISoundEngine _soundEngine = new ISoundEngine();
        private static ISound _speechSound;
        private static OnSpeechStopHandler _onSpeechStop = new OnSpeechStopHandler();

        private static SpeechSynthesizer _synthesizer = new SpeechSynthesizer();
        private static SpeechRecognitionEngine _recognizer = new SpeechRecognitionEngine();

        private static VoiceInfo _defaultVoice = _synthesizer.Voice;
        private static List<DialogLine> _queue = new List<DialogLine>();
        #endregion


        #region Event Handlers
        private static void recognized(object sender, SpeechRecognizedEventArgs e)
        {
            // TODO: Implement recognized commands (keypresses or audible feedback)

            if (e.Result.Text == "test")
            {
                Say("Hello");
                Say("Hello");
            }
            else if (e.Result.Text == "refresh") Interactor.Initialize();
        }
        #endregion


        #region Public Functions
        /// <summary>
        /// Initializes the Speech Engine.
        /// </summary>
        public static void Initialize()
        {
            // TODO: Build grammar. Example below:
            /*
                GrammarBuilder grammarBuilder;

                grammarbuilder = new GrammarBuilder();
                grammarbuilder.Append("i");
                grammarbuilder.Append(new Choices("love", "hate"));
                grammarbuilder.Append("evochron");
                grammarbuilder.Append(new Choices("legend", "mercenary", "legacy"));
                recognizer.LoadGrammar(new Grammar(grammarbuilder));
            */

            GrammarBuilder grammarbuilder;
            grammarbuilder = new GrammarBuilder();
            grammarbuilder.Append(new Choices("test", "refresh"));
            _recognizer.LoadGrammar(new Grammar(grammarbuilder));

            _recognizer.SpeechRecognized += recognized;
            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }


        /// <summary>
        /// Lets the VI say the specified text.
        /// </summary>
        /// <param name="line"> The text to be spoken.</param>
        /// <param name="async"> If set to true, the function will be run asynchronously.</param>
        public static void Say(string text="", VoiceModulationModes modulation = VoiceModulationModes.ROBOTIC, bool async = false)
        {
            Say(new DialogLine(text), modulation, async);
        }


        /// <summary>
        /// Lets the VI say the specified text.
        /// </summary>
        /// <param name="line"> The text to be spoken.</param>
        /// <param name="async"> If set to true, the function will be run asynchronously.</param>
        public static void Say(DialogLine dialogLine, VoiceModulationModes modulation = VoiceModulationModes.ROBOTIC, bool async = false)
        {
            if (!_queue.Contains(dialogLine)) { _queue.Add(dialogLine); }
            if (_queue[0] != dialogLine) { return; }

            MemoryStream streamAudio = new MemoryStream();
            _synthesizer.SetOutputToWaveStream(streamAudio);
            
            // TODO: Set emphasis and break lengths
            PromptBuilder prmptBuilder = new PromptBuilder();
            prmptBuilder.StartVoice(_defaultVoice);
            prmptBuilder.StartSentence();
            prmptBuilder.AppendText(dialogLine.Text);
            prmptBuilder.EndSentence();
            prmptBuilder.EndVoice();

            _synthesizer.Speak(prmptBuilder);
            
            // Create the modulated sound file by speaking "into RAM"
            streamAudio.Position = 0;
            _soundEngine.RemoveAllSoundSources();
            _soundEngine.AddSoundSourceFromIOStream(streamAudio, "SpokenSentence.wav");
            _speechSound = _soundEngine.Play2D("SpokenSentence.wav", false, false, StreamMode.Streaming, true);

            if (modulation == VoiceModulationModes.ROBOTIC)
            {
                _speechSound.SoundEffectControl.EnableChorusSoundEffect();
                _speechSound.SoundEffectControl.EnableChorusSoundEffect(50f, 10f, 40f, 1.1f, true, 0f, 90);
            }

            _soundEngine.Update();

            // Prepare event handler
            _speechSound.setSoundStopEventReceiver(
                _onSpeechStop,
                new KeyValuePair<OnSoundStopHandler.SoundType, OnSoundStopHandler.StopStates>(OnSoundStopHandler.SoundType.SPEECH, OnSoundStopHandler.StopStates.TRACK_FINISHED)
            );
        }
        #endregion
    }


    public static class DialogTreeReader
    {
        public static PromptBuilder BuildDialogTree()
        {
            throw new NotImplementedException(); 
        }
    }
}
