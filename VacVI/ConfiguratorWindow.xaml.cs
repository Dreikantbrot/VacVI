using System;
using System.Windows;
using System.IO;
using VacVI;
using VacVI.Plugins;
using System.Windows.Controls;
using System.Windows.Media;
using VacVI.Database;
using System.Collections.Generic;
using VacVI.Dialog;
using System.Windows.Input;
using VacVI.Input;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace VacVIConfigurator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class ConfiguratorWindow : Window
    {
        #region Classes
        /// <summary> Class holding a real and displayed value for comboboxes.</summary>
        private class ValueLabelPair
        {
            #region Variables
            protected object _value;
            protected string _displayValue;
            #endregion


            #region Properties
            /// <summary> Returns or sets the underlying (real) value.
            /// </summary>
            public object Value
            {
                get { return _value; }
                set { _value = value; }
            }


            /// <summary> Returns or sets the value's label.
            /// </summary>
            public string DisplayValue
            {
                get { return _displayValue; }
                set { _displayValue = value; }
            }
            #endregion


            #region Constructor
            /// <summary> Creates a value-label pair.
            /// </summary>
            /// <param name="pValue">The underlying value.</param>
            /// <param name="pLabel">The displayed value / value label.</param>
            public ValueLabelPair(object pValue = null, string pDisplayValue = null)
            {
                _value = pValue;
                _displayValue = pDisplayValue;
            }
            #endregion
        }


        /// <summary> Class holding a real and displayed value for supported games.</summary>
        private class GameEntry : ValueLabelPair
        {
            /// <summary> Creates a game entry instance as a value-label pair.
            /// </summary>
            /// <param name="pValue">The supported game.</param>
            public GameEntry(GameMeta.SupportedGame pValue) : base(pValue, GameMeta.GetDescription(pValue)) { }
        }


        /// <summary> Class holding a real and displayed value for VI voices.</summary>
        private class VIVoice : ValueLabelPair
        {
            /// <summary> Creates a VI voice entry instance as a value-label pair.
            /// </summary>
            /// <param name="pValue">The VI modulation mode (aka the voice).</param>
            public VIVoice(SpeechEngine.VoiceModulationModes pValue) : base(pValue)
            {                
                switch(pValue)
                {
                    case SpeechEngine.VoiceModulationModes.NORMAL:
                        _displayValue = "Normal";
                        break;

                    case SpeechEngine.VoiceModulationModes.ROBOTIC:
                        _displayValue = "Robotic";
                        break;

                    case SpeechEngine.VoiceModulationModes.DEFAULT:
                    default:
                        _displayValue = "Default";
                        break;
                }
            }
        }
        #endregion


        #region Enums
        /// <summary> Enum for Okay-Warning-Error states.</summary>
        private enum CheckBoxColorState
        {
            NONE = -1,
            OKAY = 0,
            WARNING = 1,
            ERROR = 2
        };
        #endregion


        #region Variables
        private List<Key> keyDowns = new List<Key>();

        private bool _uiLocked = false;
        private bool _openChanges = false;
        private SolidColorBrush _brightThemeBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255,255));
        private SolidColorBrush _darkThemeBrush = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));

        private System.Windows.Media.Imaging.BitmapImage _imgEvochronMercenary = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"Resources/EvochronMercenary.png", UriKind.RelativeOrAbsolute));
        private System.Windows.Media.Imaging.BitmapImage _imgEvochronLegacy = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"Resources/EvochronLegacy.png", UriKind.RelativeOrAbsolute));
        private System.Windows.Media.Imaging.BitmapImage _imgLogo_VI = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"Resources/Logo_VI.png", UriKind.RelativeOrAbsolute));
        #endregion


        #region Constructor
        public ConfiguratorWindow()
        {
            InitializeComponent();

            /* Load assets */
            PluginManager.LoadPlugins(true);
            ConfigurationManager.LoadConfiguration();


            /* Fill VI voice list */
            comBox_Config_VIVoice.Items.Clear();
            SpeechEngine.VoiceModulationModes[] voiceModes = (SpeechEngine.VoiceModulationModes[])Enum.GetValues(typeof(SpeechEngine.VoiceModulationModes));
            for (int i = 0; i < voiceModes.Length; i++) { comBox_Config_VIVoice.Items.Add(new VIVoice(voiceModes[i])); }


            /* Fill games list */
            comBox_GameSelection.Items.Clear();
            GameMeta.SupportedGame[] games = (GameMeta.SupportedGame[])Enum.GetValues(typeof(GameMeta.SupportedGame));
            for (int i = 0; i < games.Length; i++)
            {
                if (games[i] == GameMeta.SupportedGame.NONE) { continue; }

                comBox_GameSelection.Items.Add(new GameEntry(games[i]));
            }


            /* Load main configuration */
            // "Game" section
            for (int i = 0; i < comBox_GameSelection.Items.Count; i++)
            {
                if (GameMeta.CurrentGame == (GameMeta.SupportedGame)((ValueLabelPair)comBox_GameSelection.Items[i]).Value)
                {
                    comBox_GameSelection.SelectedIndex = i;
                    break;
                }
            }
            if (comBox_GameSelection.SelectedItem == null) { comBox_GameSelection.SelectedIndex = 0; }

            // "Filepaths" section
            txt_InstallDir.Text = GameMeta.CurrentGameDirectoryPath;

            // "Overlay" section
            chckBox_Config_LoadingAnimation.IsChecked = (
                (!ConfigurationManager.ConfigurationFile.HasKey(ConfigurationManager.SECTION_OVERLAY, "Play_Intro")) ||         // <-- Set to "on", if not set
                ConfigurationManager.ConfigurationFile.ValueIsBoolAndTrue(ConfigurationManager.SECTION_OVERLAY, "Play_Intro")
            );
            chckBox_Config_GameDataLoadingIndicator.IsChecked = ConfigurationManager.ConfigurationFile.ValueIsBoolAndTrue(ConfigurationManager.SECTION_OVERLAY, "Display_Update_Indicator");
            txt_Config_OverlayPosX.Text = ConfigurationManager.ConfigurationFile.GetValue(ConfigurationManager.SECTION_OVERLAY, "X");
            txt_Config_OverlayPosY.Text = ConfigurationManager.ConfigurationFile.GetValue(ConfigurationManager.SECTION_OVERLAY, "Y");

            // "VI" section
            txt_Config_VIName.Text = VI.Name;
            txt_Config_VIPhoneticName.Text = VI.PhoneticName;

            for (int i = 0; i < comBox_Config_VIVoice.Items.Count; i++)
            {
                SpeechEngine.VoiceModulationModes currVoice = (SpeechEngine.VoiceModulationModes)((ValueLabelPair)comBox_Config_VIVoice.Items[i]).Value;
                if (SpeechEngine.VoiceModulation == currVoice) { comBox_Config_VIVoice.SelectedIndex = i; break; }
            }
            if (comBox_Config_VIVoice.SelectedItem == null) { comBox_Config_VIVoice.SelectedIndex = 0; }

            for (int i = 0; i < comBox_Config_SpeechRecLang.Items.Count; i++)
            {
                string currLanguageValue = ((ComboBoxItem)comBox_Config_SpeechRecLang.Items[i]).Content.ToString();
                if (SpeechEngine.Language == currLanguageValue) { comBox_Config_SpeechRecLang.SelectedIndex = i; break; }
            }
            if (comBox_Config_SpeechRecLang.SelectedItem == null) { comBox_Config_SpeechRecLang.SelectedIndex = 0; }

            // "Player" section
            txt_Config_PlayerName.Text = VI.PlayerName;
            txt_Config_PlayerPhoneticName.Text = VI.PlayerPhoneticName;


            /* Fill plugin list */
            foreach (KeyValuePair<string, List<IPlugin>> dllPlugin in PluginManager.LoadedDLLs)
            {
                for (int i = 0; i < dllPlugin.Value.Count; i++)
                {
                    comBox_PluginSelection.Items.Add(
                        new ValueLabelPair(
                            PluginManager.Plugins[i].Id,
                            "[ " + dllPlugin.Key.Substring(0, dllPlugin.Key.LastIndexOf('.')) + " ] - " + PluginManager.Plugins[i].Name
                        )
                    );
                }
            }
            if (PluginManager.Plugins.Count > 0) { comBox_PluginSelection.SelectedIndex = 0; }


            /* Configurator Settings */
            // Set the Theme
            for (int i = 0; i < comBox_ExtrasTheme.Items.Count; i++)
            {
                ComboBoxItem item = (ComboBoxItem)comBox_ExtrasTheme.Items[i];
                if (item.Content.ToString() == ConfigurationManager.ConfigurationFile.GetValue("Configurator", "Theme")) { comBox_ExtrasTheme.SelectedIndex = i; break; }
            }
            if (comBox_ExtrasTheme.SelectedItem == null) { comBox_ExtrasTheme.SelectedIndex = 0; }
            
            
            /* Update Overview tab */
            tab_Overview_Label_GotFocus(null, null);


            _openChanges = false;
            SpeechEngine.Initialize();
            PluginManager.InitializePluginDialogTrees();
            DialogTreeBuilder.DialogsActive = false;
            GameMeta.OnGameProcessStarted += GameMeta_OnGameProcessStarted;
        }
        #endregion


        #region Functions
        /// <summary> Sets the given checkbox's style.
        /// </summary>
        /// <param name="chckBox">The checkbox which style to set.</param>
        /// <param name="state">The state the checkbox's appearance should represent.</param>
        /// <param name="checkIfAtLeast">Checks the checkbox, if the state ist at least as good as the given value.</param>
        private void setCheckboxColor(CheckBox chckBox, CheckBoxColorState state, CheckBoxColorState checkIfAtLeast = CheckBoxColorState.OKAY)
        {
            chckBox.Foreground = (
                (state == CheckBoxColorState.OKAY) ? (SolidColorBrush)FindResource("SuccessForeColor") :
                (state == CheckBoxColorState.WARNING) ? (SolidColorBrush)FindResource("WarningForeColor") :
                (SolidColorBrush)FindResource("ErrorForeColor")
            );
            chckBox.BorderBrush = (
                (state == CheckBoxColorState.OKAY) ? (SolidColorBrush)FindResource("SuccessBorderColor") :
                (state == CheckBoxColorState.WARNING) ? (SolidColorBrush)FindResource("WarningBorderColor") :
                (SolidColorBrush)FindResource("ErrorBorderColor")
            );
            chckBox.Background = (
                (state == CheckBoxColorState.OKAY) ? (SolidColorBrush)FindResource("SuccessBackColor") :
                (state == CheckBoxColorState.WARNING) ? (SolidColorBrush)FindResource("WarningBackColor") :
                (SolidColorBrush)FindResource("ErrorBackColor")
            );

            chckBox.IsChecked = (state <= checkIfAtLeast);
        }


        /// <summary> Verifies whether the path in the textbox is a valid installation directory.
        /// </summary>
        private bool verifyInstallDir()
        {
            txtBox_StatusText.Visibility = System.Windows.Visibility.Visible;

            if (String.IsNullOrWhiteSpace(txt_InstallDir.Text))
            {
                // No path entered
                txtBox_StatusText.Foreground = new System.Windows.Media.SolidColorBrush(Colors.White);
                txtBox_StatusText.Text = "Please enter the installation path for " + GameMeta.GetDescription(GameMeta.CurrentGame) + "!";

                return false;
            }


            /* Check whether the entered path is a valid installation directory */
            bool pathIsValid = (
                (Directory.Exists(txt_InstallDir.Text)) &&
                (Directory.GetFiles(txt_InstallDir.Text, GameMeta.GetDescription(GameMeta.CurrentGame).Replace(' ', '*') + ".exe").Length > 0) &&
                (Directory.GetFiles(txt_InstallDir.Text, "EvochronData.evo").Length > 0)
            );

            if (pathIsValid)
            {
                // Path exists
                txtBox_StatusText.Foreground = new System.Windows.Media.SolidColorBrush(Color.FromRgb(0, 240, 0));
                txtBox_StatusText.Text = "Your installation path for " + GameMeta.GetDescription(GameMeta.CurrentGame) + " is valid!";

                // Save the game path within the configuration file
                ConfigurationManager.ConfigurationFile.SetValue("Filepaths", GameMeta.CurrentGame.ToString(), txt_InstallDir.Text);
                GameMeta.GameDetails[GameMeta.CurrentGame].UserInstallDirectory = txt_InstallDir.Text;

                return true;
            }
            else
            {
                // Entered path does not exist
                txtBox_StatusText.Foreground = new System.Windows.Media.SolidColorBrush(Color.FromRgb(220, 0, 0));
                txtBox_StatusText.Text = "Error: The path given is not a valid " + GameMeta.GetDescription(GameMeta.CurrentGame) + " installation directory!";

                return false;
            }
        }


        /// <summary> Toggles the UI lock state on or off.
        /// <para>Only the overview page will be locked completely.
        /// If the user is on a different tab when disables, all functions in that tab will still be available.
        /// Only tab-switching will not be possible on those pages.
        /// </para>
        /// </summary>
        /// <param name="lockUI">If true, locks the UI and unlocks it, if false.</param>
        private void ToggleUILock(bool lockUI)
        {
            if (lockUI)
            {
                int enabledPlugins = 0;
                for (int i = 0; i < PluginManager.Plugins.Count; i++)
                {
                    if (PluginManager.PluginFile.ValueIsBoolAndTrue(PluginManager.Plugins[i].Id.ToString(), "Enabled")) { enabledPlugins++; }
                }

                if (_openChanges)
                {
                    txt_GameReadyOverlay.Text = "There are still some unsaved changes! Press \"Cancel\" and save your settings!";
                    txt_GameReadyOverlay.BorderBrush = (SolidColorBrush)this.Resources["WarningBorderColor"];
                    txt_GameReadyOverlay.Background = (SolidColorBrush)this.Resources["WarningBackColor"];
                    txt_GameReadyOverlay.Foreground = (SolidColorBrush)this.Resources["NormalForeColor"];
                }
                else if (enabledPlugins == 0)
                {
                    txt_GameReadyOverlay.Text = "There are no plugins activated - Functionality might be restricted!";
                    txt_GameReadyOverlay.BorderBrush = (SolidColorBrush)this.Resources["WarningBorderColor"];
                    txt_GameReadyOverlay.Background = (SolidColorBrush)this.Resources["WarningBackColor"];
                    txt_GameReadyOverlay.Foreground = (SolidColorBrush)this.Resources["NormalForeColor"];
                }
                else
                {
                    txt_GameReadyOverlay.Text = "The VI is ready! - Please start the game!";
                    txt_GameReadyOverlay.BorderBrush = (SolidColorBrush)this.Resources["SuccessBorderColor"];
                    txt_GameReadyOverlay.Background = (SolidColorBrush)this.Resources["SuccessBackColor"];
                    txt_GameReadyOverlay.Foreground = (SolidColorBrush)this.Resources["NormalForeColor"];
                }

                txt_GameReadyOverlay.Visibility = System.Windows.Visibility.Visible;
                btn_Close.Tag = btn_Close.Content;
                btn_Close.Content = "Cancel";

                for (int i = 0; i < tab_Main.Items.Count; i++) { ((TabItem)tab_Main.Items[i]).IsEnabled = false; }

                btn_StartGame.IsEnabled = false;
            }
            else
            {
                btn_Close.Content = btn_Close.Tag;
                tab_Main.IsEnabled = true;
                btn_StartGame.IsEnabled = true;
                txt_GameReadyOverlay.Visibility = System.Windows.Visibility.Hidden;
                for (int i = 0; i < tab_Main.Items.Count; i++) { ((TabItem)tab_Main.Items[i]).IsEnabled = true; }
            }

            _uiLocked = lockUI;
        }


        /// <summary> Tests the pronounciation of the entered name in text-to-speech.
        /// </summary>
        /// <param name="testText">The name to test.</param>
        private void testName(string testText)
        {
            btn_Config_VINameTest.IsEnabled = false;
            btn_Config_PlayerNameTest.IsEnabled = false;

            SpeechEngine.Say(new DialogVI(testText), true, SpeechEngine.VoiceModulation, null, true);

            btn_Config_VINameTest.IsEnabled = true;
            btn_Config_PlayerNameTest.IsEnabled = true;
        }


        /// <summary> Checks and updates the status of the currently loaded plugin.
        /// </summary>
        private void validatePlugin()
        {
            if (comBox_PluginSelection.SelectedItem == null) { return; }

            // Update plugin information
            string newSelectedPluginName = ((ValueLabelPair)comBox_PluginSelection.SelectedItem).Value.ToString();
            IPlugin plugin = PluginManager.GetPlugin(newSelectedPluginName);

            if (plugin != null)
            {
                lbl_PluginName.Text = plugin.Name;
                lbl_PluginVersion.Text = "Version: " + (!String.IsNullOrWhiteSpace(plugin.Version) ? plugin.Version : "N/A");
                lbl_PluginDescription.Text = !String.IsNullOrWhiteSpace(plugin.Description) ? plugin.Description : "";
                lbl_PluginAuthor.Text = !String.IsNullOrWhiteSpace(plugin.Author) ? plugin.Author : "<Unknown Author>";
                lbl_PluginHomepage.Text = !String.IsNullOrWhiteSpace(plugin.Homepage) ? plugin.Homepage : "<Homepage N/A>";

                if (plugin.LogoImage != null)
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        System.Drawing.Bitmap bitmap = plugin.LogoImage;
                        bitmap.Save(memory, ImageFormat.Png);
                        memory.Position = 0;
                        memory.Seek(0, SeekOrigin.Begin);
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        img_PluginLogo.Source = bitmapImage;
                    }
                }
                else
                {
                    img_PluginLogo.Source = null;
                }

                // (Un-)Check the "compatibility" checkbox and update the style
                bool isCompatible = ((GameMeta.CurrentGame & plugin.CompatibilityFlags) == GameMeta.CurrentGame);
                txt_PluginCompatibility.Foreground = (
                    new System.Windows.Media.SolidColorBrush(isCompatible ? Color.FromArgb(200, 0, 200, 0) : Color.FromArgb(200, 250, 0, 0))
                );
                txt_PluginCompatibility.Background = (
                    new System.Windows.Media.SolidColorBrush(isCompatible ? Color.FromArgb(128, 0, 180, 0) : Color.FromArgb(128, 200, 0, 0))
                );
                txt_PluginCompatibility.Text = (isCompatible ? "" : "Not ") + "Compatible";
                txt_PluginCompatibility.Visibility = System.Windows.Visibility.Visible;

                // (Un-)Check the "enabled" checkbox and trigger checked/unchecked event to do the rest
                chck_PluginsEnabled.IsChecked = (
                    (!PluginManager.PluginFile.HasKey(plugin.Id.ToString(), "Enabled")) ||
                    (PluginManager.PluginFile.ValueIsBoolAndTrue(plugin.Id.ToString(), "Enabled"))
                );
                chck_PluginsEnabled_CheckedUnchecked(null, null);
            }
        }
        #endregion


        #region Events
        /// <summary> Stops the game process searh, if still ongoing.
        /// Fires, when the configurator is closing.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The cancel event arguments.</param>
        private void Configurator_Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GameMeta.StopGameProcessSearch();
        }
        

        #region Overview Events
        /// <summary> Updates the checklist on the "Overview"-tab.
        /// Fires when the "Overview"-tab is being focused.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void tab_Overview_Label_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_uiLocked) { return; }

            bool criticalError = false;

            // Install directory is valid?
            setCheckboxColor(
                chckBox_Overview_InstallDirValid,
                verifyInstallDir() ? CheckBoxColorState.OKAY : CheckBoxColorState.WARNING
            );
            chckBox_Overview_InstallDirValid.Content = "Installation path valid";
            lbl_Overview_InstallDirValid_Hints.Visibility = (chckBox_Overview_InstallDirValid.IsChecked == true) ? 
                System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            criticalError = criticalError || false;

            // SW3DG directory is present?
            setCheckboxColor(
                chckBox_Overview_SW3DGDir,
                Directory.Exists(GameMeta.DefaultGameSettingsDirectoryPath) ? CheckBoxColorState.OKAY : CheckBoxColorState.ERROR
            );
            chckBox_Overview_SW3DGDir.Content = "SW3DG game directory (" + GameMeta.DefaultGameSettingsDirectoryPath + ") available";
            lbl_Overview_SW3DGDir_Hints.Visibility = (chckBox_Overview_SW3DGDir.IsChecked == true) ? 
                System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            criticalError = criticalError || (chckBox_Overview_SW3DGDir.IsChecked != true);

            // Savedatasettings.txt has been created?
            setCheckboxColor(
                chckBox_Overview_Savedatatextssettings,
                File.Exists(GameMeta.CurrentSaveDataSettingsTextFilePath) ? CheckBoxColorState.OKAY : CheckBoxColorState.WARNING
            );
            chckBox_Overview_Savedatatextssettings.Content = (
                verifyInstallDir() ? 
                "\"SavedataSettings.txt\" found in \"" + GameMeta.CurrentGameDirectoryPath + "\"" :
                "Install directory is not valid - \"SavedataSettings.txt\" could not be found."
            );
            lbl_Overview_Savedatatextssettings_Hints.Visibility = (chckBox_Overview_Savedatatextssettings.IsChecked == true) ?
                System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            criticalError = criticalError || (chckBox_Overview_Savedatatextssettings.IsChecked != true);

            // Speech recognition engine available?
            System.Globalization.CultureInfo currentCulture = new System.Globalization.CultureInfo(SpeechEngine.Language, false);
            bool isSupported = SpeechEngine.CheckLanguageSupport(currentCulture);
            setCheckboxColor(chckBox_Overview_SpeechRecogEngine, isSupported ? CheckBoxColorState.OKAY : CheckBoxColorState.ERROR);

            chckBox_Overview_SpeechRecogEngine.Content = "Speech recognition language supported";
            lbl_Overview_SpeechRecogEngine_Hints.Visibility = (isSupported) ?
                System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            criticalError = criticalError || (chckBox_Overview_SpeechRecogEngine.IsChecked != true);

            // All plugins are compatible?
            bool pluginsCompatible = true;
            foreach(KeyValuePair<string, Dictionary<string, string>> keyal in PluginManager.PluginFile.Sections)
            {
                if (PluginManager.PluginFile.ValueIsBoolAndTrue(keyal.Key, "Enabled"))
                {
                    IPlugin currPlugin = PluginManager.GetPlugin(keyal.Key);

                    if (currPlugin == null) { continue; }

                    if ((currPlugin.CompatibilityFlags & GameMeta.CurrentGame) != GameMeta.CurrentGame)
                    {
                        pluginsCompatible = false;
                        break;
                    }
                }
            }
            setCheckboxColor(
                chckBox_Overview_PluginsCompatible,
                pluginsCompatible ? CheckBoxColorState.OKAY : CheckBoxColorState.WARNING
            );
            chckBox_Overview_PluginsCompatible.Content = "All plugins compatible with selected game";
            lbl_Overview_PluginsCompatible_Hints.Visibility = (chckBox_Overview_PluginsCompatible.IsChecked == true) ?
                System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            criticalError = criticalError || false;

            btn_StartGame.IsEnabled = !criticalError;
        }


        /// <summary> Cancels game process earch and unlocks UI or closes the application.
        /// Fires when the "Close"-button is clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            if (_uiLocked) { GameMeta.StopGameProcessSearch(); ToggleUILock(false); } else { this.Close(); }
        }


        /// <summary> Locks the UI and starts the game process check.
        /// Locks the configurator UI and waits for the game process to start.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_StartGame_Click(object sender, RoutedEventArgs e)
        {
            ToggleUILock(true);

            if (ConfigurationManager.StartupParams.IgnoreGameProcessStart)
            {
                GameMeta_OnGameProcessStarted(null, null);
            }
            else
            {
                GameMeta.CheckForGameProcess();
            }            
        }


        /// <summary> Hides the configurator, starts the overlay and awaits its termination.
        /// Hides the configurator and starts the overlay.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void GameMeta_OnGameProcessStarted(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() =>
                {
                    this.Hide();

                    // Start the Overlay
                    VacVIOverlay.OverlayWindow overlayWindow = new VacVIOverlay.OverlayWindow();
                    overlayWindow.ShowDialog();

                    VI.State = VI.VIState.OFFLINE;
                    PluginManager.LoadPlugins(true);
                    PluginManager.InitializePluginDialogTrees();
                    DialogTreeBuilder.DialogsActive = false;

                    ToggleUILock(false);
                    this.Show();
                    this.BringIntoView();
                })
            );
        }
        #endregion


        #region Configuration Events
        /// <summary> Saves the configurator settings to a file.
        /// Fires when the "Save Settings"-button has been clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            /* NOTE: Deleting characters from the window coordinate textboxes will not be caught by the event handler,
             * so once the user deletes something, it won't be updated within the coniguration.
             * So in order to save the value that's actually in the textbox, we need to update it manually here.
             */
            ConfigurationManager.ConfigurationFile.SetValue(ConfigurationManager.SECTION_OVERLAY, "X", txt_Config_OverlayPosX.Text);
            ConfigurationManager.ConfigurationFile.SetValue(ConfigurationManager.SECTION_OVERLAY, "Y", txt_Config_OverlayPosY.Text);

            ConfigurationManager.ConfigurationFile.Write(ConfigurationManager.ConfigurationFilepath);
            _openChanges = false;
        }

        
        #region Install Directory
        /// <summary> Opens the "Browse" dialog window.
        /// Fires when the "Browse"-button is clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_InstallDir_Browse_Click(object sender, RoutedEventArgs e)
        {
            // Open file browse dialog
            OpenFolderDialog folderDialog = new OpenFolderDialog();
            folderDialog.StartFolder = txt_InstallDir.Text;
            folderDialog.ShowDialog();
            if (!String.IsNullOrWhiteSpace(folderDialog.SelectedFolder)) { txt_InstallDir.Text = folderDialog.SelectedFolder; }
        }


        /// <summary> Changes the selected game and updates the configurator.
        /// Fires when the game within the game selection list changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void comBox_GameSelection_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ValueLabelPair newItemGame = (ValueLabelPair)(comBox_GameSelection.SelectedItem);
            GameMeta.CurrentGame = (GameMeta.SupportedGame)newItemGame.Value;

            verifyInstallDir();
            validatePlugin();

            ConfigurationManager.ConfigurationFile.SetValue(ConfigurationManager.SECTION_GAME, "Current_Game", newItemGame.Value.ToString());
            txt_InstallDir.Text = ConfigurationManager.ConfigurationFile.GetValue(ConfigurationManager.SECTION_FILEPATHS, newItemGame.Value.ToString());
            img_currentGame.Source = (
                (GameMeta.CurrentGame == GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (ImageSource)_imgEvochronMercenary :
                (GameMeta.CurrentGame == GameMeta.SupportedGame.EVOCHRON_LEGACY) ? (ImageSource)_imgEvochronLegacy :
                (ImageSource)_imgLogo_VI
            );
            _openChanges = true;
        }


        /// <summary> Verifies the newly entered install directory.
        /// Fires when the text inside the path-textbox changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        private void txt_InstallDir_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            verifyInstallDir();

            // Update "Extras"-Subtab
            tab_Extras_GotFocus(sender, null);
        }
        #endregion


        #region VI Settings
        /// <summary> Updates the VI's name in the current configuration.
        /// Fires when the VI's name has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        private void txt_Config_VIName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigurationManager.ConfigurationFile.SetValue("VI", "Name", txt_Config_VIName.Text);
            _openChanges = true;
        }


        /// <summary> Updates the VI's phonetic name in the current configuration.
        /// Fires when the VI's phonetic name has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        private void txt_Config_VIPhoneticName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigurationManager.ConfigurationFile.SetValue("VI", "Phonetic_Name", txt_Config_VIPhoneticName.Text);
            _openChanges = true;
        }


        /// <summary> Makes the VI say it's phonetic name for test purposes.
        /// Fires when the "Test Pronouncation"-button has been clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_Config_VINameTest_Click(object sender, RoutedEventArgs e)
        {
            testName(txt_Config_VIPhoneticName.Text);
        }


        /// <summary> Updates the VI's voice in the current configuration.
        /// Fires when VI voice has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void comBox_Config_VIVoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpeechEngine.VoiceModulationModes voice = (SpeechEngine.VoiceModulationModes)((ValueLabelPair)(((ComboBox)sender).SelectedItem)).Value;
            SpeechEngine.VoiceModulation = voice;

            ConfigurationManager.ConfigurationFile.SetValue("VI", "Voice", voice.ToString());
            _openChanges = true;
        }


        /// <summary> Updates the voice recognition language in the current configuration.
        /// Fires when the language for the speech recognition engine has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void comBox_Config_SpeechRecLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpeechEngine.Language = ((ComboBoxItem)comBox_Config_SpeechRecLang.SelectedItem).Content.ToString();

            ConfigurationManager.ConfigurationFile.SetValue("VI", "Speech_Recognition_Lang", SpeechEngine.Language);
            _openChanges = true;
        }


        /// <summary> Updates the player's name in the current configuration.
        /// Fires when the player's name has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        private void txt_Config_PlayerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigurationManager.ConfigurationFile.SetValue("Player", "Name", txt_Config_PlayerName.Text);
            _openChanges = true;
        }


        /// <summary> Updates the player's phonetic name in the current configuration.
        /// Fires when the player's phonetic name has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        private void txt_Config_PlayerPhoneticName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigurationManager.ConfigurationFile.SetValue("Player", "Phonetic_Name", txt_Config_PlayerPhoneticName.Text);
            _openChanges = true;
        }


        /// <summary> Makes the VI say the player's phonetic name for test purposes.
        /// Fires when the "Test Pronouncation"-button has been clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_Config_PlayerNameTest_Click(object sender, RoutedEventArgs e)
        {
            testName(txt_Config_PlayerPhoneticName.Text);
        }
        #endregion


        #region Overlay
        /// <summary> Updates the "play loading animatiton on startup" setting in the current configuration.
        /// Fires when the loading animation option has changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void chckBox_Config_LoadingAnimation_Checked(object sender, RoutedEventArgs e)
        {
            ConfigurationManager.ConfigurationFile.SetValue(
                ConfigurationManager.SECTION_OVERLAY,
                "Play_Intro", 
                (chckBox_Config_LoadingAnimation.IsChecked == true) ? "true" : "false"
            );
            _openChanges = true;
        }



        /// <summary> Updates the "display game data update indicator" setting in the current configuration.
        /// Fires when the game update indicator option has changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void chckBox_Config_GameDataLoadingIndicator_Checked(object sender, RoutedEventArgs e)
        {
            ConfigurationManager.ConfigurationFile.SetValue(
                ConfigurationManager.SECTION_OVERLAY,
                "Display_Update_Indicator",
                (chckBox_Config_GameDataLoadingIndicator.IsChecked == true) ? "true" : "false"
            );
            _openChanges = true;
        }


        /// <summary> Validates the overlay's entered X and Y coordinates.
        /// Fires when the values wthin the textboxes for the overlay positon have changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arguments.</param>
        private void validateNumericInput(object sender, TextCompositionEventArgs e)
        {
            string coordinate = (
                (sender == txt_Config_OverlayPosX) ? "X" :
                (sender == txt_Config_OverlayPosY) ? "Y" :
                "???"
            );

            // Check the validity of the currently added characters
            bool isValid = System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^\d+");

            if (isValid)
            {
                // Save the entire value
                ConfigurationManager.ConfigurationFile.SetValue(
                    ConfigurationManager.SECTION_OVERLAY,
                    coordinate,
                    ((TextBox)sender).Text + e.Text
                );
                _openChanges = true;
            }
            
            e.Handled = !isValid;
        }
        #endregion


        #region Extras
        /// <summary> Updates the "Extras"-tab.
        /// Fires when the "Extras"-tab is being focused.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void tab_Extras_GotFocus(object sender, RoutedEventArgs e)
        {
            bool installDirValid = verifyInstallDir();
            btn_Extras_GenerateAudioFiles.IsEnabled = installDirValid;
            txt_Extras_TargetAudioFilepath.Text = (installDirValid) ?
                "Target path: " + GameMeta.CurrentGameDirectoryPath + "\\alerts" : "The current installation directory is invalid!";
            txt_Extras_TargetAudioFilepath.Foreground = (installDirValid) ?
                (SolidColorBrush)FindResource("NormalForeColor") : (SolidColorBrush)FindResource("ErrorForeColor");
        }


        /// <summary> Generates custom, VI spoken sound files for the game.
        /// Fires when the "Generate Audio Files"-button within "Extras" is being clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_Extras_GenerateAudioFiles_Click(object sender, RoutedEventArgs e)
        {
            #region Description from the ModdingKit
            /* For the female vocal alert, use a directory named \alerts and the following filenames:

                bb-tractordis.wav = Docking tractor beam disengaged
                bb-lowfuel.wav = Caution: Fuel level low
                bb-lowalt.wav = Warning: low altitude
                bb-contracta.wav = Contract objectives accomplished
                bb-contractf.wav = Contract objectives failed
                bb-inbound.wav = Alert: Missile inbound
                bb-gravity = Caution: Entering high level gravity field     <-- Be wary of missing file extension
                bb-avioplanet.wav = Avionincs switched to planetary mode    <-- Be wary of typo
                bb-aviospace.wav = Avionics switched to space mode
                bb-nebula.wav = Caution: Entering dense nebula cloud zone
                bb-radiation.wav = Caution: Entering high level radiation zone
                bb-tractor.wav = Docking tractor beam engaged (4 second delay recommended)
             */
            #endregion

            Dictionary<string, string> fileData = new Dictionary<string, string>();
            fileData.Add("bb-tractordis.wav", "Docking tractor beam disengaged");
            fileData.Add("bb-lowfuel.wav", "Caution: Fuel level low");
            fileData.Add("bb-lowalt.wav", "Warning: low altitude");
            fileData.Add("bb-contracta.wav", "Contract objectives accomplished");
            fileData.Add("bb-contractf.wav", "Contract objectives failed");
            fileData.Add("bb-inbound.wav", "Alert: Missile inbound");
            fileData.Add("bb-gravity.wav", "Caution: Entering high level gravity field");
            fileData.Add("bb-avioplanet.wav", "Avionics switched to planetary mode");
            fileData.Add("bb-aviospace.wav", "Avionics switched to space mode");
            fileData.Add("bb-nebula.wav", "Caution: Entering dense nebula cloud zone");
            fileData.Add("bb-radiation.wav", "Caution: Entering high level radiation zone");
            fileData.Add("bb-tractor.wav", "Docking tractor beam engaged");

            string targetPath = GameMeta.CurrentGameDirectoryPath + "\\alerts\\";
            foreach (KeyValuePair<string, string> keyVal in fileData)
            {
                // Generate the audio files
                SpeechEngine.Say(
                    new DialogVI(keyVal.Value),
                    false,
                    SpeechEngine.VoiceModulation,
                    targetPath + keyVal.Key,
                    true
                );
            }
        }


        /// <summary> Changes the configurator theme.
        /// Fires when the Theme within "Extras" has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox themeBox = (ComboBox)sender;

            switch (themeBox.SelectedIndex)
            {
                case 0: this.Resources["BackgroundBrush"] = _brightThemeBrush; break;
                case 1: this.Resources["BackgroundBrush"] = _darkThemeBrush; break;

                default: themeBox.SelectedIndex = 0; break;
            }

            ConfigurationManager.ConfigurationFile.SaveValue("Configurator", "Theme", ((ComboBoxItem)themeBox.SelectedValue).Content.ToString());
        }


        /// <summary> Generates an HTML file with all available dialogs.
        /// Fires, when the "Generate HTML File" button has been clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_Extras_GenerateHtmlFile_Click(object sender, RoutedEventArgs e)
        {
            string appPath = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase
            ).Replace("file:\\", "");
            VacVI.Dialog.DialogTreeBuilder.GenerateHtmlOverview(
                appPath + "\\Dialog Tree.html",
                (chck_Extras_PrintPlayerCommandsOnly.IsChecked == true)
            );

            if (chck_Extras_AutoOpenHtml.IsChecked == true) { System.Diagnostics.Process.Start(appPath + "\\Dialog Tree.html"); }
        }
        #endregion

        #endregion


        #region Controls Events
        /// <summary> Updates the in-game controls overview.
        /// Fires when the "Controls"-tab received focus.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void tab_Controls_Label_GotFocus(object sender, RoutedEventArgs e)
        {
            KeyboardControls.BuildDatabase();
            KeyboardControls.LoadKeymap();

            grid_Controls.Children.Clear();

            ColumnDefinition col;

            // Control definition
            col = new ColumnDefinition();
            col.Width = new GridLength(1, GridUnitType.Auto);
            grid_Controls.ColumnDefinitions.Add(col);

            // Keyboard Control
            col = new ColumnDefinition();
            col.Width = new GridLength(1, GridUnitType.Auto);
            grid_Controls.ColumnDefinitions.Add(col);

            // Mouse Control
            col = new ColumnDefinition();
            col.Width = new GridLength(1, GridUnitType.Auto);
            grid_Controls.ColumnDefinitions.Add(col);

            // Joystick Control
            col = new ColumnDefinition();
            col.Width = new GridLength(1, GridUnitType.Auto);
            grid_Controls.ColumnDefinitions.Add(col);

            // Rest width
            col = new ColumnDefinition();
            col.Width = new GridLength(1, GridUnitType.Star);
            grid_Controls.ColumnDefinitions.Add(col);

            Thickness controlsMargin = new Thickness(15, 0, 0, 0);
            int currRow = 0;
            foreach (KeyValuePair<GameAction, ActionDetail> actionDetail in KeyboardControls.GameActions)
            {
                // Create new row
                grid_Controls.RowDefinitions.Add(new RowDefinition());


                // Draw background row color
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Margin = new Thickness(0);
                rect.Fill = new SolidColorBrush(Color.FromArgb((byte)(((currRow % 2) == 0) ? 50 : 0), 219, 244, 255));
                rect.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                rect.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                grid_Controls.Children.Add(rect);
                Grid.SetRow(rect, currRow);
                Grid.SetColumn(rect, 0);
                Grid.SetColumnSpan(rect, grid_Controls.ColumnDefinitions.Count);


                // Control description
                Label descr = new Label();
                descr.FontWeight = FontWeights.Bold;
                descr.Foreground = new SolidColorBrush(Colors.White);
                descr.Content = actionDetail.Value.Description;
                descr.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                grid_Controls.Children.Add(descr);
                Grid.SetColumn(descr, 0);
                Grid.SetRow(descr, currRow);


                // Keyboard Control
                string keyDescription = (
                    (actionDetail.Value.Scancode > 0) ? (
                        (actionDetail.Value.IsAltAction ? DIKCodes.GetDescription(DIKCodes.Keys.LMENU) + " + " : "") +
                        DIKCodes.GetDescription((DIKCodes.Keys)actionDetail.Value.Scancode)
                    ) :
                    ""
                );
                Label cntrl = new Label();
                cntrl.FontWeight = FontWeights.Bold;
                cntrl.Foreground = new SolidColorBrush(Colors.White);
                cntrl.Content = keyDescription;
                cntrl.Margin = controlsMargin;
                cntrl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                grid_Controls.Children.Add(cntrl);
                Grid.SetColumn(cntrl, 1);
                Grid.SetRow(cntrl, currRow);

                currRow++;
            }
        }
        #endregion


        #region Plugins Events
        /// <summary> Updates plugin information.
        /// Fires when the plugin within the plugin selection list changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void comBox_PluginSelection_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Update plugin information
            validatePlugin();

            // Build configuration menu
            string newSelectedPluginName = ((ValueLabelPair)comBox_PluginSelection.SelectedItem).Value.ToString();
            IPlugin plugin = PluginManager.GetPlugin(newSelectedPluginName);

            grid_pluginParameterGrid.RowDefinitions.Clear();
            grid_pluginParameterGrid.Children.Clear();

            if (PluginManager.PluginDefaults.ContainsKey(plugin.Id.ToString()))
            {
                Dictionary<string, PluginParameterDefault> pluginParams = PluginManager.PluginDefaults[plugin.Id.ToString()];

                if (pluginParams.Count > 0)
                {
                    // Change config content to the stack panel
                    grid_pluginParameterGrid.Visibility = System.Windows.Visibility.Visible;
                    lbl_noParamsToConfigure.Visibility = System.Windows.Visibility.Hidden;

                    Thickness globalMargin = new Thickness(5);

                    int currRow = 0;
                    RowDefinition newRow;
                    foreach (KeyValuePair<string, PluginParameterDefault> pluginDefault in pluginParams)
                    {
                        PluginParameterDefault currPluginParam = pluginDefault.Value;
                        string currSetVal = PluginManager.PluginFile.GetValue(plugin.Id.ToString(), currPluginParam.Key);

                        #region Create the configuration stack
                        newRow = new RowDefinition();
                        newRow.Height = new GridLength(1, GridUnitType.Auto);
                        grid_pluginParameterGrid.RowDefinitions.Add(newRow);

                        // Add the parameter label
                        TextBlock paramKey = new TextBlock();
                        paramKey.Text = currPluginParam.Key + ":";
                        paramKey.Margin = globalMargin;
                        paramKey.FontWeight = FontWeights.Bold;
                        paramKey.TextWrapping = TextWrapping.Wrap;
                        paramKey.Foreground = new System.Windows.Media.SolidColorBrush(Colors.White);
                        paramKey.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        paramKey.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                        grid_pluginParameterGrid.Children.Add(paramKey);
                        Grid.SetRow(paramKey, currRow);
                        Grid.SetColumn(paramKey, 0);
                        
                        // Add the value selection - check for list of allowed values for this parameter
                        if (currPluginParam.AllowedValues != null)
                        {
                            // Plugin has a list of allowed values - create combobox
                            ComboBox paramValue = new ComboBox();
                            paramValue.FontWeight = FontWeights.Bold;
                            paramValue.Foreground = new System.Windows.Media.SolidColorBrush(Colors.Black);
                            paramValue.Margin = globalMargin;
                            paramValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                            paramValue.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                            paramValue.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;

                            paramValue.Tag = currPluginParam.Key;
                            paramValue.SelectionChanged += onParamValueChanged;

                            for (int i = 0; i < currPluginParam.AllowedValues.Length; i++) { paramValue.Items.Add(currPluginParam.AllowedValues[i]); }
                            paramValue.SelectedValue = currSetVal;

                            grid_pluginParameterGrid.Children.Add(paramValue);
                            Grid.SetRow(paramValue, currRow);
                            Grid.SetColumn(paramValue, 1);
                        }
                        else
                        {
                            // Plugin has no list of allowed values - create textbox (anything goes)
                            TextBox paramValue = new TextBox();
                            paramValue.Text = currSetVal;
                            paramValue.FontWeight = FontWeights.Bold;
                            paramValue.BorderBrush = new System.Windows.Media.SolidColorBrush(Color.FromArgb((int)(255 * 0.2), 171, 173, 179));
                            paramValue.Background = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                            paramValue.Foreground = new System.Windows.Media.SolidColorBrush(Colors.White);
                            paramValue.Margin = globalMargin;
                            paramValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                            paramValue.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                            paramValue.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;

                            paramValue.Tag = currPluginParam.Key;
                            paramValue.TextChanged += onParamValueChanged;

                            grid_pluginParameterGrid.Children.Add(paramValue);
                            Grid.SetRow(paramValue, currRow);
                            Grid.SetColumn(paramValue, 1);
                        }
                        #endregion

                        #region Add the parameter description below
                        if (currPluginParam.DefaultValue != null)
                        {
                            newRow = new RowDefinition();
                            newRow.Height = new GridLength(1, GridUnitType.Auto);
                            grid_pluginParameterGrid.RowDefinitions.Add(newRow);
                            currRow++;

                            TextBox descrBox = new TextBox();
                            descrBox.Text = (!String.IsNullOrWhiteSpace(currPluginParam.Description)) ? currPluginParam.Description : String.Empty;
                            descrBox.Text += (
                                (String.IsNullOrEmpty(descrBox.Text) ? "" : "\n\n") + 
                                "(Default is: " + currPluginParam.DefaultValue + ")"
                            );

                            descrBox.Focusable = false;
                            descrBox.IsHitTestVisible = false;
                            descrBox.IsReadOnly = true;
                            descrBox.FontWeight = FontWeights.Bold;
                            descrBox.BorderBrush = new System.Windows.Media.SolidColorBrush(Color.FromArgb(255, 159, 185, 241));
                            descrBox.Background = new System.Windows.Media.SolidColorBrush(Color.FromArgb(85, 159, 211, 241));
                            descrBox.Foreground = new System.Windows.Media.SolidColorBrush(Colors.White);
                            descrBox.Padding = new Thickness(5);
                            descrBox.Margin = new Thickness(globalMargin.Left, globalMargin.Top, globalMargin.Right, 25);
                            descrBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                            descrBox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                            descrBox.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                            descrBox.TextWrapping = TextWrapping.Wrap;

                            grid_pluginParameterGrid.Children.Add(descrBox);
                            Grid.SetRow(descrBox, currRow);
                            Grid.SetColumn(descrBox, 0);
                            Grid.SetColumnSpan(descrBox, 2);
                        }
                        #endregion

                        currRow++;
                    }
                }
                else
                {
                    // Add the message to the configuration window
                    grid_pluginParameterGrid.Visibility = System.Windows.Visibility.Hidden;
                    lbl_noParamsToConfigure.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }


        /// <summary> Saves the edited plugin value for values in a combobox to file.
        /// Fires when a parameter has been changed under plugin configuration via combobox.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void onParamValueChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedValue == null) { return; }

            string newSelectedPluginName = ((ValueLabelPair)comBox_PluginSelection.SelectedItem).Value.ToString();
            IPlugin plugin = PluginManager.GetPlugin(newSelectedPluginName);

            string currParamValue = ((ComboBox)sender).SelectedValue.ToString();
            string currParamKey = ((ComboBox)sender).Tag.ToString();

            PluginManager.PluginFile.SetValue(plugin.Id.ToString(), currParamKey, currParamValue);
            PluginManager.PluginFile.Write(PluginManager.PluginConfigPath);
        }


        /// <summary> Saves the edited plugin value for values in a textbox to file.
        /// Fires when a parameter has been changed under plugin configuration via textbox.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        private void onParamValueChanged(object sender, TextChangedEventArgs e)
        {
            string newSelectedPluginName = ((ValueLabelPair)comBox_PluginSelection.SelectedItem).Value.ToString();
            IPlugin plugin = PluginManager.GetPlugin(newSelectedPluginName);

            string currParamKey = ((TextBox)sender).Tag.ToString();

            PluginManager.PluginFile.SetValue(plugin.Id.ToString(), currParamKey, ((TextBox)sender).Text);
            PluginManager.PluginFile.Write(PluginManager.PluginConfigPath);
        }


        /// <summary> Saves a plugin's enabled/disabled state to file.
        /// Fires when the "Enabled"-checkbox under "Plugins" has changed status.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void chck_PluginsEnabled_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            string newSelectedPluginName = ((ValueLabelPair)comBox_PluginSelection.SelectedItem).Value.ToString();
            IPlugin plugin = PluginManager.GetPlugin(newSelectedPluginName);

            if (plugin != null)
            {
                // Update the enabled state for this plugin inside the plugins.ini
                PluginManager.PluginFile.SetValue(plugin.Id.ToString(), "Enabled", (chck_PluginsEnabled.IsChecked == true) ? "True" : "False");
                PluginManager.PluginFile.Write(PluginManager.PluginConfigPath);
            }

            // Update the "Enabled" checkbox status and style 
            bool isCompatible = ((GameMeta.CurrentGame & plugin.CompatibilityFlags) == GameMeta.CurrentGame);
            setCheckboxColor(
                chck_PluginsEnabled,
                (isCompatible && (chck_PluginsEnabled.IsChecked == true)) ? CheckBoxColorState.OKAY : 
                (!isCompatible && (chck_PluginsEnabled.IsChecked == true)) ? CheckBoxColorState.WARNING : 
                CheckBoxColorState.ERROR,
                CheckBoxColorState.WARNING
            );
            chck_PluginsEnabled.Content = ((chck_PluginsEnabled.IsChecked == true) ? "En" : "Dis") + "abled";
        }
        #endregion


        #region About Events
        /// <summary> Opens a hyperlink in the "About" tab.
        /// Fires when a Hyperlink has been clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The request navigate event arguments.</param>
        private void OnNavigateURL(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
        #endregion
        
        #endregion
    }
}
