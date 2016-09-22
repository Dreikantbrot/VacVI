using VacVI;
using VacVI.Dialog;
using VacVI.Database;
using VacVI.Plugins;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Native
{
    public class VIStates : IPlugin
    {
        #region Variables
        private Guid _guid = new Guid();
        private Timer _autoSleepTimer = new Timer();
        #endregion


        #region Properties
        public Guid Id
        {
            get { return _guid; }
        }

        public string Name
        {
            get { return "VI States"; }
        }

        public string Version
        {
            get { return "0.1"; }
        }

        public string Author
        {
            get { return "Scavenger4711"; }
        }

        public string Homepage
        {
            get { return ""; }
        }

        public string Description
        {
            get
            {
                return "A plugin for retrieving and setting the VI's state and certain properties.";
            }
        }

        public GameMeta.SupportedGame CompatibilityFlags
        {
            get { return (~GameMeta.SupportedGame.NONE); }
        }
        #endregion


        #region Interface Functions
        public void Initialize()
        {
            int _timerInterval;

            Int32.TryParse(
                PluginManager.PluginFile.GetValue(this.Name, "Auto Sleep Timeout"),
                out _timerInterval
            );
            _autoSleepTimer.Interval = (_timerInterval * 1000);
            _autoSleepTimer.Elapsed += _autoSleepTimer_Elapsed;

            SpeechEngine.OnVISpeechRecognized += SpeechEngine_OnVISpeechRecognized;
            SpeechEngine.OnVISpeechRejected += SpeechEngine_OnVISpeechRejected;
            SpeechEngine.OnVISpeechStopped += SpeechEngine_OnVISpeechStopped;

            if (_autoSleepTimer.Interval > 0) { _autoSleepTimer.Start(); }
        }

        public void OnDialogAction(VacVI.Dialog.DialogBase originNode)
        {
            switch (originNode.Data.ToString())
            {
                case "sleep":
                    VI.State = VI.VIState.SLEEPING;
                    break;

                case "wake_up":
                    VI.State = VI.VIState.READY;
                    break;
            }
        }

        public void OnGameDataUpdate()
        {

        }

        public List<PluginParameterDefault> GetDefaultPluginParameters()
        {
            List<PluginParameterDefault> parameters = new List<PluginParameterDefault>();

            parameters.Add(new PluginParameterDefault(
                "Auto Sleep Timeout", 
                "Determines the time in seconds of silence, after which the VI will go on standby automatically.", 
                "120", 
                null
            ));

            return parameters;
        }

        public void BuildDialogTree()
        {
            DialogTreeBranch[] dialog = new DialogTreeBranch[] {
                new DialogTreeBranch(
                    new DialogPlayer(
                        "Take a nap!;Go to sleep!;Goodnight.;Go on standby!", 
                        DialogBase.DialogPriority.CRITICAL
                    ),
                    new DialogTreeBranch(  
                        new DialogVI(
                            "Goodnight;Nap time!;Wake me up if you need me.", 
                            DialogBase.DialogPriority.NORMAL,
                            () => { return (VI.State >= VI.VIState.READY); }, 
                            this.Name, "sleep", 
                            DialogBase.DialogFlags.NONE,
                            true
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        String.Format("$[Hey ]$[{0} ]$(Wake up|I need your help)!", VI.PhoneticName), 
                        DialogBase.DialogPriority.CRITICAL, 
                        null, 
                        null,
                        null, 
                        (DialogBase.DialogFlags.IGNORE_VI_STATE)
                    ),
                    new DialogTreeBranch(  
                        new DialogVI(
                            "$[But] I was $[already] awake the whole time.", 
                            DialogBase.DialogPriority.CRITICAL, 
                            () => { return (VI.State >= VI.VIState.READY); }
                        )
                    ),
                    new DialogTreeBranch(  
                        new DialogVI(
                            "Hello world!;Systems online.;Returning from standby.", 
                            DialogBase.DialogPriority.CRITICAL,
                            () => { return (VI.State < VI.VIState.READY); }, 
                            this.Name, 
                            "wake_up", 
                            (DialogBase.DialogFlags.IGNORE_VI_STATE)
                        )
                    )
                )
            };

            DialogTreeBuilder.BuildDialogTree(null, dialog);
        }

        public void OnProgramShutdown()
        {

        }
        #endregion


        #region Events

        void SpeechEngine_OnVISpeechRejected(SpeechEngine.VISpeechRejectedEventArgs obj)
        {
            if (_autoSleepTimer.Interval > 0) { _autoSleepTimer.Stop(); }
        }

        void SpeechEngine_OnVISpeechRecognized(SpeechEngine.VISpeechRecognizedEventArgs obj)
        {
            if (_autoSleepTimer.Interval > 0) { _autoSleepTimer.Stop(); }
        }

        void SpeechEngine_OnVISpeechStopped(SpeechEngine.VISpeechStoppedEventArgs obj)
        {
            if (_autoSleepTimer.Interval > 0) { _autoSleepTimer.Start(); }
        }


        void _autoSleepTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            VI.State = VI.VIState.SLEEPING;
        }
        #endregion
    }
}
