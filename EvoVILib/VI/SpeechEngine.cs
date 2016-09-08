using System;
using System.Collections.Generic;
using System.Threading;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.IO;
using IrrKlang;
using EvoVI.Classes.Dialog;
using EvoVI.Classes.Sound;

namespace EvoVI.Engine
{
    public static class SpeechEngine
    {
        #region Structs
        private struct SpeechStopCause
        {
            #region Variables
            private OnSoundStopHandler.SoundType _soundType;
            private OnSoundStopHandler.StopStates _soundState;
            private DialogVI _originDialogNode;
            private bool _playedAsync;
            #endregion


            #region Properties
            public OnSoundStopHandler.SoundType SoundType
            {
                get { return _soundType; }
            }

            public OnSoundStopHandler.StopStates SoundState
            {
                get { return _soundState; }
            }

            public DialogVI OriginDialogNode
            {
                get { return _originDialogNode; }
            }

            public bool PlayedAsync
            {
                get { return _playedAsync; }
                set { _playedAsync = value; }
            }
            #endregion


            #region Constructor
            /// <summary> Creates a struct containing info about the stop reason of a sentence spoken by the VI.
            /// </summary>
            /// <param name="pSoundType">The played sound's sound type.</param>
            /// <param name="pSoundState">The played sound's stop state (the reason why the track stopped).</param>
            /// <param name="pOriginDialogNode">The dialog node that has been sopken by the system.</param>
            public SpeechStopCause(
                OnSoundStopHandler.SoundType pSoundType,
                OnSoundStopHandler.StopStates pSoundState,
                DialogVI pOriginDialogNode,
                bool pPlayedAsync
            )
            {
                this._soundType = pSoundType;
                this._soundState = pSoundState;
                this._originDialogNode = pOriginDialogNode;
                this._playedAsync = pPlayedAsync;
            }
            #endregion
        }
        #endregion


        #region Classes
        private class OnSpeechStopHandler : ISoundStopEventReceiver
        {
            #region Functions
            /// <summary> Plays a new soundtrack if the previous reached the end of it's playtime.
            /// </summary>
            /// <param name="sound">The sound object, causing the stop event handler.</param>
            /// <param name="reason">The sound object's reason (from IrrKlang), why the track stopped.</param>
            /// <param name="stopCause">The event handler's (custom) reason, why the track stopped.</param>
            public void OnSoundStopped(ISound sound, StopEventCause reason, object stopCause)
            {
                SpeechStopCause speechStopCause = (SpeechStopCause)stopCause;
                switch (speechStopCause.SoundType)
                {
                    case OnSoundStopHandler.SoundType.SPEECH:
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
                        break;
                    
                    case OnSoundStopHandler.SoundType.RECORDING:
                            //byte[] audioData = ((KeyValuePair<bool, byte[]>)stopCause).Value;
                            //Recorder.StopRecordingAudio();

                        using (FileStream fs = File.Create(@"C:\Users\KevinLap\Desktop"))
                        {
                            //byte[] audioData = _soundSource.SampleData;
                            ISoundSource source = _soundEngine.GetSoundSource("SpokenSentence.wav");
                            WriteHeader(
                                fs,
                                source.SampleData.Length,
                                source.AudioFormat.ChannelCount,
                                source.AudioFormat.SampleRate
                            );
                            fs.Write(source.SampleData, 0, source.SampleData.Length);
                            fs.Close();
                        }
                    
                            //Recorder.ClearRecordedAudioDataBuffer();
                            //Recorder.Dispose();
                        break;
                }

                // Signalize speech end, if run asynchronously
                if (!speechStopCause.PlayedAsync) { SpeechDone = true; }

                // Start a new thread to prevent another VI or Command dialog to remove this 
                // object while it's still in use
                new Thread(n => { speechStopCause.OriginDialogNode.NextNode(); }).Start();
            }

