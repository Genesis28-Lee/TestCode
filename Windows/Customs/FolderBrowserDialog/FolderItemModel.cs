public class FolderItemModel
{
    public string Path { get; set; } = string.Empty;
    public ObservableCollection<FolderItemModel> SubFolders { get; set; } = new();
    public bool IsSelected { get; set; }
}
