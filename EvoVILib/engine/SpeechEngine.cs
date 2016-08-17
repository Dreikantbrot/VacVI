using System;
using System.Collections.Generic;
using System.Threading;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.IO;
using IrrKlang;
using EvoVI.classes.dialog;
using EvoVI.classes.sound;

// TODO: Clean up!

namespace EvoVI.engine
{
    public static class SpeechEngine
    {
        #region Private  Classes
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
        #endregion


        #region Enums
        /// <summary> Supported voice modulation modes.
        /// Determines how the VI's voice sound when speaking.
        /// </summary>
        public enum VoiceModulationModes { NORMAL, ROBOTIC, DEFAULT };
        #endregion


        #region Variables
        private static ISoundEngine _soundEngine = new ISoundEngine();
        private static ISound _speechSound;
        private static OnSpeechStopHandler _onSpeechStop = new OnSpeechStopHandler();

        private static SpeechSynthesizer _synthesizer = new SpeechSynthesizer();
        private static SpeechRecognitionEngine _recognizer = new SpeechRecognitionEngine();

        private static VoiceInfo _defaultVoice = _synthesizer.Voice;
        private static List<DialogNode> _queue = new List<DialogNode>();

        private static float _confidenceThreshold = 0.15f;
        private static VoiceModulationModes _voiceModulation = VoiceModulationModes.ROBOTIC;
        #endregion


        #region Event Handlers
        /// <summary> Fires, when a voice command has been recognized.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The speech recognition engine's event arguments.</param>
        private static void onSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= _confidenceThreshold)
            {
                Say("I recognized you say: " + e.Result.Text + " - I am to " + Math.Floor(e.Result.Confidence * 100) + "% certain of that.");
            }
        }


        /// <summary> Fires, when a voice command has not been recognized.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The speech recognition engine's event arguments.</param>
        private static void onSpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            for (int i = 0; i < e.Result.Alternates.Count; i++)
            {
                RecognizedPhrase currAlternative = e.Result.Alternates[i];
                if (currAlternative.Confidence >= _confidenceThreshold)
                {
                    Say(String.Format(EvoVI.Properties.StringTable.DID_NOT_UNDERSTAND_DID_YOU_MEAN, e.Result.Alternates[i].Text));
                    // TODO: Add Yes/No choice
                    break;
                }
            }
        }
        #endregion


        #region Public Functions
        /// <summary> Initializes the Speech Engine.
        /// </summary>
        public static void Initialize()
        {
            /* Standard sentences */
            DialogTreeStruct[] standardDialogs = new DialogTreeStruct[]{
                new DialogTreeStruct(
                    new DialogNodePlayer("{Yes|No}"),
                    new DialogTreeStruct[]{ }
                ),

                new DialogTreeStruct(
                    new DialogNodePlayer("Test"),
                    new DialogTreeStruct[]{ }
                )
            };

            DialogTreeReader.BuildDialogTree(standardDialogs);

            _recognizer.SpeechRecognized += onSpeechRecognized;
            _recognizer.SpeechRecognitionRejected += onSpeechRejected;
            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }


        /// <summary> Registers a dialog node's answers to the speech recognition engine.
        /// </summary>
        /// <param name="node">The dialog node, which answers to add.</param>
        public static void RegisterPlayerDialogNode(DialogNodePlayer node)
        {
            // Load the node's answers
            for (int i = 0; i < node.GrammarList.Count; i++)
            {
                if (!_recognizer.Grammars.Contains(node.GrammarList[i])) { _recognizer.LoadGrammar(node.GrammarList[i]); }
            }
        }


        /// <summary> Lets the VI say the specified text.
        /// </summary>
        /// <param name="text">The text to speak.</param>
        /// <param name="modulation">The voice modulation mode.</param>
        /// <param name="async">If true, speech will be run asynchronously.</param>
        public static void Say(string text="", VoiceModulationModes modulation = VoiceModulationModes.ROBOTIC, bool async = false)
        {
            Say(new DialogNodeVI(text), modulation, async);
        }


        /// <summary> Lets the VI say the specified dialog line.
        /// </summary>
        /// <param name="text">The dialog line instance to speak.</param>
        /// <param name="modulation">The voice modulation mode.</param>
        /// <param name="async">If true, speech will be run asynchronously.</param>
        public static void Say(DialogNode dialogLine, VoiceModulationModes modulation = VoiceModulationModes.DEFAULT, bool async = false)
        {
            if (
                (dialogLine.Speaker != DialogNode.DialogSpeaker.VI) ||
                (String.IsNullOrWhiteSpace(dialogLine.Text))
            )
            { return; }

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

            // Modulate the voice
            if (modulation == VoiceModulationModes.DEFAULT) { modulation = _voiceModulation; }
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
}
