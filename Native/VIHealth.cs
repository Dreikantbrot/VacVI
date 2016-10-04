using System;
using System.Linq;
using System.Collections.Generic;
using VacVI.Dialog;
using VacVI.Database;
using VacVI.Plugins;
using CSCore.Streams.Effects;
using System.Text;

namespace Native
{
    public class VIHealth : IPlugin
    {
        #region Constants
        const float HULL_DAMAGE_WEIGHT = 0.40f;
        const float NAV_DAMAGE_WEIGHT = 0.35f;
        const float ENGINE_DAMAGE_WEIGHT = 0.10f;
        const float WEAPON_DAMAGE_WEIGHT = 0.15f;

        static readonly char[] SCRAMBLE_IGNORE_CHARS = { '\n', '\t', '\r' };

        const string DIALOG_SYSTEM_STATUS = "Systems $[are ]at {0}%!";
        #endregion


        #region Enums
        enum ScrambleMethods { NONE, DASH, CUSTOM_CHARS, CHANGE_CASE, CUSTOM_CHARS_AND_CHANGE_CASE };
        #endregion


        #region Variables
        private ScrambleMethods _scrambleMethod = ScrambleMethods.CHANGE_CASE;
        private int _damageLevel = 0;
        StringBuilder _stringBuilder = new StringBuilder();
        Random _randNrGen = new Random();

        DialogVI _dialg_SystemStatus = new DialogVI(DIALOG_SYSTEM_STATUS);
        #endregion


        #region Parameters
        const string PARAM_NAME_LOW_DMG_THRESHOLD = "Low Damage Threshold";
        const string PARAM_NAME_MED_DMG_THRESHOLD = "Medium Damage Threshold";
        const string PARAM_NAME_HIGH_DMG_THRESHOLD = "High Damage Threshold";
        const string PARAM_NAME_CRIT_DMG_THRESHOLD = "Critical Damage Threshold";
        const string PARAM_NAME_SCRAMBLE_METHOD = "Text Scramble Method";
        const string PARAM_NAME_SCRAMBLE_CHARACTERS = "Text Scramble Characters";

        private string[] _damageThreshold_params = new string[100];
        private int _lowDmgThreshold = 0;
        private int _medDmgThreshold = 0;
        private int _highDmgThreshold = 0;
        private int _critDmgThreshold = 0;

        private string[] _scrambleMethod_params;
        private char[] _scrambleChars;
        #endregion


        #region Properties
        public Guid Id
        {
            get { return Guid.Parse("32d8ad38-25b1-435f-bbf9-75a2b770cf17"); }
        }

        public string Name
        {
            get { return "VI Health"; }
        }

        public string Description
        {
            get
            {
                return "This plugin binds the VI's health to the ship. The more damaged the ship is " +
                    "the more distorted the VI's sound will become.\n\n" +
                    "The damage is calculated as follow:\n\n" +
                    "    " + (Math.Round(HULL_DAMAGE_WEIGHT * 1000) / 10).ToString() + "% of Hull damage + \n" +
                    "    " + (Math.Round(NAV_DAMAGE_WEIGHT * 1000) / 10).ToString() + "% of Navigations damage + \n" +
                    "    " + (Math.Round(ENGINE_DAMAGE_WEIGHT * 1000) / 10).ToString() + "% of Engine damage + \n" +
                    "    " + (Math.Round(WEAPON_DAMAGE_WEIGHT * 1000) / 10).ToString() + "% of Weapon damage\n\n" + 
                    "\n" + 
                    "This plugin also provides dialog for retrieving the VI's status.";
            }
        }

        public GameMeta.SupportedGame CompatibilityFlags
        {
            get { return (~GameMeta.SupportedGame.NONE); }
        }

        public System.Drawing.Bitmap LogoImage
        {
            get { return null; }
        }

        public string Version
        {
            get { return "0.98b"; }
        }

        public string Author
        {
            get { return "Scavenger4711"; }
        }

        public string Homepage
        {
            get { return String.Empty; }
        }
        #endregion