            #region Test
            static byte[] RIFF_HEADER = new byte[] { 0x52, 0x49, 0x46, 0x46 };
            static byte[] FORMAT_WAVE = new byte[] { 0x57, 0x41, 0x56, 0x45 };
            static byte[] FORMAT_TAG = new byte[] { 0x66, 0x6d, 0x74, 0x20 };
            static byte[] AUDIO_FORMAT = new byte[] { 0x01, 0x00 };
            static byte[] SUBCHUNK_ID = new byte[] { 0x64, 0x61, 0x74, 0x61 };
            private const int BYTES_PER_SAMPLE = 2;

            public static void WriteHeader(
                 System.IO.Stream targetStream,
                 int byteStreamSize,
                 int channelCount,
                 int sampleRate)
            {

                int byteRate = sampleRate * channelCount * BYTES_PER_SAMPLE;
                int blockAlign = channelCount * BYTES_PER_SAMPLE;

                targetStream.Write(RIFF_HEADER, 0, RIFF_HEADER.Length);
                targetStream.Write(PackageInt(byteStreamSize + 42, 4), 0, 4);

                targetStream.Write(FORMAT_WAVE, 0, FORMAT_WAVE.Length);
                targetStream.Write(FORMAT_TAG, 0, FORMAT_TAG.Length);
                targetStream.Write(PackageInt(16, 4), 0, 4);//Subchunk1Size    

                targetStream.Write(AUDIO_FORMAT, 0, AUDIO_FORMAT.Length);//AudioFormat   
                targetStream.Write(PackageInt(channelCount, 2), 0, 2);
                targetStream.Write(PackageInt(sampleRate, 4), 0, 4);
                targetStream.Write(PackageInt(byteRate, 4), 0, 4);
                targetStream.Write(PackageInt(blockAlign, 2), 0, 2);
                targetStream.Write(PackageInt(BYTES_PER_SAMPLE * 8), 0, 2);
                //targetStream.Write(PackageInt(0,2), 0, 2);//Extra param size
                targetStream.Write(SUBCHUNK_ID, 0, SUBCHUNK_ID.Length);
                targetStream.Write(PackageInt(byteStreamSize, 4), 0, 4);
            }

            static byte[] PackageInt(int source, int length = 2)
            {
                if ((length != 2) && (length != 4))
                    throw new ArgumentException("length must be either 2 or 4", "length");
                var retVal = new byte[length];
                retVal[0] = (byte)(source & 0xFF);
                retVal[1] = (byte)((source >> 8) & 0xFF);
                if (length == 4)
                {
                    retVal[2] = (byte)((source >> 0x10) & 0xFF);
                    retVal[3] = (byte)((source >> 0x18) & 0xFF);
                }
                return retVal;
            }
            #endregion

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
        private static string _language = "en-US";
        private static System.Globalization.CultureInfo _culture;

        private static ISound _speechSound;
        private static ISoundSource _soundSource;
        private static ISoundEngine _soundEngine = new ISoundEngine();
        private static OnSpeechStopHandler _onSpeechStop = new OnSpeechStopHandler();

        private static SpeechSynthesizer _synthesizer = new SpeechSynthesizer();
        private static SpeechRecognitionEngine _recognizer;

        private static VoiceInfo _defaultVoice = _synthesizer.Voice;
        private static List<DialogVI> _queue = new List<DialogVI>();

        private static bool _speechDone = false;
        private static int _maxSpeechNodeAge = 2000;
        private static float _confidenceThreshold = 0.60f;
        private static VoiceModulationModes _voiceModulation = VoiceModulationModes.ROBOTIC;
        #endregion


        #region Properties
        /// <summary> Returns or sets the currently set language
        /// </summary>
        public static string Language
        {
            get { return SpeechEngine._language; }
            set { SpeechEngine._language = value; }
        }


