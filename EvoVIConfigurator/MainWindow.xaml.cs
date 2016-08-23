using System;
using System.Windows;
using System.IO;
using EvoVI;
using EvoVI.PluginContracts;
using System.Windows.Controls;
using System.Windows.Media;
using EvoVI.Database;

namespace EvoVIConfigurator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

        public MainWindow()
        {
            InitializeComponent();

            comBox_GameSelection.SelectedIndex = 0;

            /* Fill games list */
            comBox_GameSelection.Items.Clear();
            comBox_GameSelection.Items.Add(new GameEntry(GameMeta.SupportedGame.EVOCHRON_MERCENARY));
            comBox_GameSelection.Items.Add(new GameEntry(GameMeta.SupportedGame.EVOCHRON_LEGACY));
            comBox_GameSelection.SelectedIndex = 0;

            /* Fill plugin list */
            PluginLoader.LoadPlugins();
            for (int i = 0; i < PluginLoader.Plugins.Count; i++)
            {
                ComboBoxItem cmbx = new ComboBoxItem();
                cmbx.Content = PluginLoader.Plugins[i].Name;
                comBox_PluginSelection.Items.Add(cmbx);
            }

            if (PluginLoader.Plugins.Count > 0) { comBox_PluginSelection.SelectedIndex = 0; }
        }

        private void verifyInstallDir()
        {
            if (String.IsNullOrWhiteSpace(txt_InstallDir.Text))
            {
                // No path entered
                GameEntry newItemGame = (GameEntry)comBox_GameSelection.SelectedItem;
                txtBox_StatusText.Visibility = System.Windows.Visibility.Visible;
                txtBox_StatusText.Foreground = new System.Windows.Media.SolidColorBrush(Colors.White);
                txtBox_StatusText.Text = "Please enter the installation path for " + newItemGame.DisplayValue + "!";
            }
            else if (!Directory.Exists(txt_InstallDir.Text))
            {
                // Path exists
                GameEntry newItemGame = (GameEntry)comBox_GameSelection.SelectedItem;
                txtBox_StatusText.Visibility = System.Windows.Visibility.Visible;
                txtBox_StatusText.Foreground = new System.Windows.Media.SolidColorBrush(Color.FromRgb(220, 0, 0));
                txtBox_StatusText.Text = "Error: The path given is not a valid " + newItemGame.DisplayValue + " installation directory!";
            }
            else
            {
                // Entered path does not exist
                GameEntry newItemGame = (GameEntry)comBox_GameSelection.SelectedItem;
                txtBox_StatusText.Visibility = System.Windows.Visibility.Visible;
                txtBox_StatusText.Foreground = new System.Windows.Media.SolidColorBrush(Color.FromRgb(0, 240, 0));
                txtBox_StatusText.Text = "Your installation path for " + newItemGame.DisplayValue + " is valid!";
            }
        }

        private void btn_InstallDir_Browse_Click(object sender, RoutedEventArgs e)
        {
            // Open file browse dialog
            OpenFolderDialog folderDialog = new OpenFolderDialog();
            folderDialog.StartFolder = @"C:\sw3dg";
            folderDialog.ShowDialog();
            if (!String.IsNullOrWhiteSpace(folderDialog.SelectedFolder)) { txt_InstallDir.Text = folderDialog.SelectedFolder; }
        }

        private void comBox_GameSelection_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            verifyInstallDir();
        }

        private void txt_InstallDir_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            verifyInstallDir();
        }

        private void comBox_PluginSelection_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Update plugin information
            string newItemText = ((ComboBoxItem)comBox_PluginSelection.SelectedItem).Content.ToString();
            IPlugin plugin = PluginLoader.GetPlugin(newItemText);

            if (plugin != null)
            {
                lbl_PluginName.Text = plugin.Name;
                lbl_PluginVersion.Text = "Version: " + (!String.IsNullOrWhiteSpace(plugin.Version) ? plugin.Version : "N/A");
                lbl_PluginDescription.Text = !String.IsNullOrWhiteSpace(plugin.Description) ? plugin.Description : "N/A";
                lbl_PluginAuthor.Text = !String.IsNullOrWhiteSpace(plugin.Author) ? plugin.Author : "N/A";
                lbl_PluginHomepage.Text = !String.IsNullOrWhiteSpace(plugin.Homepage) ? plugin.Homepage : "N/A";

                // (Un-)Check the compatibility checkbox
                GameEntry currGame = (GameEntry)comBox_GameSelection.SelectedItem;
                chck_PluginsCompatibility.IsChecked = ((currGame.Value & plugin.CompatibilityFlags) == currGame.Value);
                chck_PluginsCompatibility.Foreground = (
                    new System.Windows.Media.SolidColorBrush((chck_PluginsCompatibility.IsChecked == true) ? Color.FromArgb(200, 0, 200, 0) : Color.FromArgb(200, 250, 0, 0))
                );
                chck_PluginsCompatibility.BorderBrush = (
                    new System.Windows.Media.SolidColorBrush((chck_PluginsCompatibility.IsChecked == true) ? Color.FromArgb(200, 0, 200, 0) : Color.FromArgb(200, 250, 0, 0))
                );
                chck_PluginsCompatibility.Background = (
                    new System.Windows.Media.SolidColorBrush((chck_PluginsCompatibility.IsChecked == true) ? Color.FromArgb(128, 0, 200, 0) : Color.FromArgb(128, 250, 0, 0))
                );
                chck_PluginsCompatibility.Content = ((chck_PluginsCompatibility.IsChecked == true) ? "" : "Not ") + "Compatible";
            }
        }
    }
}
