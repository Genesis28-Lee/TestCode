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
    }
}
