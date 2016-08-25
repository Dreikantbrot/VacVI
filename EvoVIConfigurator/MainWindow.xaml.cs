using System;
using System.Windows;
using System.IO;
using EvoVI;
using EvoVI.PluginContracts;
using System.Windows.Controls;
using System.Windows.Media;
using EvoVI.Database;
using System.Collections.Generic;

namespace EvoVIConfigurator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
        #endregion


        #region Variables
        private Label lbl_noParamsToConfigure;
        #endregion


        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

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

            /* Fill games list */
            comBox_GameSelection.Items.Clear();
            comBox_GameSelection.Items.Add(new GameEntry(GameMeta.SupportedGame.EVOCHRON_MERCENARY));
            comBox_GameSelection.Items.Add(new GameEntry(GameMeta.SupportedGame.EVOCHRON_LEGACY));
            comBox_GameSelection.SelectedIndex = 0;

            /* Fill plugin list */
            PluginManager.LoadPlugins(true);
            for (int i = 0; i < PluginManager.Plugins.Count; i++)
            {
                ComboBoxItem cmbx = new ComboBoxItem();
                cmbx.Content = PluginManager.Plugins[i].Name;
                comBox_PluginSelection.Items.Add(cmbx);
            }

            if (PluginManager.Plugins.Count > 0) { comBox_PluginSelection.SelectedIndex = 0; }
        }
        #endregion


        #region Events
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
            verifyInstallDir();
            validatePlugin();
        }


        /// <summary> Fires when the text inside the path-textbox changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text changed event arguments.</param>
        private void txt_InstallDir_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            verifyInstallDir();
        }


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
            GameEntry currGame = (GameEntry)comBox_GameSelection.SelectedItem;
            bool isCompatible = ((currGame.Value & plugin.CompatibilityFlags) == currGame.Value);
            chck_PluginsEnabled.Foreground = (
                new System.Windows.Media.SolidColorBrush(
                    (isCompatible && (chck_PluginsEnabled.IsChecked == true)) ? Color.FromArgb(200, 0, 200, 0) :
                    (!isCompatible && (chck_PluginsEnabled.IsChecked == true)) ? Color.FromArgb(200, 200, 180, 0) : 
                    Color.FromArgb(200, 250, 0, 0)
                )
            );
            chck_PluginsEnabled.BorderBrush = (
                new System.Windows.Media.SolidColorBrush(
                    (isCompatible && (chck_PluginsEnabled.IsChecked == true)) ? Color.FromArgb(200, 0, 200, 0) :
                    (!isCompatible && (chck_PluginsEnabled.IsChecked == true)) ? Color.FromArgb(200, 200, 180, 0) :
                    Color.FromArgb(200, 250, 0, 0)
                )
            );
            chck_PluginsEnabled.Background = (
                new System.Windows.Media.SolidColorBrush(
                    (isCompatible && (chck_PluginsEnabled.IsChecked == true)) ? Color.FromArgb(128, 0, 200, 0) :
                    (!isCompatible && (chck_PluginsEnabled.IsChecked == true)) ? Color.FromArgb(128, 200, 180, 0) :
                    Color.FromArgb(128, 250, 0, 0)
                )
            );
            chck_PluginsEnabled.Content = ((chck_PluginsEnabled.IsChecked == true) ? "En" : "Dis") + "abled";
        }
        #endregion


        #region Functions
        /// <summary> Verifies whether the path in the textbox is a valid installation directory.
        /// </summary>
        private void verifyInstallDir()
        {
            GameEntry newItemGame = (GameEntry)comBox_GameSelection.SelectedItem;
            txtBox_StatusText.Visibility = System.Windows.Visibility.Visible;

            if (String.IsNullOrWhiteSpace(txt_InstallDir.Text))
            {
                // No path entered
                txtBox_StatusText.Foreground = new System.Windows.Media.SolidColorBrush(Colors.White);
                txtBox_StatusText.Text = "Please enter the installation path for " + newItemGame.DisplayValue + "!";

                return;
            }

            
            /* Check whether the entered path is a valid installation directory */
            bool pathIsValid = (
                (Directory.Exists(txt_InstallDir.Text)) &&
                (Directory.GetFiles(txt_InstallDir.Text, GameMeta.GetDescription(newItemGame.Value).Replace(' ', '*') + ".exe").Length > 0) &&
                (Directory.GetFiles(txt_InstallDir.Text, "EvochronData.evo").Length > 0)
            );

            if (pathIsValid)
            {
                // Entered path does not exist
                txtBox_StatusText.Foreground = new System.Windows.Media.SolidColorBrush(Color.FromRgb(0, 240, 0));
                txtBox_StatusText.Text = "Your installation path for " + newItemGame.DisplayValue + " is valid!";
            }
            else
            {
                // Path exists
                txtBox_StatusText.Foreground = new System.Windows.Media.SolidColorBrush(Color.FromRgb(220, 0, 0));
                txtBox_StatusText.Text = "Error: The path given is not a valid " + newItemGame.DisplayValue + " installation directory!";
            }
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
                GameEntry currGame = (GameEntry)comBox_GameSelection.SelectedItem;
                bool isCompatible = ((currGame.Value & plugin.CompatibilityFlags) == currGame.Value);
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
    }
}
