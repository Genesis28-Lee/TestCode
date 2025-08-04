public partial class FolderBrowserDialogWindow : Window
{
    public List<string> SelectedPaths { get; private set; } = new();

    public FolderBrowserDialogWindow()
    {
        InitializeComponent();

        if (DataContext is FolderBrowserDialogViewModel vm)
        {
            vm.RequestCloseWithOK += () =>
            {
                DialogResult = true;
                SelectedPaths = vm.SelectedFolders;
                Close();
            };

            vm.RequestCloseWithCancel += () =>
            {
                DialogResult = false;
                Close();
            };
        }
    }
}
