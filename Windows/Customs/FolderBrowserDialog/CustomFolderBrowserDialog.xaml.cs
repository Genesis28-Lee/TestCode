using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MyApp.Controls
{
    public partial class CustomFolderBrowserDialog : UserControl
    {
        public string? SelectedPath { get; private set; }

        public event EventHandler? FolderSelected;

        public CustomFolderBrowserDialog()
        {
            InitializeComponent();
            LoadDrives();
        }

        private void LoadDrives()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                var rootItem = CreateDirectoryNode(drive.RootDirectory);
                FolderTree.Items.Add(rootItem);
            }
        }

        private TreeViewItem CreateDirectoryNode(DirectoryInfo dir)
        {
            var item = new TreeViewItem
            {
                Header = dir.FullName,
                Tag = dir
            };

            try
            {
                foreach (var sub in dir.GetDirectories())
                {
                    item.Items.Add(CreateDirectoryNode(sub));
                }
            }
            catch { }

            return item;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem is TreeViewItem item)
            {
                if (item.Tag is DirectoryInfo dir)
                    SelectedPath = dir.FullName;
            }

            FolderSelected?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SelectedPath = null;
            FolderSelected?.Invoke(this, EventArgs.Empty);
        }
    }
}
