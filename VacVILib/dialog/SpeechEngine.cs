using System;
using System.Collections.Generic;
using System.Threading;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.IO;
using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using CSCore.Streams.Effects;

namespace VacVI.Dialog
{
    /// <summary> Manages and enables speech synthesis for the VI.</summary>
    public static class SpeechEngine
    {
        #region Custom Actions
        public static event Action<VISpeechRecognizedEventArgs> OnVISpeechRecognized = null;
        public class VISpeechRecognizedEventArgs
        {
            public DialogPlayer RecognizedDialog { get; private set; }
            public Grammar RecognizedGrammar { get; private set; }
            public string RecognizedPhrase { get; private set; }

            internal VISpeechRecognizedEventArgs(DialogPlayer pRecognizedDialog, Grammar pRecognizedGrammar, string pRecognizedPhrase)
            {
                this.RecognizedDialog = pRecognizedDialog;
                this.RecognizedGrammar = pRecognizedGrammar;
                this.RecognizedPhrase = pRecognizedPhrase;
            }
        }


        public static event Action<VISpeechRejectedEventArgs> OnVISpeechRejected = null;
        public class VISpeechRejectedEventArgs
        {
            public DialogPlayer RejectedDialog { get; private set; }
            public Grammar RejectedGrammar { get; private set; }
            public string RejectedPhrase { get; private set; }
            public string BestAlternative { get; private set; }

            internal VISpeechRejectedEventArgs(DialogPlayer pRejectedDialog, Grammar pRejectedGrammar, string pRejectedPhrase, string pBestAlternative)
            {
                this.RejectedDialog = pRejectedDialog;
                this.RejectedGrammar = pRejectedGrammar;
                this.RejectedPhrase = pRejectedPhrase;
                this.BestAlternative = pBestAlternative;
            }
        }


        public static event Action<VISpeechStartedEventArgs> OnVISpeechStarted = null;
        public class VISpeechStartedEventArgs
        {
            public DialogVI SpokenDialog { get; private set; }
            public string SpokenPhrase { get; private set; }

            internal VISpeechStartedEventArgs(DialogVI pSpokenDialog, string pSpokenPhrase)
            {
                this.SpokenDialog = pSpokenDialog;
                this.SpokenPhrase = pSpokenPhrase;
            }
        }


        public static event Action<VISpeechStoppedEventArgs> OnVISpeechStopped = null;
        public class VISpeechStoppedEventArgs
        {
            public DialogVI SpokenDialog { get; private set; }

            internal VISpeechStoppedEventArgs(DialogVI pSpokenDialog)
            {
                this.SpokenDialog = pSpokenDialog;
            }
        }
        #endregion


        #region Enums
        /// <summary> Supported voice modulation modes.
        /// Determines how the VI's voice sound when speaking.
        /// </summary>
        public enum VoiceModulationModes { NORMAL, ROBOTIC, DEFAULT };
        #endregion


        #region Variables
        private static string _language = "en-US";
        private static System.Globalization.CultureInfo _culture;

        private static SpeechSynthesizer _synthesizer = new SpeechSynthesizer();
        private static SpeechRecognitionEngine _recognizer;

        private static VoiceInfo _defaultVoice = _synthesizer.Voice;
        private static List<DialogVI> _queue = new List<DialogVI>();

        private static ISoundOut _soundOutput;
        private static WaveFileReader _soundSource;
        private static MemoryStream _audioStream = new MemoryStream();

        private static uint _maxSpeechNodeAge = 10000;
        private static float _confidenceThreshold = 0.60f;
        private static VoiceModulationModes _voiceModulation = VoiceModulationModes.ROBOTIC;
        #endregion


        #region Properties
        /// <summary> Returns or sets the currently set language
        /// </summary>
        public static string Language
        {
            get { return SpeechEngine._language; }
            internal set { SpeechEngine._language = value; }
        }


        /// <summary> Returns or sets the culture info object it.
        /// </summary>
        public static System.Globalization.CultureInfo Culture
        {
            get { return SpeechEngine._culture; }
            internal set { SpeechEngine._culture = value; }
        }


        /// <summary> Returns or sets the voice modulation mode.
        /// </summary>
        public static VoiceModulationModes VoiceModulation
        {
            get { return SpeechEngine._voiceModulation; }
            set { SpeechEngine._voiceModulation = value; }
        }


        /// <summary> Returns or sets the time in milliseconds a VI dialog node will be held in the queue before deletion.
        /// </summary>
        public static uint MaxSpeechNodeAge
        {
            get { return SpeechEngine._maxSpeechNodeAge; }
            internal set { SpeechEngine._maxSpeechNodeAge = value; }
        }
        #endregion