        #region Constructor
        public VIHealth()
        {
            /* Fill damage threshold parameter list */
            for (int i = 0; i < _damageThreshold_params.Length; i++) { _damageThreshold_params[i] = i.ToString(); }

            /* Fill scramble methods parameters */
            ScrambleMethods[] scrambleMethods = (ScrambleMethods[])Enum.GetValues(typeof(ScrambleMethods));
            _scrambleMethod_params = new string[scrambleMethods.Length];

            for (int i = 0; i < scrambleMethods.Length; i++)
            {
                _scrambleMethod_params[i] = scrambleMethods[i].ToString();
            }
        }
        #endregion


        #region Interface Functions
        public List<PluginParameterDefault> GetDefaultPluginParameters()
        {
            List<PluginParameterDefault> parameters = new List<PluginParameterDefault>();

            parameters.Add(new PluginParameterDefault(
                PARAM_NAME_LOW_DMG_THRESHOLD,
                "Determines the overall damage threshold for the VI's damage state to be considered \"low\".",
                "25",
                _damageThreshold_params
            ));

            parameters.Add(new PluginParameterDefault(
                PARAM_NAME_MED_DMG_THRESHOLD,
                "Determines the overall damage threshold for the VI's damage state to be considered \"medium\".",
                "40",
                _damageThreshold_params
            ));

            parameters.Add(new PluginParameterDefault(
                PARAM_NAME_HIGH_DMG_THRESHOLD,
                "Determines the overall damage threshold for the VI's damage state to be considered \"high\".",
                "50",
                _damageThreshold_params
            ));

            parameters.Add(new PluginParameterDefault(
                PARAM_NAME_CRIT_DMG_THRESHOLD,
                "Determines the overall damage threshold for the VI's damage state to be considered \"critical\".",
                "70",
                _damageThreshold_params
            ));

            parameters.Add(new PluginParameterDefault(
                PARAM_NAME_SCRAMBLE_METHOD,
                "Determines how displayed text should be scrambled, when the VI's damage state is \"high\" or \"critical\".",
                "CUSTOM_CHARS_AND_CHANGE_CASE",
                _scrambleMethod_params
            ));

            parameters.Add(new PluginParameterDefault(
                PARAM_NAME_SCRAMBLE_CHARACTERS,
                "If \"CUSTOM_CHARS_AND_CHANGE_CASE\" or \"CUSTOM_CHARS\" is selected as an option for the scrambling method, " + 
                "the following characters will be used for scrambling the text.\n" + 
                "Simply enter any desired characters into the textbox without a separator.",
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-%$/&!\"'#+"
            ));

            return parameters;
        }

        public void Initialize()
        {
            /* Set parameter values from plugins.ini */
            if (_damageThreshold_params.Contains(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_NAME_LOW_DMG_THRESHOLD)))
            {
                Int32.TryParse(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_NAME_LOW_DMG_THRESHOLD), out _lowDmgThreshold);
            }
            
