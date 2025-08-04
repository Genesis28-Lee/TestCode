public enum NotificationPriority
{
    Normal,
    High
}

public class NotificationMessage : INotifyPropertyChanged
{
    public string Message { get; set; } = string.Empty;
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public int Count { get; private set; } = 1;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? Tag { get; set; } // 예: 폴더 경로

    public string DisplayMessage => Count > 1 ? $"{Message} ({Count}회)" : Message;

    public void IncrementCount()
    {
        Count++;
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(nameof(DisplayMessage));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string prop) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