        #region Event Handlers
        /// <summary> Fires, when a voice command has been recognized.
        /// <para>This is called before the player dialog plugin functions.</para>
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The speech recognition engine's event arguments.</param>
        private static void onSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= _confidenceThreshold)
            {
                DialogPlayer recognizedNode = DialogTreeBuilder.GetPlayerDialog(e.Result.Grammar);
                if (OnVISpeechRecognized != null)
                {
                    OnVISpeechRecognized(
                        new VISpeechRecognizedEventArgs(
                            recognizedNode,
                            e.Result.Grammar,
                            e.Result.Text
                        )
                    );
                }

                recognizedNode.SetActive();
                // Say("I recognized you say: " + e.Result.Text + " - I am to " + Math.Floor(e.Result.Confidence * 100) + "% certain of that.");
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
                if (currAlternative.Confidence >= 0.15f)
                {
                    if (OnVISpeechRejected != null)
                    {
                        OnVISpeechRejected(
                            new VISpeechRejectedEventArgs(
                                DialogTreeBuilder.GetPlayerDialog(e.Result.Grammar),
                                e.Result.Grammar,
                                e.Result.Text,
                                e.Result.Alternates[i].Text
                            )
                        );
                    }

                    break;
                }
            }
        }
        #endregion


        #region Functions
        /// <summary> Initializes the Speech Engine.
        /// </summary>
        internal static void Initialize()
        {
            if (_recognizer != null)
            {
                _recognizer.RecognizeAsyncStop();
                _recognizer.UnloadAllGrammars();
                _recognizer = null; 
            }

            _culture = new System.Globalization.CultureInfo(_language, false);
            
            if (CheckLanguageSupport(_culture))
            {
                _recognizer = new SpeechRecognitionEngine(_culture);
            }
            else
            {
                _recognizer = null;
                Say(String.Format(Properties.StringTable.CANNOT_UNDERSTAND_SELECTED_LANGAUGE, _culture.EnglishName));
                return;
            }

            /* Standard sentences */
            DialogTreeBranch standardDialogs = new DialogTreeBranch(
                new DialogPlayer("What did you eat for breakfast?")
            );

            DialogTreeBuilder.BuildDialogTree(null, standardDialogs);

            _recognizer.SpeechRecognized += onSpeechRecognized;
            _recognizer.SpeechRecognitionRejected += onSpeechRejected;
            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }


        /// <summary> Checks, whether the system supports the given language.
        /// </summary>
        /// <param name="culture">The culture instance to check.</param>
        /// <returns>Whether the system supports the given language.</returns>
        internal static bool CheckLanguageSupport(System.Globalization.CultureInfo culture)
        {
            // Get first available, supported language
            foreach (RecognizerInfo recognizerInfo in SpeechRecognitionEngine.InstalledRecognizers())
            {
                if (recognizerInfo.Culture.Equals(culture)) { return true; }
            }

            return false;
        }


        /// <summary> Registers a dialog node's answers to the speech recognition engine.
        /// </summary>
        /// <param name="node">The dialog node, which answers to add.</param>
        internal static void RegisterPlayerDialogNode(DialogPlayer node)
        {
            if (_recognizer == null) { return; }

            // Load the node's answers
            for (int i = 0; i < node.GrammarList.Count; i++)
            {
                if (!_recognizer.Grammars.Contains(node.GrammarList[i])) { _recognizer.LoadGrammar(node.GrammarList[i]); }
            }
        }


        /// <summary> De-registers a dialog node's answers from the speech recognition engine.
        /// </summary>
        /// <param name="node">The dialog node, which answers to remove.</param>
        internal static void DeregisterPlayerDialogNode(DialogPlayer node)
        {
            if (_recognizer == null) { return; }

            // Load the node's answers
            for (int i = 0; i < node.GrammarList.Count; i++)
            {
                if (!_recognizer.Grammars.Contains(node.GrammarList[i])) { _recognizer.UnloadGrammar(node.GrammarList[i]); }
            }
        }


        /// <summary> Lets the VI say the specified text.
        /// </summary>
        /// <param name="text">The text to speak.</param>
        /// <param name="modulation">The voice modulation mode.</param>
        /// <param name="async">If true, speech will be run asynchronously.</param>
        public static void Say(string text = "", bool async = true, VoiceModulationModes modulation = VoiceModulationModes.DEFAULT)
        {
            Say(new DialogVI(text), async, modulation, null, false);
        }


        /// <summary> Lets the VI say the specified dialog line.
        /// </summary>
        /// <param name="dialogNode">The dialog node instance to speak.</param>
        /// <param name="modulation">The voice modulation mode.</param>
        /// <param name="async">If true, speech will be run asynchronously, else the function will halt the script until the speech has finished.</param>
        /// <param name="outputFilepath">If set, the audio will be saved as a file to the given location. The audio itself will not be played.</param>
        public static void Say(DialogVI dialogNode, bool async = true, VoiceModulationModes modulation = VoiceModulationModes.DEFAULT, string outputFilepath = null)
        {
            Say(dialogNode, async, modulation, outputFilepath, false);
        }


        /// <summary> Lets the VI say the specified dialog line.
        /// <remarks>This function offers a <c>forceSpeech</c>-parameter that is publicly not available.</remarks>
        /// </summary>
        /// <param name="dialogNode">The dialog node instance to speak.</param>
        /// <param name="modulation">The voice modulation mode.</param>
        /// <param name="async">If true, speech will be run asynchronously, else the function will halt the script until the speech has finished.</param>
        /// <param name="outputFilepath">If set, the audio will be saved as a file to the given location. The audio itself will not be played.</param>
        internal static void Say(DialogVI dialogNode, bool async, VoiceModulationModes modulation, string outputFilepath, bool forceSpeech)
        {
            // Absolute KO criteria - don't even try!
            if (
                (dialogNode.Speaker != DialogBase.DialogSpeaker.VI) ||
                (String.IsNullOrWhiteSpace(dialogNode.Text))
            )
            { return; }

            // Queue the dialog, if not already queued
            if (!_queue.Contains(dialogNode))
            {
                _queue.Add(dialogNode);
                dialogNode.SpeechRegisteredInQueue = DateTime.Now; 
            }

            // Check whether it's time to speak the dialog
            if (
                (_queue[0] != dialogNode) ||
            
                (
                    (!forceSpeech) &&
                    (VI.State <= VI.VIState.TALKING) &&
                    (_synthesizer.State == SynthesizerState.Ready) &&
                    (dialogNode.Priority < DialogBase.DialogPriority.CRITICAL)
                )
            )
            { return; }

            VI.State = VI.VIState.TALKING;
            string textToSpeak = dialogNode.Text;

            if (OnVISpeechStarted != null) { OnVISpeechStarted(new VISpeechStartedEventArgs(dialogNode, textToSpeak)); }
            
            // TODO: Set emphasis and break lengths
            PromptBuilder prmptBuilder = new PromptBuilder();
            prmptBuilder.StartVoice(_defaultVoice);
            prmptBuilder.StartSentence();
            prmptBuilder.AppendText(textToSpeak);
            prmptBuilder.EndSentence();
            prmptBuilder.EndVoice();


            // Create the modulated sound file by speaking "into RAM"
            _audioStream = new MemoryStream();
            _synthesizer.SetOutputToWaveStream(_audioStream);
            _synthesizer.Speak(prmptBuilder);
            _audioStream.Seek(0, SeekOrigin.Begin);

            // Play the audio stream
            _soundSource = new WaveFileReader(_audioStream);
            _soundOutput = new WaveOut();
            _soundOutput.Initialize(_soundSource);
            if (modulation == VoiceModulationModes.DEFAULT) { modulation = _voiceModulation; }
            if (modulation == VoiceModulationModes.ROBOTIC)
            {
                DmoChorusEffect echoSource = new DmoChorusEffect(_soundSource);
                echoSource.Delay = 20;
                _soundOutput.Initialize(echoSource);
            }

            if (outputFilepath == null)
            {
                _soundOutput.Stopped += soundOutput_Stopped;
                _soundOutput.Play();

                while ((!async) && (_soundOutput.PlaybackState == PlaybackState.Playing)) { }
                //if (!async) { _soundOutput.WaitForStopped(); }    // <-- This function does NOT WORK! - Check at CSCore, whether it's fixed yet
            }
            else
            {
                if (!Directory.Exists(Path.GetDirectoryName(outputFilepath))) { Directory.CreateDirectory(Path.GetDirectoryName(outputFilepath)); }
                _soundOutput.WaveSource.WriteToFile(outputFilepath);
                soundOutput_Stopped(null, null);
            }
        }


        /// <summary> Fires when the speech audio playback has been stopped.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The playback stopped event arguments.</param>
        private static void soundOutput_Stopped(object sender, PlaybackStoppedEventArgs e)
        {
            VI.State = VI.VIState.READY;
            if (_queue.Count <= 0) { 
                return; }

            // Get the dialog node that has just been played
            DialogVI currNode = _queue[0];

            if (OnVISpeechStopped != null) { OnVISpeechStopped(new VISpeechStoppedEventArgs(currNode)); }

            // Remove dialog nodes that are too old
            for (int i = _queue.Count - 1; i >= 0; i--)
            {
                if ((DateTime.Now - _queue[i].SpeechRegisteredInQueue).TotalMilliseconds > _maxSpeechNodeAge) { _queue.RemoveAt(i); }
            }

            // Remove current dialog node and start the next one
            if (_queue.Count > 0) { _queue.RemoveAt(0); }
            if (_queue.Count > 0)
            {
                Thread starter = new Thread(n => { Say(_queue[0]); });
                starter.Start();
            }

            _soundSource.Dispose();
            _audioStream.Dispose();

            // Start a new thread to prevent another VI or Command dialog to remove this 
            // object while it's still in use
            new Thread(n => { currNode.NextNode(); }).Start();
        }
        #endregion
    }
}
