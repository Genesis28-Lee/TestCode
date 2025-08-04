public class FolderBrowserDialogViewModel : INotifyPropertyChanged
{
    public ObservableCollection<FolderItemModel> Folders { get; set; } = new();
    public ObservableCollection<FolderItemModel> RecentFolders { get; set; } = new();

    public NotificationService Notifier { get; }

    public bool ShowNotification { get; set; } = true;

    public List<string> SelectedFolders =>
        Folders.Flatten(f => f.SubFolders)
               .Where(f => f.IsSelected)
               .Select(f => f.Path)
               .ToList();

    public ICommand SelectCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action? RequestCloseWithOK;
    public event Action? RequestCloseWithCancel;

    public FolderBrowserDialogViewModel(NotificationService notifier)
    {
        Notifier = notifier;
        LoadFolders();
        LoadRecent();
        SelectCommand = new RelayCommand(ExecuteSelect);
        CancelCommand = new RelayCommand(() => RequestCloseWithCancel?.Invoke());
    }

    private void ExecuteSelect()
    {
        if (SelectedFolders.Any())
        {
            RecentFolderManager.SaveRecent(SelectedFolders);
            if (ShowNotification)
            {
                foreach (var path in SelectedFolders)
                {
                    Notifier.Notify($"선택된 폴더: {path}", NotificationPriority.Normal);
                }
            }
    
            RequestCloseWithOK?.Invoke();
        }
    }

    private void LoadRecent()
    {
        var recents = RecentFolderManager.LoadRecent();
        RecentFolders.Clear();
        foreach (var path in recents.Where(Directory.Exists))
        {
            RecentFolders.Add(new FolderItemModel { Path = path });
        }
    }

    private void LoadFolders()
    {
        foreach (var drive in DriveInfo.GetDrives())
        {
            var root = new FolderItemModel { Path = drive.RootDirectory.FullName };
            LoadSubDirs(root);
            Folders.Add(root);
        }
    }

    private void LoadSubDirs(FolderItemModel parent)
    {
        try
        {
            foreach (var dir in new DirectoryInfo(parent.Path).GetDirectories())
            {
                var child = new FolderItemModel { Path = dir.FullName };
                parent.SubFolders.Add(child);
            }
        }
        catch { }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
