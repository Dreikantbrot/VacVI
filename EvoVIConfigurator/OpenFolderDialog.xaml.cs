using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace EvoVIConfigurator
{
    /// <summary>
    /// Interaktionslogik für OpenFolderDialog.xaml
    /// </summary>
    public partial class OpenFolderDialog : Window
    {
        #region Constants
        readonly Image CD_ICON;
        readonly Image HDD_ICON;
        readonly Image DIR_ICON;
        #endregion


        #region Variables
        private string _rootFolder = "%HOMEPATH%";
        private string _selectedFolder = "";
        private string _startFolder = "";
        private bool _displayHiddenFolders = false;
        private bool _selectedFolderConfirmed = false;
        #endregion


        #region Properties
        public string RootFolder
        {
            get { return _rootFolder; }
            set { _rootFolder = value; }
        }

        public string SelectedFolder
        {
            get { return _selectedFolder; }
            set { _selectedFolder = value; }
        }

        public string StartFolder
        {
            get { return _startFolder; }
            set { _startFolder = value; }
        }

        public bool DisplayHiddenFolders
        {
            get { return _displayHiddenFolders; }
            set { _displayHiddenFolders = value; }
        }
        #endregion

        public OpenFolderDialog()
        {
            InitializeComponent();

            /* Search for available hard drives */
            string drives = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int i = 0; i < drives.Length; i++)
            {
                string filepath = drives[i] + ":\\";
                if (!Directory.Exists(filepath)) { continue; }

                TreeViewItem newBranch = CreateNewNode(filepath);
                if (newBranch != null)
                {
                    tree_FolderTreeView.Items.Add(newBranch);
                    BuildFileSystem(filepath, newBranch);
                }
            }
        }


        #region Function Overrides
        private void OnBranchExpanded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Update the sender's children
            TreeViewItem treeView = ((TreeViewItem)sender);

            for (int i = 0; i < treeView.Items.Count; i++)
            {
                TreeViewItem currChild = ((TreeViewItem)treeView.Items[i]);
                if (currChild.Items.Count == 0) { BuildFileSystem((string)currChild.Tag, currChild); }
            }
        }
        
        
        private void OnBranchSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            // Fill the textbos with the current node path and save the path into _selectedFolder
            TreeViewItem treeViewBranch = ((TreeViewItem)sender);
            txt_PathTextBox.Text = treeViewBranch.Tag.ToString();
            _selectedFolder = txt_PathTextBox.Text;
            e.Handled = true;
        }


        /// <summary> Builds the treeview.
        /// </summary>
        public void BuildFileSystem(string filepath, TreeViewItem currTreeNode)
        {
            DirectoryInfo[] folders;
            DirectoryInfo dirInfo = new DirectoryInfo(filepath);

            try { folders = dirInfo.GetDirectories(); }
            catch (Exception) { return; }

            for (int i = 0; i < folders.Length; i++)
            {
                TreeViewItem newBranch = CreateNewNode(folders[i].FullName);
                if (newBranch != null) { currTreeNode.Items.Add(newBranch); }
            }
        }

        private TreeViewItem CreateNewNode(string filepath)
        {
            DirectoryInfo dir = new DirectoryInfo(filepath);

            // Filter the directory within the filepath
            if (
                (dir.Parent != null) && 
                (
                    ((dir.Attributes & FileAttributes.System) == FileAttributes.System) ||
                    ((dir.Attributes & FileAttributes.Temporary) == FileAttributes.Temporary) ||
                    (!DisplayHiddenFolders && ((dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden))
                )
            )
            { return null; }

            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            Label label1 = new Label();
            label1.Content = (dir.Parent != null) ? "[DIR] " : "[HDD] ";
            label1.Margin = new System.Windows.Thickness(0);
            label1.Padding = new System.Windows.Thickness(0);
            stack.Children.Add(label1);
            
            Label label = new Label();
            label.Content = dir.Name;
            label.Margin = new System.Windows.Thickness(0);
            label.Padding = new System.Windows.Thickness(0);
            stack.Children.Add(label);

            TreeViewItem newBranch = new TreeViewItem();
            newBranch.Header = stack;
            newBranch.Tag = dir.FullName;

            newBranch.Selected += OnBranchSelected;
            newBranch.Expanded += OnBranchExpanded;

            return newBranch;
        }


        private void selectNodeByPath(string filepath, TreeViewItem currNode = null)
        {
            ItemCollection items = (currNode == null) ? tree_FolderTreeView.Items : currNode.Items;
            for (int i = 0; i < items.Count; i++)
            {
                TreeViewItem currItem = (TreeViewItem)items[i];
                string currFilePath = currItem.Tag.ToString().TrimEnd('\\');

                if (currFilePath == filepath)
                {
                    // The current path is the *exact* destination path
                    currItem.IsExpanded = true;
                    currItem.IsSelected = true;
                    currItem.BringIntoView();
                    return;
                }
                else if (
                    (filepath.Contains(currFilePath)) &&
                    // Check the "full" folder path
                    // --> Prevents matches like "D:\steam\steam" with destination filepath "D:\steam\steamapps
                    (currFilePath.Length == filepath.IndexOf('\\', currFilePath.Length - 1))
                )
                {
                    // The current path is *part* of the destination filepath
                    currItem.IsExpanded = true;
                    selectNodeByPath(filepath, currItem);
                    return;
                }
            }
        }
        #endregion

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Search and select the input path, if available
            if (
                (Directory.Exists(txt_PathTextBox.Text)) &&
                (tree_FolderTreeView.SelectedItem == null) ||
                (
                    (tree_FolderTreeView.SelectedItem != null) && 
                    ((string)((TreeViewItem)tree_FolderTreeView.SelectedItem).Tag != txt_PathTextBox.Text)
                )
            )
            { selectNodeByPath(txt_PathTextBox.Text); }
        }

        private void btn_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            _selectedFolderConfirmed = true;
            this.Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            _selectedFolderConfirmed = false;
            this.Close();
        }

        private void OpenFolderDialog_Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TreeViewItem currItem = (TreeViewItem)tree_FolderTreeView.SelectedItem;
            _selectedFolder = (_selectedFolderConfirmed && (currItem != null)) ? currItem.Tag.ToString() : "";
        }

        private void OpenFolderDialog_Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Select start folder
            selectNodeByPath(_startFolder);
        }
    }
}
