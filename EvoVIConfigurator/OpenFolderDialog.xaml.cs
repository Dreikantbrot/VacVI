using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EvoVIConfigurator
{
    /// <summary>
    /// Interaktionslogik für OpenFolderDialog.xaml
    /// </summary>
    public partial class OpenFolderDialog : Window
    {
        #region Constants
        const string DEFAULT_START_FOLDER = "";
        #endregion


        #region Variables
        private string _rootFolder = null;
        private string _selectedFolder = "";
        private string _startFolder = DEFAULT_START_FOLDER;
        private bool _displayHiddenFolders = false;
        private bool _selectedFolderConfirmed = false;

        private BitmapImage _openedDirectoryImage = new BitmapImage(new Uri(@"Resources/DirectoryOpened_12x12.png", UriKind.RelativeOrAbsolute));
        private BitmapImage _closedDirectoryImage = new BitmapImage(new Uri(@"Resources/DirectoryClosed_12x12.png", UriKind.RelativeOrAbsolute));
        #endregion


        #region Properties
        /// <summary> Returns or sets the root scan folder.
        /// </summary>
        public string RootFolder
        {
            get { return _rootFolder; }
            set { _rootFolder = value; }
        }


        /// <summary> Returns the folder path selected by the user. 
        /// </summary>
        public string SelectedFolder
        {
            get { return _selectedFolder; }
        }


        /// <summary> Returns or sets the start folder to select when opening the dialog.
        /// </summary>
        public string StartFolder
        {
            get { return _startFolder; }
            set { _startFolder = value; }
        }


        /// <summary> Returns or sets whether hidden folders should also be displayed or not.
        /// </summary>
        public bool DisplayHiddenFolders
        {
            get { return _displayHiddenFolders; }
            set { _displayHiddenFolders = value; }
        }
        #endregion


        #region Constuctor
        public OpenFolderDialog()
        {
            InitializeComponent();

            /* Search for available hard drives */
            string drives = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int i = 0; i < drives.Length; i++)
            {
                string filepath = drives[i] + ":\\";
                if (!Directory.Exists(filepath)) { continue; }

                TreeViewItem newBranch = createNewNode(filepath);
                if (newBranch != null)
                {
                    tree_FolderTreeView.Items.Add(newBranch);
                    addChildrenFromPath(filepath, newBranch);
                }
            }
        }
        #endregion


        #region Events
        /// <summary> Fires when a node in the treeview has been expanded.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void onBranchExpanded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Change icon + update the sender's children
            TreeViewItem treeView = ((TreeViewItem)sender);

            ((Image)((StackPanel)treeView.Header).Children[0]).Source = _openedDirectoryImage;

            for (int i = 0; i < treeView.Items.Count; i++)
            {
                TreeViewItem currChild = ((TreeViewItem)treeView.Items[i]);
                if (currChild.Items.Count == 0) { addChildrenFromPath((string)currChild.Tag, currChild); }
            }
        }

        /// <summary> Fires when a node in the treeview has been collapsed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void onBranchCollapsed(object sender, System.Windows.RoutedEventArgs e)
        {
            // Change icon
            TreeViewItem treeView = ((TreeViewItem)sender);

            ((Image)((StackPanel)treeView.Header).Children[0]).Source = _closedDirectoryImage;
            
            e.Handled = true;
        }


        /// <summary> Fires when a node in the treeview has been selected.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void onBranchSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            // Fill the textbos with the current node path and save the path into _selectedFolder
            TreeViewItem treeViewBranch = ((TreeViewItem)sender);
            txt_PathTextBox.Text = treeViewBranch.Tag.ToString();
            _selectedFolder = txt_PathTextBox.Text;
            e.Handled = true;
        }


        /// <summary> Fires when a node in the treeview has been selected.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void txt_PathTextBox_TextChanged(object sender, TextChangedEventArgs e)
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


        /// <summary> Fires when the "Open Folder"-button has been clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            _selectedFolderConfirmed = true;
            this.Close();
        }


        /// <summary> Fires when the "Cancel"-button has been clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            _selectedFolderConfirmed = false;
            this.Close();
        }


        /// <summary> Fires when the dialog is closing.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The cancel event arguments.</param>
        private void OpenFolderDialog_Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TreeViewItem currItem = (TreeViewItem)tree_FolderTreeView.SelectedItem;
            _selectedFolder = (_selectedFolderConfirmed && (currItem != null)) ? currItem.Tag.ToString() : "";
        }


        /// <summary> Fires when the dialog has been loaded.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void OpenFolderDialog_Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Select start folder, if it exists
            selectNodeByPath(_startFolder);
        }
        #endregion


        #region Functions
        /// <summary> Adds all children nodes for the given node and path.
        /// </summary>
        /// <param name="filepath">The filepath from which to create the child nodes.</param>
        /// <param name="currTreeNode">The node to which to add the children to.</param>
        private void addChildrenFromPath(string filepath, TreeViewItem currTreeNode)
        {
            DirectoryInfo[] folders;
            DirectoryInfo dirInfo = new DirectoryInfo(filepath);

            try { folders = dirInfo.GetDirectories(); }
            catch (Exception) { return; }

            for (int i = 0; i < folders.Length; i++)
            {
                TreeViewItem newBranch = createNewNode(folders[i].FullName);
                if (newBranch != null) { currTreeNode.Items.Add(newBranch); }
            }
        }


        /// <summary> Creates a node for the folder selection treeview, adding all necessary events etc..
        /// </summary>
        /// <param name="filepath">The new node's filepath.</param>
        /// <returns></returns>
        private TreeViewItem createNewNode(string filepath)
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

            Image img = new Image();
            img.Source = _closedDirectoryImage;
            img.Margin = new Thickness(0, 0, 5, 0);
            stack.Children.Add(img);
            
            Label label = new Label();
            label.Content = dir.Name;
            label.Margin = new System.Windows.Thickness(0);
            label.Padding = new System.Windows.Thickness(0);
            stack.Children.Add(label);

            TreeViewItem newBranch = new TreeViewItem();
            newBranch.Header = stack;
            newBranch.Tag = dir.FullName;

            newBranch.Selected += onBranchSelected;
            newBranch.Expanded += onBranchExpanded;
            newBranch.Collapsed += onBranchCollapsed;

            return newBranch;
        }


        /// <summary> Selects the node corresponding to the given filepath.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="currNode">The node from which to start searching the target node.</param>
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
                    currItem.BringIntoView();
                    selectNodeByPath(filepath, currItem);
                    return;
                }
            }

            // Not found - select how far you've come so far
            if (currNode != null)
            {
                currNode.IsExpanded = true;
                currNode.IsSelected = true;
                currNode.BringIntoView();
            }
        }
        #endregion
    }
}
