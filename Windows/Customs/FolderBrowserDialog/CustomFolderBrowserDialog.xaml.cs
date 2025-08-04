using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MyApp.Controls
{
    public partial class CustomFolderBrowserDialog : UserControl
    {
        public string? SelectedPath { get; private set; }

        public CustomFolderBrowserDialog()
        {
            InitializeComponent();
            LoadDrives();
        }

        private void LoadDrives()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                var item = CreateDirectoryNode(drive.RootDirectory);
                FolderTree.Items.Add(item);
            }
        }

        private TreeViewItem CreateDirectoryNode(DirectoryInfo directory)
        {
            var item = new TreeViewItem
            {
                Header = directory.FullName,
                Tag = directory,
                IsExpanded = false
            };
            try
            {
                foreach (var subDir in directory.GetDirectories())
                {
                    item.Items.Add(CreateDirectoryNode(subDir));
                }
            }
            catch { /* 권한 오류 무시 */ }

            return item;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem is TreeViewItem selected)
            {
                SelectedPath = (selected.Tag as DirectoryInfo)?.FullName;
                RaiseFolderSelected();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SelectedPath = null;
            RaiseFolderSelected();
        }

        public event EventHandler? FolderSelected;

        private void RaiseFolderSelected()
        {
            FolderSelected?.Invoke(this, EventArgs.Empty);
        }
    }
}
