﻿using System;
using System.Windows;
using System.IO;
using EvoVI;
using EvoVI.PluginContracts;
using System.Windows.Controls;
using System.Windows.Media;
using EvoVI.Database;
using System.Collections.Generic;
using EvoVI.Engine;
using System.Windows.Input;

namespace EvoVIConfigurator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class ConfiguratorWindow : Window
    {
        #region DLL Imports
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int MapVirtualKey(uint uCode, uint uMapType);
        #endregion


        #region Structs
        private struct GameEntry
        {
            #region Variables
            GameMeta.SupportedGame _value;
            string _displayValue;
            #endregion


            #region Properties
            public GameMeta.SupportedGame Value
            {
                get { return _value; }
                set { _value = value; }
            }

            public string DisplayValue
            {
                get { return _displayValue; }
                set { _displayValue = value; }
            }
            #endregion


            public GameEntry(GameMeta.SupportedGame pValue)
            {
                _value = pValue;
                _displayValue = GameMeta.GetDescription(pValue);
            }
        }

        private struct VIVoice
        {
            #region Variables
            SpeechEngine.VoiceModulationModes _value;
            string _displayValue;
            #endregion


            #region Properties
            public SpeechEngine.VoiceModulationModes Value
            {
                get { return _value; }
                set { _value = value; }
            }

            public string DisplayValue
            {
                get { return _displayValue; }
                set { _displayValue = value; }
            }
            #endregion


            public VIVoice(SpeechEngine.VoiceModulationModes pValue)
            {
                _value = pValue;
                
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
        private enum CheckBoxColorState
        {
            NONE = -1,
            OKAY = 0,
            WARNING = 1,
            ERROR = 2
        };
        #endregion


        #region Variables
        private Label lbl_noParamsToConfigure;
        private List<Key> keyDowns = new List<Key>();
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


            /* Build "No parameters to configure!"-textbox in configuration window */
            lbl_noParamsToConfigure = new Label();
            lbl_noParamsToConfigure.Content = "No parameters to configure!";
            lbl_noParamsToConfigure.FontWeight = FontWeights.Bold;
            lbl_noParamsToConfigure.Margin = new Thickness(10);
            lbl_noParamsToConfigure.Foreground = (new System.Windows.Media.SolidColorBrush(Colors.WhiteSmoke));
            lbl_noParamsToConfigure.Background = (new System.Windows.Media.SolidColorBrush(Color.FromArgb(128, 200, 200, 200)));
            lbl_noParamsToConfigure.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            lbl_noParamsToConfigure.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            lbl_noParamsToConfigure.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            lbl_noParamsToConfigure.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;


            /* Load main configuration */
            // "Game" section
            for (int i = 0; i < comBox_GameSelection.Items.Count; i++)
            {
                if (GameMeta.CurrentGame == ((GameEntry)comBox_GameSelection.Items[i]).Value)
                {
                    comBox_GameSelection.SelectedIndex = i;
                    break;
                }
            }
            if (comBox_GameSelection.SelectedItem == null) { comBox_GameSelection.SelectedIndex = 0; }

            // "Filepaths" section
            txt_InstallDir.Text = GameMeta.CurrentGameDirectoryPath;

            // "Overlay" section
            chckBox_Config_LoadingAnimation.IsChecked = ConfigurationManager.ConfigurationFile.ValueIsBoolAndTrue(ConfigurationManager.SECTION_OVERLAY, "Play_Intro");

            // "VI" section
            txt_Config_VIName.Text = VI.Name;
            txt_Config_VIPhoneticName.Text = VI.PhoneticName;

            for (int i = 0; i < comBox_Config_VIVoice.Items.Count; i++)
            {
                SpeechEngine.VoiceModulationModes currVoice = ((VIVoice)comBox_Config_VIVoice.Items[i]).Value;
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
            txt_Config_PlayerName.Text = ConfigurationManager.ConfigurationFile.GetValue(ConfigurationManager.SECTION_PLAYER, "Name");
            txt_Config_PlayerPhoneticName.Text = ConfigurationManager.ConfigurationFile.GetValue(ConfigurationManager.SECTION_PLAYER, "Phonetic_Name");


            /* Fill plugin list */
            for (int i = 0; i < PluginManager.Plugins.Count; i++)
            {
                ComboBoxItem cmbx = new ComboBoxItem();
                cmbx.Content = PluginManager.Plugins[i].Name;
                comBox_PluginSelection.Items.Add(cmbx);
            }
            if (PluginManager.Plugins.Count > 0) { comBox_PluginSelection.SelectedIndex = 0; }


            /* Update Overview tab */
            tab_Overview_Label_GotFocus(null, null);
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
                new System.Windows.Media.SolidColorBrush(
                    (state == CheckBoxColorState.OKAY) ? Color.FromArgb(200, 0, 200, 0) :
                    (state == CheckBoxColorState.WARNING) ? Color.FromArgb(200, 200, 180, 0) :
                    Color.FromArgb(200, 250, 0, 0)
                )
            );
            chckBox.BorderBrush = (
                new System.Windows.Media.SolidColorBrush(
                    (state == CheckBoxColorState.OKAY) ? Color.FromArgb(200, 0, 200, 0) :
                    (state == CheckBoxColorState.WARNING) ? Color.FromArgb(200, 200, 180, 0) :
                    Color.FromArgb(200, 250, 0, 0)
                )
            );
            chckBox.Background = (
                new System.Windows.Media.SolidColorBrush(
                    (state == CheckBoxColorState.OKAY) ? Color.FromArgb(128, 0, 200, 0) :
                    (state == CheckBoxColorState.WARNING) ? Color.FromArgb(128, 200, 180, 0) :
                    Color.FromArgb(128, 250, 0, 0)
                )
            );

            chckBox.IsChecked = (state <= checkIfAtLeast);
        }
        #endregion


        #region Events
        
        #region Overview Events
        /// <summary> Fires when the "Overview"-tab is being focused.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void tab_Overview_Label_GotFocus(object sender, RoutedEventArgs e)
        {
            // Install directory is valid?
            setCheckboxColor(
                chckBox_Overview_InstallDirValid,
                verifyInstallDir() ? CheckBoxColorState.OKAY : CheckBoxColorState.ERROR
            );
            chckBox_Overview_InstallDirValid.Content = "Installation path valid";

            // SW3DG directory is present?
            setCheckboxColor(
                chckBox_Overview_SW3DGDir,
                Directory.Exists(GameMeta.DEFAULT_SAVEDATA_PATH) ? CheckBoxColorState.OKAY : CheckBoxColorState.ERROR
            );
            chckBox_Overview_SW3DGDir.Content = "SW3DG directory (" + GameMeta.DEFAULT_SAVEDATA_PATH + ") available";

            // Savedatasettings.txt has been created?
            setCheckboxColor(
                chckBox_Overview_Savedatatextssettings,
                File.Exists(GameMeta.CurrentSaveDataSettingsTextFilePath) ? CheckBoxColorState.OKAY : CheckBoxColorState.WARNING
            );
            chckBox_Overview_Savedatatextssettings.Content = "\"SavedataSettings.txt\" found in \"" + GameMeta.CurrentGameDirectoryPath + "\"";

            // Speech recognition engine available?
            try
            {
                new System.Speech.Recognition.SpeechRecognitionEngine(
                    new System.Globalization.CultureInfo(SpeechEngine.Language, false)
                );
                setCheckboxColor(chckBox_Overview_SpeechRecogEngine, CheckBoxColorState.OKAY);
            }
            catch (ArgumentException)
            {
                setCheckboxColor(chckBox_Overview_SpeechRecogEngine, CheckBoxColorState.ERROR);
            }
            chckBox_Overview_SpeechRecogEngine.Content = "Speech recognition language supported";

            // All plugins are compabtible?
            bool pluginsCompatible = true;
            foreach(KeyValuePair<string, Dictionary<string, string>> keyal in PluginManager.PluginFile.Sections)
            {
                if (PluginManager.PluginFile.ValueIsBoolAndTrue(keyal.Key, "Enabled"))
                {
                    IPlugin currPlugin = PluginManager.GetPlugin(keyal.Key);

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
        }


        /// <summary> Fires when the "Close"-button is clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion


        #region Configuration Events
        /// <summary> Fires when the "Browse"-button is clicked.
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


        /// <summary> Fires when the game within the game selection list changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void comBox_GameSelection_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            GameEntry newItemGame = (GameEntry)comBox_GameSelection.SelectedItem;
            GameMeta.CurrentGame = newItemGame.Value;

            verifyInstallDir();
            validatePlugin();

            ConfigurationManager.ConfigurationFile.SetValue(ConfigurationManager.SECTION_GAME, "Current_Game", newItemGame.Value.ToString());
            txt_InstallDir.Text = ConfigurationManager.ConfigurationFile.GetValue(ConfigurationManager.SECTION_FILEPATHS, newItemGame.Value.ToString());
        }


        /// <summary> Fires when the text inside the path-textbox changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        private void txt_InstallDir_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            verifyInstallDir();
        }


        /// <summary> Fires when the VI's name has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        private void txt_Config_VIName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigurationManager.ConfigurationFile.SetValue("VI", "Name", txt_Config_VIName.Text);
        }


        /// <summary> Fires when the VI's phonetic name has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        private void txt_Config_VIPhoneticName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigurationManager.ConfigurationFile.SetValue("VI", "Phonetic_Name", txt_Config_VIPhoneticName.Text);
        }


        /// <summary> Fires when the "Test Pronouncation"-button has been clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_Config_VINameTest_Click(object sender, RoutedEventArgs e)
        {
            testName(txt_Config_VIPhoneticName.Text);
        }


        /// <summary> Fires when VI voice has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void comBox_Config_VIVoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpeechEngine.VoiceModulationModes voice = ((VIVoice)(((ComboBox)sender).SelectedItem)).Value;
            SpeechEngine.VoiceModulation = voice;

            ConfigurationManager.ConfigurationFile.SetValue("VI", "Voice", voice.ToString());
        }


        /// <summary> Fires when the language for the speech recognition engine has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void comBox_Config_SpeechRecLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpeechEngine.Language = ((ComboBoxItem)comBox_Config_SpeechRecLang.SelectedItem).Content.ToString();

            ConfigurationManager.ConfigurationFile.SetValue("VI", "Speech_Recognition_Lang", SpeechEngine.Language);
        }


        /// <summary> Fires when the player's name has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        private void txt_Config_PlayerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigurationManager.ConfigurationFile.SetValue("Player", "Name", txt_Config_PlayerName.Text);
        }


        /// <summary> Fires when the player's phonetic name has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        private void txt_Config_PlayerPhoneticName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigurationManager.ConfigurationFile.SetValue("Player", "Phonetic_Name", txt_Config_PlayerPhoneticName.Text);
        }


        /// <summary> Fires when the "Test Pronouncation"-button has been clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_Config_PlayerNameTest_Click(object sender, RoutedEventArgs e)
        {
            testName(txt_Config_PlayerPhoneticName.Text);
        }


        /// <summary> Fires when the player's phonetic name has been changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        private void chckBox_Config_LoadingAnimation_Checked(object sender, RoutedEventArgs e)
        {
            ConfigurationManager.ConfigurationFile.SetValue("Overlay", "Play_Intro", (chckBox_Config_LoadingAnimation.IsChecked == true) ? "true" : "false");
        }


        /// <summary> Tests the pronounciation of the entered name in text-to-speech.
        /// </summary>
        /// <param name="testText">The name to test.</param>
        private void testName(string testText)
        {
            btn_Config_VINameTest.IsEnabled = false;
            btn_Config_PlayerNameTest.IsEnabled = false;

            SpeechEngine.Say(testText, false);

            btn_Config_VINameTest.IsEnabled = true;
            btn_Config_PlayerNameTest.IsEnabled = true;
        }


        /// <summary> Fires when the "Save Settings"-button has been clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationManager.ConfigurationFile.Write(ConfigurationManager.ConfigurationFilepath);
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
        #endregion


        #region Controls Events
        /// <summary> Fires when the "Controls"-tab received focus.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void tab_Controls_Label_GotFocus(object sender, RoutedEventArgs e)
        {
            KeyboardControls.BuildDatabase();
            KeyboardControls.LoadKeymap();
            bool highlightBackground = true;

            stck_Controls.Children.Clear();
            stck_ControlDescr.Children.Clear();

            foreach (KeyValuePair<GameAction, ActionDetail> actionDetail in KeyboardControls.GameActions)
            {
                SolidColorBrush currBackgroundBrush = new SolidColorBrush(
                    Color.FromArgb((byte)(highlightBackground ? 50 : 0), 219, 244, 255)
                );

                Label descr = new Label();
                descr.FontWeight = FontWeights.Bold;
                descr.Foreground = new SolidColorBrush(Colors.White);
                descr.Content = actionDetail.Value.Description;
                descr.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                descr.Background = currBackgroundBrush;
                stck_ControlDescr.Children.Add(descr);

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
                cntrl.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                cntrl.Background = currBackgroundBrush;
                stck_Controls.Children.Add(cntrl);

                highlightBackground = !highlightBackground;
            }
        }
        #endregion


        #region Plugins Events
        /// <summary> Fires when the plugin within the plugin selection list changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void comBox_PluginSelection_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Update plugin information
            validatePlugin();

            // Build configuration menu
            string newItemText = ((ComboBoxItem)comBox_PluginSelection.SelectedItem).Content.ToString();
            IPlugin plugin = PluginManager.GetPlugin(newItemText);
            Dictionary<string, string> pluginParams = PluginManager.PluginFile.GetSectionAttributes(plugin.Name);

            stck_PluginsParameters.Children.Clear();

            if (pluginParams != null)
            {
                // Filter out the "enabled" attribute from the parameter count
                int customParamsCount = pluginParams.Count;
                if (PluginManager.PluginFile.HasKey(plugin.Name, "Enabled")) { customParamsCount--; }

                if (customParamsCount > 0)
                {
                    // Change config content to the stack panel
                    grpBox_PluginsConfigWndow.Content = stck_PluginsParameters;

                    foreach (KeyValuePair<string, string> attributes in pluginParams)
                    {
                        if (String.Equals(attributes.Key, "Enabled", StringComparison.InvariantCultureIgnoreCase)) { continue; }

                        #region Create the configuration stack panel
                        // Create keyval stack panel
                        StackPanel stckPanel = new StackPanel();
                        stckPanel.Orientation = Orientation.Horizontal;
                        stckPanel.Margin = new Thickness(0, 5, 0, 5);
                        stckPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        stckPanel.VerticalAlignment = System.Windows.VerticalAlignment.Top;

                        // Add the parameter label
                        Label paramKey = new Label();
                        paramKey.Content = attributes.Key + ":";
                        paramKey.FontWeight = FontWeights.Bold;
                        paramKey.Foreground = new System.Windows.Media.SolidColorBrush(Colors.White);
                        paramKey.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        paramKey.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        paramKey.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                        paramKey.MaxWidth = 250;
                        stckPanel.Children.Add(paramKey);

                        // Add the value selection - check for list of allowed values for this parameter
                        if (
                            (PluginManager.PluginDefaults.ContainsKey(plugin.Name)) &&
                            (PluginManager.PluginDefaults[plugin.Name].ContainsKey(attributes.Key)) &&
                            (PluginManager.PluginDefaults[plugin.Name][attributes.Key].AllowedValues != null)
                        )
                        {
                            // Plugin has a list of allowed values - create combobox
                            ComboBox paramValue = new ComboBox();
                            paramValue.FontWeight = FontWeights.Bold;
                            paramValue.Foreground = new System.Windows.Media.SolidColorBrush(Colors.Black);
                            paramValue.MinWidth = 120;
                            paramValue.Margin = new Thickness(5, 0, 0, 0);
                            paramValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                            paramValue.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                            paramValue.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;

                            paramValue.Tag = attributes.Key;
                            paramValue.SelectionChanged += onParamValueChanged;

                            string[] allowedVals = PluginManager.PluginDefaults[plugin.Name][attributes.Key].AllowedValues;
                            for (int i = 0; i < allowedVals.Length; i++) { paramValue.Items.Add(allowedVals[i]); }
                            paramValue.SelectedValue = attributes.Value;

                            stckPanel.Children.Add(paramValue);
                        }
                        else
                        {
                            // Plugin has no list of allowed values - create textbox (anything goes)
                            TextBox paramValue = new TextBox();
                            paramValue.Text = attributes.Value;
                            paramValue.FontWeight = FontWeights.Bold;
                            paramValue.BorderBrush = new System.Windows.Media.SolidColorBrush(Color.FromArgb((int)(255 * 0.2), 171, 173, 179));
                            paramValue.Background = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                            paramValue.Foreground = new System.Windows.Media.SolidColorBrush(Colors.White);
                            paramValue.MinWidth = 120;
                            paramValue.Margin = new Thickness(5, 0, 0, 0);
                            paramValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                            paramValue.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                            paramValue.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;

                            paramValue.Tag = attributes.Key;
                            paramValue.TextChanged += onParamValueChanged;

                            stckPanel.Children.Add(paramValue);
                        }
                        #endregion

                        // Add the stack panel to plugin configurations
                        stck_PluginsParameters.Children.Add(stckPanel);
                        stckPanel.InvalidateArrange();
                    }
                }
                else
                {
                    // Add the message to the configuration window
                    grpBox_PluginsConfigWndow.Content = lbl_noParamsToConfigure;
                }
            }
        }


        /// <summary> Fires when a parameter has been changed under plugin configuration via combobox.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The selection changed event arguments.</param>
        void onParamValueChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedValue == null) { return; }

            string newItemText = ((ComboBoxItem)comBox_PluginSelection.SelectedItem).Content.ToString();
            IPlugin plugin = PluginManager.GetPlugin(newItemText);

            string currParamValue = ((ComboBox)sender).SelectedValue.ToString();
            string currParamKey = ((ComboBox)sender).Tag.ToString();

            PluginManager.PluginFile.SetValue(plugin.Name, currParamKey, currParamValue);
            PluginManager.PluginFile.Write(PluginManager.PluginConfigPath);
        }


        /// <summary> Fires when a parameter has been changed under plugin configuration via textbox.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        void onParamValueChanged(object sender, TextChangedEventArgs e)
        {
            string newItemText = ((ComboBoxItem)comBox_PluginSelection.SelectedItem).Content.ToString();
            IPlugin plugin = PluginManager.GetPlugin(newItemText);

            string currParamKey = ((TextBox)sender).Tag.ToString();

            PluginManager.PluginFile.SetValue(plugin.Name, currParamKey, ((TextBox)sender).Text);
            PluginManager.PluginFile.Write(PluginManager.PluginConfigPath);
        }


        /// <summary> Fires when the "Enabled"-checkbox under "Plugins" has changed status.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void chck_PluginsEnabled_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            string newItemText = ((ComboBoxItem)comBox_PluginSelection.SelectedItem).Content.ToString();
            IPlugin plugin = PluginManager.GetPlugin(newItemText);

            if (plugin != null)
            {
                // Update the enabled state for this plugin inside the plugins.ini
                PluginManager.PluginFile.SetValue(plugin.Name, "Enabled", (chck_PluginsEnabled.IsChecked == true) ? "True" : "False");
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


        /// <summary> Checks and updates the status of the currently loaded plugin.
        /// </summary>
        private void validatePlugin()
        {
            if (comBox_PluginSelection.SelectedItem == null) { return; }

            // Update plugin information
            string newItemText = ((ComboBoxItem)comBox_PluginSelection.SelectedItem).Content.ToString();
            IPlugin plugin = PluginManager.GetPlugin(newItemText);

            if (plugin != null)
            {
                lbl_PluginName.Text = plugin.Name;
                lbl_PluginVersion.Text = "Version: " + (!String.IsNullOrWhiteSpace(plugin.Version) ? plugin.Version : "N/A");
                lbl_PluginDescription.Text = !String.IsNullOrWhiteSpace(plugin.Description) ? plugin.Description : "";
                lbl_PluginAuthor.Text = !String.IsNullOrWhiteSpace(plugin.Author) ? plugin.Author : "<Unknown Author>";
                lbl_PluginHomepage.Text = !String.IsNullOrWhiteSpace(plugin.Homepage) ? plugin.Homepage : "<Homepage N/A>";

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
                    (!PluginManager.PluginFile.HasKey(plugin.Name, "Enabled")) ||
                    (PluginManager.PluginFile.ValueIsBoolAndTrue(plugin.Name, "Enabled"))
                );
                chck_PluginsEnabled_CheckedUnchecked(null, null);
            }
        }
        #endregion
                
        #endregion
    }
}