            if (_damageThreshold_params.Contains(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_NAME_MED_DMG_THRESHOLD)))
            {
                Int32.TryParse(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_NAME_MED_DMG_THRESHOLD), out _medDmgThreshold);
            }
            
            if (_damageThreshold_params.Contains(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_NAME_HIGH_DMG_THRESHOLD)))
            {
                Int32.TryParse(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_NAME_HIGH_DMG_THRESHOLD), out _highDmgThreshold);
            }
            
            if (_damageThreshold_params.Contains(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_NAME_CRIT_DMG_THRESHOLD)))
            {
                Int32.TryParse(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_NAME_CRIT_DMG_THRESHOLD), out _critDmgThreshold);
            }

            if (_scrambleMethod_params.Contains(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_NAME_SCRAMBLE_METHOD)))
            {
                Enum.TryParse(PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_NAME_SCRAMBLE_METHOD), out _scrambleMethod);
            }

            if (
                (
                    (_scrambleMethod == ScrambleMethods.CUSTOM_CHARS) ||
                    (_scrambleMethod == ScrambleMethods.CUSTOM_CHARS_AND_CHANGE_CASE)
                ) &&
                (PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_NAME_SCRAMBLE_CHARACTERS).Length > 0)
            )
            {
                _scrambleChars = PluginManager.PluginFile.GetValue(this.Id.ToString(), PARAM_NAME_SCRAMBLE_CHARACTERS).ToCharArray();
            }


            SpeechEngine.OnSoundOutputInitialized += SpeechEngine_OnSoundOutputInitialized;
            SpeechEngine.OnVISpeechStarted += SpeechEngine_OnVISpeechStarted;
        }

        public void BuildDialogTree()
        {
            DialogTreeBranch[] dialog = new DialogTreeBranch[] {
                new DialogTreeBranch(
                    new DialogPlayer(
                        "$(Is everything|Everything|Are you) $(okay|alright)?" + ";" + 
                        "How are you?"
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "I'm fine, thanks$[ for asking]!" + ";" + 
                            "All systems working at maximum capacity!",
                            DialogBase.DialogPriority.VERY_HIGH,
                            () => { return (_damageLevel < _lowDmgThreshold); }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$(I've sustained only minor damage.|Just a few scratches and CRC mismatches.) " +
                            "All systems$[ are] still functional.",
                            DialogBase.DialogPriority.HIGH,
                            () => { return (_damageLevel < _medDmgThreshold); }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "I've sustained moderate damage$[ - running system diagnostics now..].",
                            DialogBase.DialogPriority.NORMAL,
                            () => { return (_damageLevel < _highDmgThreshold); }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "I've sustained damage! System operations might be impaired.",
                            DialogBase.DialogPriority.LOW,
                            () => { return (_damageLevel < _critDmgThreshold); }
                        )
                    ),
                    new DialogTreeBranch(
                        new DialogVI(
                            "$(I've |< -- --> >)sustained critical damage!",
                            DialogBase.DialogPriority.VERY_LOW,
                            () => { return (_damageLevel >= _critDmgThreshold); }
                        )
                    )
                ),
                new DialogTreeBranch(
                    new DialogPlayer(
                        "$(What's|What is) your status?"
                    ),
                    new DialogTreeBranch(
                        new DialogCommand(
                            "Give system status specifics",
                            DialogBase.DialogPriority.NORMAL,
                            null,
                            this.Id.ToString(),
                            "give_status"
                        )
                    )
                )
            };

            DialogTreeBuilder.BuildDialogTree(null, dialog);
        }

        public void OnDialogAction(DialogBase originNode)
        {
            switch(originNode.Data.ToString())
            {
                case "give_status":
                    _dialg_SystemStatus.RawText = String.Format(DIALOG_SYSTEM_STATUS, 100 - _damageLevel);
                    SpeechEngine.Say(_dialg_SystemStatus);
                    break;
            }
        }

        public void OnGameDataUpdate()
        {
            if (
                (PlayerShipData.HullIntegrity != null) &&
                (PlayerShipData.NavHealth != null) &&
                (PlayerShipData.EngineHealth != null) &&
                (PlayerShipData.WeaponHealth != null)
            )
            {
                _damageLevel = (int)(
                    100 - (
                        (HULL_DAMAGE_WEIGHT * PlayerShipData.HullIntegrity) +
                        (NAV_DAMAGE_WEIGHT * PlayerShipData.NavHealth) +
                        (ENGINE_DAMAGE_WEIGHT * PlayerShipData.EngineHealth) +
                        (WEAPON_DAMAGE_WEIGHT * PlayerShipData.WeaponHealth)
                    )
                );
            }
        }

        public void OnProgramShutdown()
        {

        }
        #endregion


        #region Events
        void SpeechEngine_OnSoundOutputInitialized(SpeechEngine.SoundOutputInitializedEventArgs obj)
        {
            // Distort the VI's voice according to the damage
            // All single distortion effect stack
            if (_damageLevel >= _lowDmgThreshold)
            {
                // Add a flanger effect at low damage
                DmoFlangerEffect flangeSrc = new DmoFlangerEffect(obj.SoundOutput.WaveSource);
                flangeSrc.Depth = _damageLevel;
                flangeSrc.Frequency = ((0.01f * _damageLevel) * 5);
                obj.SoundOutput.Initialize(flangeSrc);

                if (_damageLevel >= _highDmgThreshold)
                {
                    // Add a square-wave gargle effect on high or critical damage
                    DmoGargleEffect addDistSource = new DmoGargleEffect(obj.SoundOutput.WaveSource);
                    addDistSource.WaveShape = GargleWaveShape.Square;
                    addDistSource.RateHz = 450;
                    obj.SoundOutput.Initialize(addDistSource);
                }
                else if (_damageLevel >= _medDmgThreshold)
                {
                    // Add a triangle-wave gargle effect on medium damage only
                    DmoGargleEffect addDistSource = new DmoGargleEffect(obj.SoundOutput.WaveSource);
                    addDistSource.WaveShape = GargleWaveShape.Triangle;
                    addDistSource.RateHz = 450;
                    obj.SoundOutput.Initialize(addDistSource);
                }

                if (_damageLevel >= _critDmgThreshold)
                {
                    // Add a stronger square-wave gargle effect on critical damage
                    DmoGargleEffect distSource = new DmoGargleEffect(obj.SoundOutput.WaveSource);
                    distSource.WaveShape = GargleWaveShape.Square;
                    distSource.RateHz = 50;
                    obj.SoundOutput.Initialize(distSource);
                }
            }
        }

        void SpeechEngine_OnVISpeechStarted(SpeechEngine.VISpeechStartedEventArgs obj)
        {
            int minThreshold = _medDmgThreshold;

            // Scramble the scrambled eggs out of the displayed sentence, if damage
            if (
                (_damageLevel >= minThreshold) &&
                (_scrambleMethod != ScrambleMethods.NONE)
            )
            {
                _stringBuilder.Clear();
                _stringBuilder.Append(obj.DisplayedPhrase);

                int ingoreCharsCount = obj.DisplayedPhrase.Count(c => SCRAMBLE_IGNORE_CHARS.Contains(c));
                double scramblePercentage = (double)(_damageLevel - minThreshold) / (100 - minThreshold);
                int charsToScramble = (int)((_stringBuilder.Length - ingoreCharsCount) * scramblePercentage);
                
                int[] indicesToScramble = new int[charsToScramble];
                for (int i = 0; i < indicesToScramble.Length; i++)
                {
                    int targetIndex;
                    
                    do 
                    {
                        targetIndex = _randNrGen.Next(_stringBuilder.Length);
                        
                        if (
                            (!indicesToScramble.Contains(targetIndex)) &&
                            (!SCRAMBLE_IGNORE_CHARS.Contains(_stringBuilder[targetIndex]))
                        )
                        { indicesToScramble[i] = targetIndex; }
                    }
                    while (indicesToScramble[i] == 0);
                }

                for (int i = 0; i < indicesToScramble.Length; i++)
                {
                    _stringBuilder[indicesToScramble[i]] = (
                        
                        // Change the case of characters
                        (_scrambleMethod == ScrambleMethods.CHANGE_CASE) ? 
                        (
                            Char.IsUpper(_stringBuilder[indicesToScramble[i]]) ? 
                            Char.ToLower(_stringBuilder[indicesToScramble[i]]) : 
                            Char.ToUpper(_stringBuilder[indicesToScramble[i]])
                        ) :
                        
                        // Replace chars with dashes
                        (_scrambleMethod == ScrambleMethods.DASH) ? '-' :

                        // Replace chars with random characters from _scrambleChars[]
                        ((_scrambleMethod == ScrambleMethods.CUSTOM_CHARS) && (_scrambleChars != null)) ? (
                            _scrambleChars[_randNrGen.Next(_scrambleChars.Length)]
                        ) :

                        // Replace chars with random characters from _scrambleChars[] + Change the case of characters
                        (
                            ((_randNrGen.Next(100) < 50) && (_scrambleChars != null)) ? 
                            _scrambleChars[_randNrGen.Next(_scrambleChars.Length)] : 
                            (
                                Char.IsUpper(_stringBuilder[indicesToScramble[i]]) ?
                                Char.ToLower(_stringBuilder[indicesToScramble[i]]) :
                                Char.ToUpper(_stringBuilder[indicesToScramble[i]])
                            )
                        )
                    );
                }

                obj.DisplayedPhrase = _stringBuilder.ToString();
            }
        }
        #endregion
    }
}
