using VacVI;
using VacVI.Dialog;
using VacVI.Database;
using VacVI.Plugins;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Native
{
    public class NapTime : IPlugin
    {
        #region Variables
        private Timer _autoSleepTimer = new Timer();
        #endregion


        #region Properties
        public Guid Id
        {
            get { return Guid.Parse("0cc0f49b-f4b1-4786-9c9f-b65cc2f80194"); }
        }

        public string Name
        {
            get { return "Nap time"; }
        }

        public string Version
        {
            get { return "1.0"; }
        }

        public string Author
        {
            get { return "Scavenger4711"; }
        }

        public string Homepage
        {
            get { return String.Empty; }
        }

        public string Description
        {
            get
            {
                return "A plugin for setting the VI's sleep state.\n" + 
                    "This allows the user to set the VI into a sleep-state and wake it up.";
            }
        }

        public GameMeta.SupportedGame CompatibilityFlags
        {
            get { return (~GameMeta.SupportedGame.NONE); }
        }

        public System.Drawing.Bitmap LogoImage
        {
            get { return Properties.Resources.NapTime; }
        }
        #endregion


        #region Interface Functions
        public List<PluginParameterDefault> GetDefaultPluginParameters()
        {
            List<PluginParameterDefault> parameters = new List<PluginParameterDefault>();

            parameters.Add(new PluginParameterDefault(
                "Auto Sleep Timeout",
                "Determines the time of silence (in seconds), after which the VI will go on standby automatically.",
                "120",
                null
            ));

            return parameters;
        }

        public void Initialize()
        {
            int _timerInterval;

            Int32.TryParse(
                PluginManager.PluginFile.GetValue(this.Id.ToString(), "Auto Sleep Timeout"),
                out _timerInterval
            );
            _autoSleepTimer.Interval = (_timerInterval * 1000);
            _autoSleepTimer.Elapsed += _autoSleepTimer_Elapsed;

            SpeechEngine.OnVISpeechRecognized += SpeechEngine_OnVISpeechRecognized;
            SpeechEngine.OnVISpeechRejected += SpeechEngine_OnVISpeechRejected;
            SpeechEngine.OnVISpeechStopped += SpeechEngine_OnVISpeechStopped;

            if (_autoSleepTimer.Interval > 0) { _autoSleepTimer.Start(); }
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
                            this.Id.ToString(), "sleep", 
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
                            this.Id.ToString(), 
                            "wake_up", 
                            (DialogBase.DialogFlags.IGNORE_VI_STATE)
                        )
                    )
                )
            };

            DialogTreeBuilder.BuildDialogTree(null, dialog);
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

        public void OnProgramShutdown()
        {
            _autoSleepTimer.Stop();
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
