using System.Collections.Generic;
using System.Threading;
using IrrKlang;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.IO;

namespace Evo_VI.engine
{
    public static class SpeechEngine
    {
        #region Variables
        private static ISoundEngine soundEngine = new ISoundEngine();
        private static ISound speechSound;

        private static SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        private static SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();

        private static VoiceInfo defaultVoice = synthesizer.Voice;
        private static List<int> queue = new List<int>();
        #endregion


        #region Event Handlers
        private static void recognized(object sender, SpeechRecognizedEventArgs e)
        {
            // TODO: Implement recognized commands (keypresses or audible feedback)
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

                grammarbuilder = new grammarbuilder();
                grammarbuilder.append("i");
                grammarbuilder.append(new choices("love", "hate"));
                grammarbuilder.append("evochron");
                grammarbuilder.append(new choices("legend", "mercenary", "legacy"));
                recognizer.LoadGrammar(new Grammar(grammarBuilder));
            */

            recognizer.SpeechRecognized += recognized;
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }


        /// <summary>
        /// Lets the VI say the specified text.
        /// </summary>
        /// <param name="text"> The text to be spoken.</param>
        /// <param name="async"> If set to true, the function will be run asynchronously.</param>
        public static void Say(string text = "", bool async = false)
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
            
            streamAudio.Position = 0;
            soundEngine.RemoveAllSoundSources();
            soundEngine.AddSoundSourceFromIOStream(streamAudio, "SpokenSentence.wav");

            speechSound = soundEngine.Play2D("SpokenSentence.wav", false, false, StreamMode.Streaming, true);
            speechSound.SoundEffectControl.EnableChorusSoundEffect();
            speechSound.SoundEffectControl.EnableChorusSoundEffect(50f, 10f, 40f, 1.1f, true, 0f, 90);

            soundEngine.Update();

            queue.RemoveAt(0);
            Thread.Sleep(100);
        }
        #endregion
    }
}
