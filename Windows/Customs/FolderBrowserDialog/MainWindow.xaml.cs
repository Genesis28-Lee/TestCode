using System.Windows;
using MyApp.Controls;
using MyApp.Helpers;

namespace MyApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var dialog = new CustomFolderBrowserDialog();
            dialog.FolderSelected += (s, e) =>
            {
                if (!string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    NotificationHelper.ShowFolderSelectedToast(dialog.SelectedPath);
                }

                MainGrid.Children.Clear(); // 다이얼로그 닫기
            };

            MainGrid.Children.Add(dialog);
        }


        private void OpenFolderDialog()
        {
            var dialog = new FolderBrowserDialogWindow
            {
                Owner = this
            };
        
            if (dialog.ShowDialog() == true)
            {
                var selected = dialog.SelectedPaths;
                MessageBox.Show($"선택된 폴더: {string.Join("\n", selected)}");
            }
        }
    }
}
