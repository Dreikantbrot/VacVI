using System.Collections.Generic;
using System.Threading;
using IrrKlang;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.IO;
using System;

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
                    SpeechEngine.SpeechFinished = true;
                    break;
            }
        }
        #endregion
    }


    public static class SpeechEngine
    {
        #region Enums
        public enum VoiceModulationModes { NORMAL, ROBOTIC };
        #endregion


        #region Variables
        private static ISoundEngine soundEngine = new ISoundEngine();
        private static ISound speechSound;
        private static OnSoundStopHandler onSoundStop = new OnSoundStopHandler();

        private static SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        private static SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();

        private static VoiceInfo defaultVoice = synthesizer.Voice;
        private static List<int> queue = new List<int>();

        public static bool SpeechFinished = true;
        #endregion


        #region Event Handlers
        private static void recognized(object sender, SpeechRecognizedEventArgs e)
        {
            // TODO: Implement recognized commands (keypresses or audible feedback)

            if (e.Result.Text == "test")
            {
                Interactor.SendKey((uint)System.Windows.Forms.Keys.H);
                Interactor.SendKey((uint)System.Windows.Forms.Keys.E);
                Interactor.SendKey((uint)System.Windows.Forms.Keys.L);
                Interactor.SendKey((uint)System.Windows.Forms.Keys.L);
                Interactor.SendKey((uint)System.Windows.Forms.Keys.O);
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
            recognizer.LoadGrammar(new Grammar(grammarbuilder));

            recognizer.SpeechRecognized += recognized;
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }


        /// <summary>
        /// Lets the VI say the specified text.
        /// </summary>
        /// <param name="text"> The text to be spoken.</param>
        /// <param name="async"> If set to true, the function will be run asynchronously.</param>
        public static void Say(string text = "", VoiceModulationModes modulation = VoiceModulationModes.ROBOTIC, bool async = false)
        {
            // Wait for other jobs to finish
            int textHash = text.GetHashCode();
            queue.Add(textHash);
            while ((queue[0] != textHash) && (synthesizer.State != SynthesizerState.Ready)) { Thread.Sleep(100); }

            MemoryStream streamAudio = new MemoryStream();
            synthesizer.SetOutputToWaveStream(streamAudio);
            
            // TODO: Set emphasis and break lengths
            PromptBuilder prmptBuilder = new PromptBuilder();
            prmptBuilder.StartVoice(defaultVoice);
            prmptBuilder.StartSentence();
            prmptBuilder.AppendText(text);
            prmptBuilder.EndSentence();
            prmptBuilder.EndVoice();

            synthesizer.Speak(prmptBuilder);
            
            // Create the modulated sound file by speaking "into RAM"
            streamAudio.Position = 0;
            soundEngine.RemoveAllSoundSources();
            soundEngine.AddSoundSourceFromIOStream(streamAudio, "SpokenSentence.wav");
            speechSound = soundEngine.Play2D("SpokenSentence.wav", false, false, StreamMode.Streaming, true);

            if (modulation == VoiceModulationModes.ROBOTIC)
            {
                speechSound.SoundEffectControl.EnableChorusSoundEffect();
                speechSound.SoundEffectControl.EnableChorusSoundEffect(50f, 10f, 40f, 1.1f, true, 0f, 90);
            }

            // Prepare event handler
            speechSound.setSoundStopEventReceiver(
                onSoundStop,
                new KeyValuePair<OnSoundStopHandler.SoundType, OnSoundStopHandler.StopStates>(OnSoundStopHandler.SoundType.SPEECH, OnSoundStopHandler.StopStates.TRACK_FINISHED)
            );

            // Wait until ready before talking
            while ((!async) && (!SpeechFinished)) { Thread.Sleep(100); }
            SpeechFinished = false;
            soundEngine.Update();

            queue.RemoveAt(0);
            Thread.Sleep(100);
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
