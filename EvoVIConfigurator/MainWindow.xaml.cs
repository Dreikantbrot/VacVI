using System;
using System.Windows;
using System.IO;
using EvoVI;
using EvoVI.PluginContracts;
using System.Windows.Controls;

namespace EvoVIConfigurator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

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
                string newItemText = ((ComboBoxItem)comBox_GameSelection.SelectedItem).Content.ToString();
                txtBox_StatusText.Visibility = System.Windows.Visibility.Visible;
                txtBox_StatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                txtBox_StatusText.Text = "Please enter the installation path for " + newItemText + "!";
            }
            else if (!Directory.Exists(txt_InstallDir.Text))
            {
                // Path exists
                string newItemText = ((ComboBoxItem)comBox_GameSelection.SelectedItem).Content.ToString();
                txtBox_StatusText.Visibility = System.Windows.Visibility.Visible;
                txtBox_StatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 0, 0));
                txtBox_StatusText.Text = "Error: The path given is not a valid " + newItemText + " installation directory!";
            }
            else
            {
                // Entered path does not exist
                string newItemText = ((ComboBoxItem)comBox_GameSelection.SelectedItem).Content.ToString();
                txtBox_StatusText.Visibility = System.Windows.Visibility.Visible;
                txtBox_StatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 240, 0));
                txtBox_StatusText.Text = "Your installation path for " + newItemText + " is valid!";
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
            }
        }
    }
}