        /// <summary> Returns or sets the culture info object it.
        /// </summary>
        public static System.Globalization.CultureInfo Culture
        {
            get { return SpeechEngine._culture; }
            set { SpeechEngine._culture = value; }
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
        public static bool SpeechDone
        {
            get { return SpeechEngine._speechDone; }
            set { SpeechEngine._speechDone = value; }
        }
        #endregion


        #region Event Handlers
        /// <summary> Fires, when a voice command has been recognized.
        /// <para>This is called after the plugin functions and player dialog node recognition.</para>
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The speech recognition engine's event arguments.</param>
        private static void onSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= _confidenceThreshold)
            {
                VI.LastRecognizedPhrase = e.Result.Text;
                VI.LastMisunderstoodDialogNode = null;
                VI.LastRecognizedGrammar = null;

                // Update dialog states to disable Command Repeater plugin
                if (PluginManager.GetPlugin("Command Repeater") != null) { DialogTreeBuilder.UpdateReadyNodes(); }

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
                    VI.LastRecognizedGrammar = e.Result.Grammar;
                    VI.LastMisunderstoodDialogNode = DialogTreeBuilder.GetPlayerDialog(e.Result.Grammar);

                    // Update dialog states to enable Command Repeater plugin
                    if (PluginManager.GetPlugin("Command Repeater") != null)
                    {
                        Say(String.Format(EvoVI.Properties.StringTable.DID_NOT_UNDERSTAND_DID_YOU_MEAN, e.Result.Alternates[i].Text));
                        DialogTreeBuilder.UpdateReadyNodes();
                    }
                    break;
                }
            }
        }
        #endregion


        #region Functions
        /// <summary> Initializes the Speech Engine.
        /// </summary>
        public static void Initialize()
        {
            _culture = new System.Globalization.CultureInfo(_language, false);
            try 
            {
                _recognizer = new SpeechRecognitionEngine(_culture);
            }
            catch (ArgumentException)
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


        /// <summary> Registers a dialog node's answers to the speech recognition engine.
        /// </summary>
        /// <param name="node">The dialog node, which answers to add.</param>
        public static void RegisterPlayerDialogNode(DialogPlayer node)
        {
            if (_recognizer == null) { return; }

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
        public static void Say(string text = "", bool async = true, VoiceModulationModes modulation = VoiceModulationModes.DEFAULT)
        {
            Say(new DialogVI(text), async, modulation);
        }


        /// <summary> Lets the VI say the specified dialog line.
        /// </summary>
        /// <param name="text">The dialog line instance to speak.</param>
        /// <param name="modulation">The voice modulation mode.</param>
        /// <param name="async">If true, speech will be run asynchronously, else the function will halt the script until the speech has finished.</param>
        public static void Say(DialogVI dialogNode, bool async = true, VoiceModulationModes modulation = VoiceModulationModes.DEFAULT, string outputFilepath = null)
        {
            if (
                (dialogNode.Speaker != DialogBase.DialogSpeaker.VI) ||
                (String.IsNullOrWhiteSpace(dialogNode.Text)) ||
                (
                    (VI.State <= VI.VIState.SLEEPING) &&
                    (dialogNode.Priority < DialogBase.DialogPriority.CRITICAL)
                )
            )
            { return; }

            if (!_queue.Contains(dialogNode))
            {
                _queue.Add(dialogNode);
                dialogNode.SpeechRegisteredInQueue = DateTime.Now; 
            }

            if (_queue[0] != dialogNode) { return; }

            _speechDone = false;

            MemoryStream streamAudio = new MemoryStream();
            _synthesizer.SetOutputToWaveStream(streamAudio);
            
            // TODO: Set emphasis and break lengths
            PromptBuilder prmptBuilder = new PromptBuilder();
            prmptBuilder.StartVoice(_defaultVoice);
            prmptBuilder.StartSentence();
            prmptBuilder.AppendText(dialogNode.Text);
            prmptBuilder.EndSentence();
            prmptBuilder.EndVoice();

            _synthesizer.Speak(prmptBuilder);
            
            // Create the modulated sound file by speaking "into RAM"
            streamAudio.Position = 0;
            _soundEngine.RemoveAllSoundSources();
            _soundSource = _soundEngine.AddSoundSourceFromIOStream(streamAudio, "SpokenSentence.wav");
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
                new SpeechStopCause(
                    (outputFilepath != null) ? OnSoundStopHandler.SoundType.SPEECH : OnSoundStopHandler.SoundType.RECORDING,
                    OnSoundStopHandler.StopStates.TRACK_FINISHED,
                    dialogNode, 
                    async
                )
            );


            while (!_speechDone && !async) { }
            _speechDone = true;
        }
        #endregion
    }
}
